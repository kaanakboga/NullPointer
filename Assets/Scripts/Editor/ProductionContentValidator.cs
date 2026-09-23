using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NullPointer.Content;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Evidence;
using NullPointer.Interrogation;
using NullPointer.Inspect;
using NullPointer.Journal;
using NullPointer.Memory;
using NullPointer.Progression;
using NullPointer.Runtime;
using NullPointer.SceneFlow;
using NullPointer.Terminal;
using NullPointer.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace NullPointer.Editor
{
    public static class ProductionContentValidator
    {
        public const string CatalogPath = "Assets/Data/CAT_OpeningContent.asset";
        public const string BootstrapScenePath = "Assets/Scenes/Bootstrap/SCN_Bootstrap.unity";
        public const string MainMenuScenePath = "Assets/Scenes/MainMenu/SCN_MainMenu.unity";
        public const string ErenScenePath = "Assets/Scenes/Gameplay/SCN_ErenApartment.unity";
        public const string MertScenePath = "Assets/Scenes/Gameplay/SCN_MertApartment.unity";

        public static readonly string[] ProductionScenePaths =
        {
            BootstrapScenePath,
            MainMenuScenePath,
            ErenScenePath,
            MertScenePath
        };

        private static readonly string[] RequiredOpeningIds =
        {
            "case.mert.suspicious_death",
            "character.eren.vardar",
            "character.mert.ersoy",
            "character.dispatch.sector_7",
            "location.system.main_menu",
            "location.eren.apartment",
            "location.mert.apartment",
            "terminal.eren.dispatch",
            "terminal.mert.personal",
            "dialogue.mert.damaged_recorder",
            "dialogue.ch01.ending_hook",
            "memory.mert.photo_glitch",
            "evidence.mert.photo",
            "evidence.mert.terminal_log_0251",
            "evidence.mert.death_time_0236",
            "evidence.mert.damaged_implant",
            "evidence.mert.door_status",
            "evidence.mert.memory_deletion_0229",
            "evidence.mert.last_call_eren",
            "evidence.eren.call_history_gap",
            "deduction.mert.postmortem_terminal",
            "deduction.mert.suicide_timeline_inconsistent",
            "deduction.mert.locked_room_unreliable",
            "checkpoint.ch01.eren_start",
            "checkpoint.ch01.dispatch_complete",
            "checkpoint.ch01.mert_entrance",
            "checkpoint.ch01.critical_evidence",
            "checkpoint.ch01.first_deduction",
            "objective.ch01.inspect_dispatch",
            "objective.ch01.go_to_mert",
            "objective.ch01.investigate_scene",
            "objective.ch01.trace_photo",
            "objective.ch01.inspect_terminal",
            "objective.ch01.confirm_death_time",
            "objective.ch01.compare_evidence"
        };

        [MenuItem("Null Pointer/Content/Validate Production Content")]
        public static void ValidateMenu()
        {
            ValidateOrThrow();
        }

        public static void ValidateBatch()
        {
            ValidateOrThrow();
        }

        public static void ValidateOrThrow()
        {
            IReadOnlyList<string> errors = Validate();
            if (errors.Count == 0)
            {
                Debug.Log("[Content] Production content validation passed.");
                return;
            }

            foreach (string error in errors)
            {
                Debug.LogError($"[Content] {error}");
            }

            throw new InvalidOperationException(
                $"Production content validation failed with {errors.Count} error(s).");
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            foreach (ContentIdDiagnostic diagnostic in ContentValidationMenu.ValidateAll())
            {
                errors.Add($"{diagnostic.Code}: {diagnostic.Message}");
            }

            ValidateBuildScenes(errors);
            ContentCatalog catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);
            if (catalog == null)
            {
                errors.Add($"Required opening catalog is missing at '{CatalogPath}'.");
                return errors;
            }

            AuthoredContentAsset[] allContent = catalog.AllContent.Where(item => item != null).ToArray();
            if (allContent.Length != catalog.AllContent.Count)
            {
                errors.Add("Opening catalog contains a null authored-content reference.");
            }

            var allIds = new HashSet<string>(allContent.Select(item => item.StableId), StringComparer.Ordinal);
            foreach (string requiredId in RequiredOpeningIds)
            {
                if (!allIds.Contains(requiredId))
                {
                    errors.Add($"Required Chapter 1 content ID '{requiredId}' is missing from the opening catalog.");
                }
            }

            ValidateCatalogMembership(catalog.Evidence, allContent, "evidence", errors);
            ValidateCatalogMembership(catalog.Deductions, allContent, "deduction", errors);
            ValidateCatalogMembership(catalog.Objectives, allContent, "objective", errors);
            ValidateCatalogMembership(catalog.Checkpoints, allContent, "checkpoint", errors);
            ValidateCatalogMembership(catalog.JournalEntries, allContent, "journal", errors);
            ValidateCatalogMembership(catalog.InterrogationClaims, allContent, "interrogation claim", errors);

            var evidenceIds = new HashSet<string>(catalog.Evidence.Where(item => item != null).Select(item => item.StableId), StringComparer.Ordinal);
            var deductionIds = new HashSet<string>(catalog.Deductions.Where(item => item != null).Select(item => item.StableId), StringComparer.Ordinal);
            ValidateDeductions(catalog.Deductions, evidenceIds, deductionIds, errors);
            ValidateCheckpoints(catalog.Checkpoints, allIds, errors);
            ValidateJournal(catalog.JournalEntries, evidenceIds, deductionIds, errors);
            ValidateInterrogations(catalog.InterrogationClaims, evidenceIds, errors);
            ValidateTerminals(allContent.OfType<TerminalData>(), evidenceIds, errors);
            ValidateDialogues(allContent.OfType<DialogueData>(), allIds, evidenceIds, errors);
            ValidateLocations(allContent.OfType<LocationData>(), errors);
            ValidateProductionSceneLinks(catalog, errors);
            return errors;
        }

        private static void ValidateBuildScenes(ICollection<string> errors)
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path.Replace('\\', '/'))
                .ToArray();
            if (!enabledScenes.SequenceEqual(ProductionScenePaths))
            {
                errors.Add(
                    "Enabled production scenes must be exactly Bootstrap, Main Menu, Eren Apartment, and Mert Apartment in that order.");
            }

            foreach (string path in ProductionScenePaths)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
                {
                    errors.Add($"Required production scene is missing at '{path}'.");
                }
            }

            if (enabledScenes.Any(path => path.IndexOf("/Test/", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                errors.Add("An engineering/test scene is enabled for the production build.");
            }
        }

        private static void ValidateCatalogMembership<T>(
            IEnumerable<T> typedItems,
            IReadOnlyCollection<AuthoredContentAsset> allContent,
            string label,
            ICollection<string> errors)
            where T : AuthoredContentAsset
        {
            foreach (T item in typedItems ?? Array.Empty<T>())
            {
                if (item == null)
                {
                    errors.Add($"Opening catalog contains a null {label} reference.");
                }
                else if (!allContent.Contains(item))
                {
                    errors.Add($"{label} '{item.name}' is not registered in the catalog's all-content list.");
                }
            }
        }

        private static void ValidateDeductions(
            IEnumerable<DeductionData> deductions,
            ISet<string> evidenceIds,
            ISet<string> deductionIds,
            ICollection<string> errors)
        {
            foreach (DeductionData deduction in deductions.Where(item => item != null))
            {
                ValidateIds(deduction.RequiredEvidenceIds, evidenceIds, deduction.StableId, "evidence", errors);
                ValidateIds(deduction.RequiredDeductionIds, deductionIds, deduction.StableId, "deduction", errors);
                if (!string.IsNullOrWhiteSpace(deduction.ResultingEvidenceId) &&
                    !evidenceIds.Contains(deduction.ResultingEvidenceId))
                {
                    errors.Add($"Deduction '{deduction.StableId}' references missing result evidence '{deduction.ResultingEvidenceId}'.");
                }
            }
        }

        private static void ValidateCheckpoints(
            IEnumerable<CheckpointData> checkpoints,
            ISet<string> allIds,
            ICollection<string> errors)
        {
            foreach (CheckpointData checkpoint in checkpoints.Where(item => item != null))
            {
                if (checkpoint.Location == null || !allIds.Contains(checkpoint.Location.StableId))
                {
                    errors.Add($"Checkpoint '{checkpoint.StableId}' has no registered location.");
                }

                if (string.IsNullOrWhiteSpace(checkpoint.SpawnPointId))
                {
                    errors.Add($"Checkpoint '{checkpoint.StableId}' has no spawn-point ID.");
                }
            }
        }

        private static void ValidateJournal(
            IEnumerable<JournalEntryData> entries,
            ISet<string> evidenceIds,
            ISet<string> deductionIds,
            ICollection<string> errors)
        {
            foreach (JournalEntryData entry in entries.Where(item => item != null))
            {
                if (!string.IsNullOrWhiteSpace(entry.RequiredEvidenceId) && !evidenceIds.Contains(entry.RequiredEvidenceId))
                {
                    errors.Add($"Journal entry '{entry.StableId}' references missing evidence '{entry.RequiredEvidenceId}'.");
                }

                if (!string.IsNullOrWhiteSpace(entry.RequiredDeductionId) && !deductionIds.Contains(entry.RequiredDeductionId))
                {
                    errors.Add($"Journal entry '{entry.StableId}' references missing deduction '{entry.RequiredDeductionId}'.");
                }
            }
        }

        private static void ValidateInterrogations(
            IEnumerable<InterrogationClaimData> claims,
            ISet<string> evidenceIds,
            ICollection<string> errors)
        {
            foreach (InterrogationClaimData claim in claims.Where(item => item != null))
            {
                ValidateIds(claim.ContradictingEvidenceIds, evidenceIds, claim.StableId, "evidence", errors);
            }
        }

        private static void ValidateTerminals(
            IEnumerable<TerminalData> terminals,
            ISet<string> evidenceIds,
            ICollection<string> errors)
        {
            foreach (TerminalData terminal in terminals)
            {
                var entryIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (TerminalEntry entry in terminal.Entries)
                {
                    if (entry == null || string.IsNullOrWhiteSpace(entry.EntryId))
                    {
                        errors.Add($"Terminal '{terminal.StableId}' contains an entry without a stable entry ID.");
                        continue;
                    }

                    if (!entryIds.Add(entry.EntryId))
                    {
                        errors.Add($"Terminal '{terminal.StableId}' contains duplicate entry ID '{entry.EntryId}'.");
                    }

                    if (!string.IsNullOrWhiteSpace(entry.EvidenceId) && !evidenceIds.Contains(entry.EvidenceId))
                    {
                        errors.Add($"Terminal entry '{entry.EntryId}' references missing evidence '{entry.EvidenceId}'.");
                    }
                }
            }
        }

        private static void ValidateDialogues(
            IEnumerable<DialogueData> dialogues,
            ISet<string> allIds,
            ISet<string> evidenceIds,
            ICollection<string> errors)
        {
            foreach (DialogueData dialogue in dialogues)
            {
                var nodeIds = new HashSet<string>(StringComparer.Ordinal);
                foreach (DialogueNode node in dialogue.Nodes)
                {
                    if (node == null || string.IsNullOrWhiteSpace(node.NodeId) || !nodeIds.Add(node.NodeId))
                    {
                        errors.Add($"Dialogue '{dialogue.StableId}' contains a missing or duplicate node ID.");
                    }

                    if (node?.Speaker == null || !allIds.Contains(node.Speaker.StableId))
                    {
                        errors.Add($"Dialogue '{dialogue.StableId}' node '{node?.NodeId}' has no registered speaker.");
                    }
                }

                if (string.IsNullOrWhiteSpace(dialogue.StartNodeId) || !nodeIds.Contains(dialogue.StartNodeId))
                {
                    errors.Add($"Dialogue '{dialogue.StableId}' has an invalid start node '{dialogue.StartNodeId}'.");
                }

                foreach (DialogueNode node in dialogue.Nodes.Where(item => item != null))
                {
                    ValidateNodeLink(dialogue, node.NodeId, node.NextNodeId, nodeIds, errors);
                    foreach (DialogueChoice choice in node.Choices)
                    {
                        if (choice == null)
                        {
                            errors.Add($"Dialogue '{dialogue.StableId}' node '{node.NodeId}' contains a null choice.");
                            continue;
                        }

                        ValidateNodeLink(dialogue, node.NodeId, choice.NextNodeId, nodeIds, errors);
                        if (!string.IsNullOrWhiteSpace(choice.RequiredEvidenceId) &&
                            !evidenceIds.Contains(choice.RequiredEvidenceId))
                        {
                            errors.Add(
                                $"Dialogue '{dialogue.StableId}' choice references missing evidence '{choice.RequiredEvidenceId}'.");
                        }
                    }
                }
            }
        }

        private static void ValidateNodeLink(
            DialogueData dialogue,
            string nodeId,
            string nextNodeId,
            ISet<string> nodeIds,
            ICollection<string> errors)
        {
            if (!string.IsNullOrWhiteSpace(nextNodeId) && !nodeIds.Contains(nextNodeId))
            {
                errors.Add($"Dialogue '{dialogue.StableId}' node '{nodeId}' links to missing node '{nextNodeId}'.");
            }
        }

        private static void ValidateLocations(IEnumerable<LocationData> locations, ICollection<string> errors)
        {
            var sceneNames = new HashSet<string>(
                ProductionScenePaths.Select(Path.GetFileNameWithoutExtension),
                StringComparer.Ordinal);
            foreach (LocationData location in locations)
            {
                if (string.IsNullOrWhiteSpace(location.SceneName) || !sceneNames.Contains(location.SceneName))
                {
                    errors.Add($"Location '{location.StableId}' references missing production scene '{location.SceneName}'.");
                }
            }
        }

        private static void ValidateProductionSceneLinks(ContentCatalog catalog, ICollection<string> errors)
        {
            var obtainableEvidenceIds = new HashSet<string>(StringComparer.Ordinal);
            ValidateBootstrapScene(errors);
            ValidateMainMenuScene(errors);
            ValidateGameplayScene(ErenScenePath, obtainableEvidenceIds, errors);
            ValidateGameplayScene(MertScenePath, obtainableEvidenceIds, errors);

            IEnumerable<string> requiredEvidence = catalog.Deductions
                .Where(item => item != null)
                .SelectMany(item => item.RequiredEvidenceIds)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal);
            foreach (string evidenceId in requiredEvidence)
            {
                if (!obtainableEvidenceIds.Contains(evidenceId))
                {
                    errors.Add($"Required deduction evidence '{evidenceId}' has no production-scene acquisition route.");
                }
            }
        }

        private static void ValidateBootstrapScene(ICollection<string> errors)
        {
            WithScene(BootstrapScenePath, roots =>
            {
                RequireReferences<GameApplication>(
                    roots,
                    BootstrapScenePath,
                    errors,
                    "_gameModes",
                    "_inputReader",
                    "_pauseHandler",
                    "_sceneLoader",
                    "_contentCatalog",
                    "_startingLocation",
                    "_mainMenuLocation",
                    "_startingCheckpoint");
            }, errors);
        }

        private static void ValidateMainMenuScene(ICollection<string> errors)
        {
            WithScene(MainMenuScenePath, roots =>
            {
                RequireReferences<MainMenuSceneInstaller>(roots, MainMenuScenePath, errors, "_controller");
                ValidateSingleSceneInfrastructure(roots, MainMenuScenePath, errors);
            }, errors);
        }

        private static void ValidateGameplayScene(
            string scenePath,
            ISet<string> obtainableEvidenceIds,
            ICollection<string> errors)
        {
            WithScene(scenePath, roots =>
            {
                RequireReferences<OpeningSceneInstaller>(
                    roots,
                    scenePath,
                    errors,
                    "_location",
                    "_player",
                    "_interactionDetector",
                    "_inspectController",
                    "_dialogueController",
                    "_terminalController",
                    "_memoryController",
                    "_evidenceBoardController",
                    "_interactionPrompt",
                    "_evidenceNotification",
                    "_pauseMenuController",
                    "_objectivePresenter",
                    "_progressionCoordinator",
                    "_audioHooks");
                ValidateSingleSceneInfrastructure(roots, scenePath, errors);

                foreach (EvidenceInteractable interactable in FindAll<EvidenceInteractable>(roots))
                {
                    if (interactable.Inspection == null || interactable.Evidence == null)
                    {
                        errors.Add($"Scene '{scenePath}' has an EvidenceInteractable with a broken authored reference.");
                    }
                    else
                    {
                        obtainableEvidenceIds.Add(interactable.Evidence.StableId);
                    }
                }

                foreach (TerminalInteractable interactable in FindAll<TerminalInteractable>(roots))
                {
                    if (interactable.Terminal == null)
                    {
                        errors.Add($"Scene '{scenePath}' has a TerminalInteractable without TerminalData.");
                        continue;
                    }

                    foreach (TerminalEntry entry in interactable.Terminal.Entries.Where(item => item != null))
                    {
                        if (!string.IsNullOrWhiteSpace(entry.EvidenceId))
                        {
                            obtainableEvidenceIds.Add(entry.EvidenceId);
                        }
                    }
                }

                foreach (DialogueInteractable interactable in FindAll<DialogueInteractable>(roots))
                {
                    if (interactable.Dialogue == null)
                    {
                        errors.Add($"Scene '{scenePath}' has a DialogueInteractable without DialogueData.");
                    }
                }

                foreach (MemoryEvidenceTrigger trigger in FindAll<MemoryEvidenceTrigger>(roots))
                {
                    if (trigger.Memory == null)
                    {
                        errors.Add($"Scene '{scenePath}' has a memory trigger without MemoryData.");
                    }
                }

                foreach (SceneTransitionInteractable transition in FindAll<SceneTransitionInteractable>(roots))
                {
                    if (transition.TargetLocation == null || string.IsNullOrWhiteSpace(transition.TargetSpawnPointId))
                    {
                        errors.Add($"Scene '{scenePath}' has a transition with a broken target location or spawn ID.");
                    }
                }

                if (string.Equals(scenePath, ErenScenePath, StringComparison.Ordinal))
                {
                    bool dispatchGate = FindAll<SceneTransitionInteractable>(roots).Any(transition =>
                        string.Equals(
                            transition.RequiredStoryFlag,
                            "flag.dispatch.mert_assignment_received",
                            StringComparison.Ordinal));
                    if (!dispatchGate)
                    {
                        errors.Add("Eren Apartment has no dispatch-gated transition to the investigation.");
                    }
                }
                else if (string.Equals(scenePath, MertScenePath, StringComparison.Ordinal))
                {
                    RequireReferences<ChapterEndSequenceController>(
                        roots,
                        scenePath,
                        errors,
                        "_dialogue",
                        "_dialogueController");
                }
            }, errors);
        }

        private static void ValidateSingleSceneInfrastructure(
            GameObject[] roots,
            string scenePath,
            ICollection<string> errors)
        {
            if (FindAll<EventSystem>(roots).Length != 1)
            {
                errors.Add($"Scene '{scenePath}' must contain exactly one EventSystem.");
            }

            if (FindAll<AudioListener>(roots).Length != 1)
            {
                errors.Add($"Scene '{scenePath}' must contain exactly one AudioListener.");
            }

            VisualRootAnchor[] visualRoots = FindAll<VisualRootAnchor>(roots);
            if (visualRoots.Length != 1 ||
                string.IsNullOrWhiteSpace(visualRoots[0].StableId) ||
                visualRoots[0].Theme == null)
            {
                errors.Add($"Scene '{scenePath}' must contain exactly one configured VisualRootAnchor.");
            }

            FinalArtSlot[] artSlots = FindAll<FinalArtSlot>(roots);
            if (artSlots.Length == 0)
            {
                errors.Add($"Scene '{scenePath}' must contain at least one FinalArtSlot.");
            }

            foreach (IGrouping<string, FinalArtSlot> duplicate in artSlots
                         .Where(slot => slot != null)
                         .GroupBy(slot => slot.StableId, StringComparer.Ordinal)
                         .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() > 1))
            {
                errors.Add($"Scene '{scenePath}' has an empty or duplicate final-art slot ID '{duplicate.Key}'.");
            }

            foreach (FinalArtSlot slot in artSlots.Where(slot =>
                         slot != null && string.IsNullOrWhiteSpace(slot.ManifestAssetId)))
            {
                errors.Add($"Scene '{scenePath}' final-art slot '{slot.StableId}' has no manifest asset ID.");
            }
        }

        private static void RequireReferences<T>(
            GameObject[] roots,
            string scenePath,
            ICollection<string> errors,
            params string[] propertyNames)
            where T : Component
        {
            T[] components = FindAll<T>(roots);
            if (components.Length != 1)
            {
                errors.Add($"Scene '{scenePath}' must contain exactly one {typeof(T).Name}.");
                return;
            }

            var serialized = new SerializedObject(components[0]);
            foreach (string propertyName in propertyNames)
            {
                SerializedProperty property = serialized.FindProperty(propertyName);
                if (property == null || property.objectReferenceValue == null)
                {
                    errors.Add(
                        $"Scene '{scenePath}' {typeof(T).Name} is missing required reference '{propertyName}'.");
                }
            }
        }

        private static T[] FindAll<T>(IEnumerable<GameObject> roots) where T : Component
        {
            return roots.SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        }

        private static void WithScene(
            string scenePath,
            Action<GameObject[]> validation,
            ICollection<string> errors)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForValidation = !scene.IsValid() || !scene.isLoaded;
            try
            {
                if (openedForValidation)
                {
                    scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                }

                GameObject[] roots = scene.GetRootGameObjects();
                foreach (GameObject root in roots)
                {
                    int missingScripts = root.GetComponentsInChildren<Transform>(true)
                        .Sum(transform => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject));
                    if (missingScripts > 0)
                    {
                        errors.Add($"Scene '{scenePath}' contains {missingScripts} missing script reference(s) under '{root.name}'.");
                    }
                }

                validation(roots);
            }
            catch (Exception exception)
            {
                errors.Add($"Scene '{scenePath}' could not be validated: {exception.Message}");
            }
            finally
            {
                if (openedForValidation && scene.IsValid() && scene.isLoaded)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        private static void ValidateIds(
            IEnumerable<string> ids,
            ISet<string> knownIds,
            string ownerId,
            string label,
            ICollection<string> errors)
        {
            foreach (string id in ids ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(id) || !knownIds.Contains(id))
                {
                    errors.Add($"'{ownerId}' references missing {label} '{id}'.");
                }
            }
        }
    }
}
