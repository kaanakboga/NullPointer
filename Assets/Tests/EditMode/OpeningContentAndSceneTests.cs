using System.Linq;
using NullPointer.Content;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Inspect;
using NullPointer.Interaction;
using NullPointer.Memory;
using NullPointer.Menus;
using NullPointer.Player;
using NullPointer.Runtime;
using NullPointer.SceneFlow;
using NullPointer.Terminal;
using NullPointer.Visual;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.Tests.EditMode
{
    public sealed class OpeningContentAndSceneTests
    {
        private const string CatalogPath = "Assets/Data/CAT_OpeningContent.asset";
        private const string BootstrapPath = "Assets/Scenes/Bootstrap/SCN_Bootstrap.unity";
        private const string MainMenuPath = "Assets/Scenes/MainMenu/SCN_MainMenu.unity";
        private const string ErenPath = "Assets/Scenes/Gameplay/SCN_ErenApartment.unity";
        private const string MertPath = "Assets/Scenes/Gameplay/SCN_MertApartment.unity";

        [Test]
        public void OpeningCatalog_ContainsValidUniqueIdsAndRequiredClueChain()
        {
            ContentCatalog catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);

            Assert.That(catalog, Is.Not.Null);
            Assert.That(ContentIdValidator.Validate(catalog.AllContent), Is.Empty);
            Assert.That(catalog.Evidence.Select(item => item.StableId), Does.Contain(
                "evidence.mert.photo"));
            Assert.That(catalog.Evidence.Select(item => item.StableId), Does.Contain(
                "evidence.mert.terminal_log_0251"));
            Assert.That(catalog.Evidence.Select(item => item.StableId), Does.Contain(
                "evidence.mert.death_time_0236"));

            DeductionData deduction = catalog.Deductions.Single(item =>
                item.StableId == "deduction.mert.postmortem_terminal");
            Assert.That(deduction.RequiredEvidenceIds, Is.EquivalentTo(new[]
            {
                "evidence.mert.terminal_log_0251",
                "evidence.mert.death_time_0236"
            }));
            Assert.That(catalog.Deductions.Count, Is.EqualTo(3));

            DialogueData recorder = AssetDatabase.LoadAssetAtPath<DialogueData>(
                "Assets/Data/Dialogue/DLG_MertDamagedRecorder.asset");
            Assert.That(recorder.Nodes.First().Speaker.StableId, Is.EqualTo("character.mert.ersoy"));
        }

        [Test]
        public void ProductionBuildSettings_StartAtBootstrapAndExcludeEngineeringScene()
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Assert.That(enabledScenes, Is.EqualTo(new[] { BootstrapPath, MainMenuPath, ErenPath, MertPath }));
            Assert.That(enabledScenes.Any(path => path.Contains("/Test/")), Is.False);
        }

        [Test]
        public void BootstrapScene_ContainsCompositionRoot()
        {
            WithScene(BootstrapPath, roots =>
            {
                Assert.That(FindInRoots<GameApplication>(roots), Is.Not.Null);
                Assert.That(FindInRoots<SceneLoader>(roots), Is.Not.Null);
            });
        }

        [Test]
        public void MainMenuScene_ContainsFunctionalMenuAndInstaller()
        {
            WithScene(MainMenuPath, roots =>
            {
                Assert.That(FindInRoots<MainMenuController>(roots), Is.Not.Null);
                Assert.That(FindInRoots<MainMenuSceneInstaller>(roots), Is.Not.Null);
                Assert.That(FindInRoots<SettingsPanel>(roots), Is.Not.Null);
            });
        }

        [Test]
        public void Phase5BMenus_ContainProductionPresentationAndReplacementSlots()
        {
            WithScene(MainMenuPath, roots =>
            {
                Assert.That(FindInRoots<MainMenuPresentation>(roots), Is.Not.Null);
                Assert.That(FindInRoots<MenuAtmosphereController>(roots), Is.Not.Null);
                Assert.That(FindAllInRoots<CyberNoirButtonVisual>(roots).Length, Is.GreaterThanOrEqualTo(6));
                Assert.That(FindAllInRoots<CyberNoirSliderVisual>(roots).Length, Is.EqualTo(4));
                Assert.That(FindAllInRoots<CyberNoirToggleVisual>(roots).Length, Is.EqualTo(3));
                Assert.That(FindAllInRoots<CyberNoirDropdownVisual>(roots).Length, Is.EqualTo(1));
                string[] slots = FindAllInRoots<FinalArtSlot>(roots)
                    .Select(slot => slot.ManifestAssetId)
                    .ToArray();
                Assert.That(slots, Does.Contain("NP-MENU-BG-FAR-001"));
                Assert.That(slots, Does.Contain("NP-MENU-BG-MID-001"));
                Assert.That(slots, Does.Contain("NP-MENU-BG-NEAR-001"));
                Assert.That(slots, Does.Contain("NP-MENU-MOTIF-001"));
                Assert.That(slots, Does.Contain("NP-MENU-FX-FOG-001"));
                Assert.That(slots, Does.Contain("NP-MENU-FX-NOISE-001"));
                Assert.That(slots, Does.Not.Contain("NP-MENU-FX-RAIN-001"),
                    "Main-menu rain is a Unity-procedural visual, not an external raster slot.");
                RectTransform subtitle = FindAllInRoots<RectTransform>(roots)
                    .Single(rect => rect.name == "Subtitle");
                RectTransform settingsHeading = FindInRoots<SettingsPanel>(roots).transform
                    .Find("Settings Frame/Heading") as RectTransform;
                Assert.That(subtitle.pivot.x, Is.EqualTo(0f), "Left-safe subtitle must not extend outside its anchor.");
                Assert.That(settingsHeading, Is.Not.Null);
                Assert.That(settingsHeading.pivot.x, Is.EqualTo(0f), "Settings heading must remain inside the modal.");
                SettingsPanel settingsPanel = FindInRoots<SettingsPanel>(roots);
                RectTransform apply = settingsPanel.transform.Find("Settings Frame/Settings Content/Apply") as RectTransform;
                RectTransform back = settingsPanel.transform.Find("Settings Frame/Settings Content/Back") as RectTransform;
                Assert.That(apply, Is.Not.Null);
                Assert.That(back, Is.Not.Null);
                Assert.That(apply.anchoredPosition.y, Is.GreaterThanOrEqualTo(0f));
                Assert.That(back.anchoredPosition.y, Is.GreaterThanOrEqualTo(0f));
            });

            WithScene(ErenPath, roots =>
            {
                PauseMenuPanel pause = FindInRoots<PauseMenuPanel>(roots);
                Assert.That(pause, Is.Not.Null);
                Assert.That(pause.GetComponent<UiTransition>(), Is.Not.Null);
                Assert.That(pause.GetComponentsInChildren<CyberNoirButtonVisual>(true).Length, Is.EqualTo(4));
                RectTransform heading = pause.transform.Find("Pause Frame/Heading") as RectTransform;
                Assert.That(heading, Is.Not.Null);
                Assert.That(heading.pivot.x, Is.EqualTo(0f), "Pause heading must remain inside the viewport.");
            });
        }

        [TestCase(ErenPath)]
        [TestCase(MertPath)]
        public void ProductionInteractables_DoNotExposeDebugMarkerRenderers(string scenePath)
        {
            WithScene(scenePath, roots =>
            {
                SpriteRenderer[] markerRenderers = FindAllInRoots<MonoBehaviour>(roots)
                    .Where(component => component is IInteractable)
                    .Select(component => component.GetComponent<SpriteRenderer>())
                    .Where(renderer => renderer != null)
                    .Distinct()
                    .ToArray();

                Assert.That(markerRenderers, Is.Not.Empty);
                Assert.That(markerRenderers.All(renderer =>
                {
                    FinalArtSlot slot = renderer.GetComponent<FinalArtSlot>();
                    return slot == null || slot.HasFinalArt || !renderer.enabled;
                }), Is.True, "Production interactable anchors without approved art must render invisibly.");
            });
        }

        [Test]
        public void ChapterOneCatalog_ContainsAuthoredObjectivesCheckpointsAndJournal()
        {
            ContentCatalog catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);

            Assert.That(catalog.Objectives.Count, Is.EqualTo(7));
            Assert.That(catalog.Checkpoints.Count, Is.EqualTo(5));
            Assert.That(catalog.JournalEntries.Count, Is.GreaterThanOrEqualTo(12));
            Assert.That(catalog.InterrogationClaims.Count, Is.EqualTo(1));
        }

        [Test]
        public void ErenApartment_ContainsPlayerInspectDispatchAndGatedExit()
        {
            WithScene(ErenPath, roots =>
            {
                Assert.That(FindInRoots<PlayerController>(roots), Is.Not.Null);
                Assert.That(FindAllInRoots<InspectInteractable>(roots).Length, Is.GreaterThanOrEqualTo(2));
                Assert.That(FindInRoots<TerminalInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<SceneTransitionInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<OpeningSceneInstaller>(roots), Is.Not.Null);
            });
        }

        [Test]
        public void MertApartment_ContainsEvidenceBoardMemoryTerminalAndDialogue()
        {
            WithScene(MertPath, roots =>
            {
                Assert.That(FindAllInRoots<EvidenceInteractable>(roots).Length, Is.GreaterThanOrEqualTo(4));
                Assert.That(FindInRoots<EvidenceBoardInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<MemoryEvidenceTrigger>(roots), Is.Not.Null);
                Assert.That(FindInRoots<TerminalInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<DialogueInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<ProgressionCoordinator>(roots), Is.Not.Null);
                Assert.That(FindInRoots<PauseMenuController>(roots), Is.Not.Null);
            });
        }

        private static void WithScene(string path, System.Action<GameObject[]> assertion)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            Assert.That(sceneAsset, Is.Not.Null, $"Production scene is missing at {path}.");
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                assertion(scene.GetRootGameObjects());
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T FindInRoots<T>(GameObject[] roots) where T : Component
        {
            return roots.Select(root => root.GetComponentInChildren<T>(true)).FirstOrDefault(item => item != null);
        }

        private static T[] FindAllInRoots<T>(GameObject[] roots) where T : Component
        {
            return roots.SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        }
    }
}
