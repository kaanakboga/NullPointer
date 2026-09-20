using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Evidence;
using NullPointer.Input;
using NullPointer.Inspect;
using NullPointer.Interaction;
using NullPointer.Memory;
using NullPointer.Player;
using NullPointer.Runtime;
using NullPointer.SceneFlow;
using NullPointer.Terminal;
using NullPointer.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace NullPointer.Editor
{
    public static class OpeningContentBuilder
    {
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";
        private const string BootstrapScenePath = "Assets/Scenes/Bootstrap/SCN_Bootstrap.unity";
        private const string ErenScenePath = "Assets/Scenes/Gameplay/SCN_ErenApartment.unity";
        private const string MertScenePath = "Assets/Scenes/Gameplay/SCN_MertApartment.unity";
        private const string DispatchReceivedFlag = "flag.dispatch.mert_assignment_received";

        private static readonly Color BackgroundColor = new Color(0.018f, 0.027f, 0.047f, 1f);
        private static readonly Color PanelColor = new Color(0.025f, 0.055f, 0.075f, 0.97f);
        private static readonly Color Cyan = new Color(0.24f, 0.86f, 0.92f, 1f);
        private static readonly Color Amber = new Color(0.96f, 0.62f, 0.2f, 1f);
        private static readonly Color Red = new Color(0.88f, 0.18f, 0.24f, 1f);
        private static readonly Color TextColor = new Color(0.86f, 0.93f, 0.94f, 1f);

        [MenuItem("Null Pointer/Opening/Rebuild Authored Opening Content and Scenes")]
        public static void RebuildAll()
        {
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                throw new InvalidOperationException($"Input action asset not found at {InputActionsPath}.");
            }

            OpeningAssets assets = CreateOrUpdateAssets();
            BuildBootstrapScene(assets, inputActions);
            BuildErenApartmentScene(assets);
            BuildMertApartmentScene(assets);
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            IReadOnlyList<ContentIdDiagnostic> diagnostics = ContentValidationMenu.ValidateAll();
            if (diagnostics.Count > 0)
            {
                throw new InvalidOperationException(string.Join(
                    Environment.NewLine,
                    diagnostics.Select(diagnostic => diagnostic.Message)));
            }

            Debug.Log("[Opening] Authored opening content and production scenes rebuilt successfully.");
        }

        public static void RebuildFromCommandLine()
        {
            RebuildAll();
        }

        private static OpeningAssets CreateOrUpdateAssets()
        {
            EnsureFolder("Assets/Data/Deductions");
            EnsureFolder("Assets/Data/Inspections");
            EnsureFolder("Assets/Data/Terminals");

            var assets = new OpeningAssets
            {
                Case = GetOrCreate<CaseData>("Assets/Data/Cases/CASE_MertSuspiciousDeath.asset"),
                Eren = GetOrCreate<CharacterData>("Assets/Data/Characters/CHAR_ErenVardar.asset"),
                Mert = GetOrCreate<CharacterData>("Assets/Data/Characters/CHAR_MertErsoy.asset"),
                Dispatch = GetOrCreate<CharacterData>("Assets/Data/Characters/CHAR_Sector7Dispatch.asset"),
                ErenLocation = GetOrCreate<LocationData>("Assets/Data/Locations/LOC_ErenApartment.asset"),
                MertLocation = GetOrCreate<LocationData>("Assets/Data/Locations/LOC_MertApartment.asset"),
                MedicationInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_ErenMedication.asset"),
                ForeshadowInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_ErenPhotograph.asset"),
                MertPhotoInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertPhotograph.asset"),
                DeathTimeInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertDeathTime.asset"),
                MertPhotoEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_PHOTO.asset"),
                MertTerminalEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_TERMINAL_LOG.asset"),
                MertDeathTimeEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_DEATH_TIME.asset"),
                PostmortemDeduction = GetOrCreate<DeductionData>(
                    "Assets/Data/Deductions/DED_MERT_POSTMORTEM_TERMINAL.asset"),
                DispatchTerminal = GetOrCreate<TerminalData>("Assets/Data/Terminals/TERM_ErenDispatch.asset"),
                MertTerminal = GetOrCreate<TerminalData>("Assets/Data/Terminals/TERM_MertPersonal.asset"),
                PhotoMemory = GetOrCreate<MemoryData>("Assets/Data/Memories/MEM_MertPhotoGlitch.asset"),
                MertRecorderDialogue = GetOrCreate<DialogueData>(
                    "Assets/Data/Dialogue/DLG_MertDamagedRecorder.asset"),
                Catalog = GetOrCreate<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset")
            };

            ConfigureCase(assets.Case);
            ConfigureCharacter(assets.Eren, "character.eren.vardar", "Eren Vardar", "Soruşturmacı");
            ConfigureCharacter(assets.Mert, "character.mert.ersoy", "Mert Ersoy", "Mnemosyne Sistem Mühendisi");
            ConfigureCharacter(assets.Dispatch, "character.dispatch.sector_7", "Sektör 7 Sevk", "Operatör");
            ConfigureLocation(assets.ErenLocation, "location.eren.apartment", "Eren'in Dairesi", "SCN_ErenApartment");
            ConfigureLocation(assets.MertLocation, "location.mert.apartment", "Mert'in Dairesi", "SCN_MertApartment");

            ConfigureInspection(
                assets.MedicationInspection,
                "location.eren.apartment.medication",
                "Nörolojik İlaç",
                "Reçete etiketi Eren'in adına. Doz, bellek bütünlüğü dalgalanmalarını bastırmak için artırılmış.");
            ConfigureInspection(
                assets.ForeshadowInspection,
                "location.eren.apartment.photograph",
                "Kesilmiş Fotoğraf",
                "Çerçevede üç kişilik boşluk var; fotoğrafın sağ kenarı dikkatle kesilmiş. Eren yüzleri çıkaramıyor.");
            ConfigureInspection(
                assets.MertPhotoInspection,
                "location.mert.apartment.photograph",
                "Mert'in Fotoğrafı",
                "Mert'in yanında Eren duruyor. Eren bu anı hatırlamıyor. Arka yüzünde: “Dördümüzden biri hatırlamalı.”");
            ConfigureInspection(
                assets.DeathTimeInspection,
                "location.mert.apartment.death_time",
                "Adli Zaman Damgası",
                "Biyometrik bileklik son yaşamsal sinyali 02:36 olarak kaydetmiş.");

            ConfigureEvidence(
                assets.MertPhotoEvidence,
                "evidence.mert.photo",
                "Mert'le Fotoğraf",
                "Gözlem: Eren, Mert'le aynı fotoğrafta. Yazı: “Dördümüzden biri hatırlamalı.”",
                EvidenceType.Physical,
                assets.Case.StableId,
                true);
            ConfigureEvidence(
                assets.MertTerminalEvidence,
                "evidence.mert.terminal_log_0251",
                "02:51 Terminal Kaydı",
                "Gözlem: Mert'in terminalinde 02:51'de manuel erişim kaydı bulunuyor.",
                EvidenceType.Digital,
                assets.Case.StableId,
                true);
            ConfigureEvidence(
                assets.MertDeathTimeEvidence,
                "evidence.mert.death_time_0236",
                "02:36 Ölüm Zamanı",
                "Gözlem: Biyometrik kayıt Mert'in ölüm zamanını 02:36 olarak gösteriyor.",
                EvidenceType.Biometric,
                assets.Case.StableId,
                true);
            ConfigureDeduction(assets.PostmortemDeduction, assets);
            ConfigureDispatchTerminal(assets.DispatchTerminal);
            ConfigureMertTerminal(assets.MertTerminal, assets.MertTerminalEvidence.StableId);
            ConfigureMemory(assets.PhotoMemory);
            ConfigureDialogue(assets.MertRecorderDialogue, assets.Mert);
            ConfigureCatalog(assets);

            EditorUtility.SetDirty(assets.Catalog);
            AssetDatabase.SaveAssets();
            return assets;
        }

        private static void ConfigureCase(CaseData data)
        {
            SerializedObject serialized = BeginContent(
                data,
                "case.mert.suspicious_death",
                "Opening suspicious-death investigation in Sector 7.");
            serialized.FindProperty("_displayName").stringValue = "Mert Ersoy — Şüpheli Ölüm";
            serialized.FindProperty("_summary").stringValue =
                "Sektör 7'de şüpheli ölüm. Kurbanın bellek bütünlüğünde açıklanamayan bir anomali var.";
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureCharacter(CharacterData data, string stableId, string displayName, string role)
        {
            SerializedObject serialized = BeginContent(data, stableId, $"Opening character: {displayName}.");
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_roleLabel").stringValue = role;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureLocation(LocationData data, string stableId, string displayName, string sceneName)
        {
            SerializedObject serialized = BeginContent(data, stableId, $"Production opening location: {displayName}.");
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_sceneName").stringValue = sceneName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureInspection(InspectData data, string stableId, string title, string description)
        {
            SerializedObject serialized = BeginContent(data, stableId, $"Authored inspection: {title}.");
            serialized.FindProperty("_title").stringValue = title;
            serialized.FindProperty("_description").stringValue = description;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureEvidence(
            EvidenceData data,
            string stableId,
            string displayName,
            string description,
            EvidenceType type,
            string caseId,
            bool isCritical)
        {
            SerializedObject serialized = BeginContent(data, stableId, $"Opening evidence: {displayName}.");
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_description").stringValue = description;
            serialized.FindProperty("_evidenceType").enumValueIndex = (int)type;
            serialized.FindProperty("_caseId").stringValue = caseId;
            serialized.FindProperty("_isCritical").boolValue = isCritical;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDeduction(DeductionData data, OpeningAssets assets)
        {
            SerializedObject serialized = BeginContent(
                data,
                "deduction.mert.postmortem_terminal",
                "Opening deduction comparing Mert's death time with terminal access.");
            SetStringArray(
                serialized.FindProperty("_requiredEvidenceIds"),
                new[] { assets.MertTerminalEvidence.StableId, assets.MertDeathTimeEvidence.StableId });
            serialized.FindProperty("_resultTitle").stringValue = "Ölüm Sonrası Terminal Erişimi";
            serialized.FindProperty("_resultText").stringValue =
                "Mert'in terminaline ölümünden on beş dakika sonra manuel olarak erişildi.";
            serialized.FindProperty("_resultingEvidenceId").stringValue = string.Empty;
            SetStringArray(
                serialized.FindProperty("_storyFlagsToSet"),
                new[] { "flag.mert.postmortem_terminal_deduced" });
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDispatchTerminal(TerminalData data)
        {
            SerializedObject serialized = BeginContent(
                data,
                "terminal.eren.dispatch",
                "Eren apartment dispatch terminal for the opening assignment.");
            serialized.FindProperty("_menuTitle").stringValue = "SEKTÖR 7 // SEVK BAĞLANTISI // 03:17";
            SerializedProperty entries = serialized.FindProperty("_entries");
            entries.arraySize = 1;
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(0),
                "dispatch_0317",
                TerminalEntryCategory.Mail,
                "ACİL GÖREVLENDİRME",
                "03:17\nŞüpheli ölüm bildirimi.\nSektör 7.\nKurban: Mert Ersoy.\nÖn tarama, bellek bütünlüğü anomalisine işaret ediyor.\nOlay yeri incelemesi için derhal hareket edin.",
                string.Empty,
                DispatchReceivedFlag);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureMertTerminal(TerminalData data, string evidenceId)
        {
            SerializedObject serialized = BeginContent(
                data,
                "terminal.mert.personal",
                "Mert apartment terminal containing the postmortem access log.");
            serialized.FindProperty("_menuTitle").stringValue = "MERT // YEREL TERMİNAL";
            SerializedProperty entries = serialized.FindProperty("_entries");
            entries.arraySize = 2;
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(0),
                "security_access_0251",
                TerminalEntryCategory.Security,
                "ERİŞİM KAYDI",
                "02:51 — MANUEL ERİŞİM\nKimlik doğrulama: yerel geçersiz kılma\nOturum süresi: 00:03:12",
                evidenceId,
                "flag.mert.terminal_log_read");
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(1),
                "integrity_warning",
                TerminalEntryCategory.Logs,
                "BÜTÜNLÜK UYARISI",
                "Bellek eşleme dizini doğrulanamadı. Arşiv alanı çevrimdışı.",
                string.Empty,
                string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureTerminalEntry(
            SerializedProperty entry,
            string entryId,
            TerminalEntryCategory category,
            string title,
            string body,
            string evidenceId,
            string storyFlag)
        {
            entry.FindPropertyRelative("_entryId").stringValue = entryId;
            entry.FindPropertyRelative("_category").enumValueIndex = (int)category;
            entry.FindPropertyRelative("_title").stringValue = title;
            entry.FindPropertyRelative("_body").stringValue = body;
            entry.FindPropertyRelative("_evidenceId").stringValue = evidenceId;
            entry.FindPropertyRelative("_storyFlagToSet").stringValue = storyFlag;
        }

        private static void ConfigureMemory(MemoryData data)
        {
            SerializedObject serialized = BeginContent(
                data,
                "memory.mert.photo_glitch",
                "First fragmented memory glitch triggered by Mert's photograph.");
            serialized.FindProperty("_title").stringValue = "BOZUK ANIMSAMA // 01";
            SerializedProperty beats = serialized.FindProperty("_beats");
            string[] text =
            {
                "LABORATUVAR // görüntü kaybı",
                "ALARM // bağlantı kararsız",
                "MERT: “Başladıktan sonra geri dönüş yok.”",
                "BİR SİLUET // kimlik çözülemedi",
                "SİNYAL KESİLDİ"
            };
            Color[] colors =
            {
                new Color(0.05f, 0.8f, 0.9f, 0.38f),
                new Color(0.85f, 0.1f, 0.16f, 0.42f),
                new Color(0.95f, 0.58f, 0.12f, 0.35f),
                new Color(0.15f, 0.2f, 0.25f, 0.65f),
                new Color(0.8f, 0.08f, 0.14f, 0.48f)
            };
            beats.arraySize = text.Length;
            for (int index = 0; index < text.Length; index++)
            {
                SerializedProperty beat = beats.GetArrayElementAtIndex(index);
                beat.FindPropertyRelative("_text").stringValue = text[index];
                beat.FindPropertyRelative("_duration").floatValue = index == 2 ? 2f : 1.25f;
                beat.FindPropertyRelative("_overlayColor").colorValue = colors[index];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDialogue(DialogueData data, CharacterData speaker)
        {
            SerializedObject serialized = BeginContent(
                data,
                "dialogue.mert.damaged_recorder",
                "Optional opening voice fragment demonstrating the data-driven dialogue pipeline.");
            serialized.FindProperty("_startNodeId").stringValue = "fragment_1";
            SerializedProperty nodes = serialized.FindProperty("_nodes");
            nodes.arraySize = 2;
            ConfigureDialogueNode(
                nodes.GetArrayElementAtIndex(0),
                "fragment_1",
                speaker,
                "Kayıt bozuk. Sesimin ne kadarı kaldı bilmiyorum.",
                "fragment_2");
            ConfigureDialogueNode(
                nodes.GetArrayElementAtIndex(1),
                "fragment_2",
                speaker,
                "Eğer bunu dinliyorsan, zaman çizelgesine güven. Hatırladıklarına değil.",
                string.Empty);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDialogueNode(
            SerializedProperty node,
            string nodeId,
            CharacterData speaker,
            string text,
            string nextNodeId)
        {
            node.FindPropertyRelative("_nodeId").stringValue = nodeId;
            node.FindPropertyRelative("_speaker").objectReferenceValue = speaker;
            node.FindPropertyRelative("_text").stringValue = text;
            node.FindPropertyRelative("_nextNodeId").stringValue = nextNodeId;
            node.FindPropertyRelative("_choices").arraySize = 0;
            node.FindPropertyRelative("_actions").arraySize = 0;
        }

        private static void ConfigureCatalog(OpeningAssets assets)
        {
            AuthoredContentAsset[] allContent =
            {
                assets.Case,
                assets.Eren,
                assets.Mert,
                assets.Dispatch,
                assets.ErenLocation,
                assets.MertLocation,
                assets.MedicationInspection,
                assets.ForeshadowInspection,
                assets.MertPhotoInspection,
                assets.DeathTimeInspection,
                assets.MertPhotoEvidence,
                assets.MertTerminalEvidence,
                assets.MertDeathTimeEvidence,
                assets.PostmortemDeduction,
                assets.DispatchTerminal,
                assets.MertTerminal,
                assets.PhotoMemory,
                assets.MertRecorderDialogue
            };
            SerializedObject serialized = new SerializedObject(assets.Catalog);
            SetObjectArray(serialized.FindProperty("_allContent"), allContent);
            SetObjectArray(
                serialized.FindProperty("_evidence"),
                new[] { assets.MertPhotoEvidence, assets.MertTerminalEvidence, assets.MertDeathTimeEvidence });
            SetObjectArray(serialized.FindProperty("_deductions"), new[] { assets.PostmortemDeduction });
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildBootstrapScene(OpeningAssets assets, InputActionAsset inputActions)
        {
            BuildScene(BootstrapScenePath, () =>
            {
                var root = new GameObject("Game Application");
                GameModeController modes = root.AddComponent<GameModeController>();
                GameplayInputReader input = root.AddComponent<GameplayInputReader>();
                input.Configure(inputActions);
                PauseInputHandler pause = root.AddComponent<PauseInputHandler>();
                SceneLoader loader = root.AddComponent<SceneLoader>();
                GameApplication application = root.AddComponent<GameApplication>();
                application.Configure(
                    modes,
                    input,
                    pause,
                    loader,
                    assets.Catalog,
                    assets.ErenLocation,
                    "entry");

                var cameraObject = new GameObject("Bootstrap Camera");
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = BackgroundColor;
            });
        }

        private static void BuildErenApartmentScene(OpeningAssets assets)
        {
            BuildScene(ErenScenePath, () =>
            {
                SceneSetup setup = CreateGameplayScene("EREN'İN DAİRESİ // 03:17", assets.ErenLocation);

                GameObject medication = CreateInteractableBlock(
                    "Neurological Medication",
                    new Vector3(-3.25f, -1.35f, 0f),
                    new Vector2(0.65f, 0.65f),
                    Cyan);
                medication.AddComponent<InspectInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.MedicationInspection);

                GameObject photograph = CreateInteractableBlock(
                    "Cut Photograph",
                    new Vector3(-0.8f, -1.2f, 0f),
                    new Vector2(0.75f, 0.9f),
                    new Color(0.3f, 0.4f, 0.48f, 1f));
                photograph.AddComponent<InspectInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.ForeshadowInspection);

                GameObject dispatch = CreateInteractableBlock(
                    "Dispatch Terminal",
                    new Vector3(1.75f, -1.15f, 0f),
                    new Vector2(1.15f, 1.15f),
                    Amber);
                dispatch.AddComponent<TerminalInteractable>().Configure(
                    setup.Ui.TerminalController,
                    assets.DispatchTerminal);

                GameObject exit = CreateInteractableBlock(
                    "Exit to Mert Apartment",
                    new Vector3(4.65f, -0.75f, 0f),
                    new Vector2(0.9f, 2f),
                    Red);
                SceneTransitionInteractable transition = exit.AddComponent<SceneTransitionInteractable>();
                transition.Configure(assets.MertLocation, "entry", DispatchReceivedFlag);

                setup.Installer.Configure(
                    assets.ErenLocation,
                    setup.Player,
                    setup.Detector,
                    setup.Ui.InspectController,
                    setup.Ui.DialogueController,
                    setup.Ui.TerminalController,
                    setup.Ui.MemoryController,
                    setup.Ui.EvidenceBoardController,
                    setup.Ui.InteractionPrompt,
                    setup.Ui.EvidenceNotification,
                    new[] { transition },
                    new[] { setup.SpawnPoint });
            });
        }

        private static void BuildMertApartmentScene(OpeningAssets assets)
        {
            BuildScene(MertScenePath, () =>
            {
                SceneSetup setup = CreateGameplayScene("MERT'İN DAİRESİ // OLAY YERİ", assets.MertLocation);

                GameObject photograph = CreateInteractableBlock(
                    "Mert Photograph",
                    new Vector3(-3.5f, -1.2f, 0f),
                    new Vector2(0.75f, 0.95f),
                    Cyan);
                MemoryEvidenceTrigger memoryTrigger = photograph.AddComponent<MemoryEvidenceTrigger>();
                memoryTrigger.Configure(setup.Ui.MemoryController, assets.PhotoMemory);
                photograph.AddComponent<EvidenceInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.MertPhotoInspection,
                    assets.MertPhotoEvidence,
                    memoryTrigger);

                GameObject terminal = CreateInteractableBlock(
                    "Mert Terminal",
                    new Vector3(-1.15f, -1.1f, 0f),
                    new Vector2(1.1f, 1.25f),
                    Amber);
                terminal.AddComponent<TerminalInteractable>().Configure(
                    setup.Ui.TerminalController,
                    assets.MertTerminal);

                GameObject deathTime = CreateInteractableBlock(
                    "Biometric Death Time",
                    new Vector3(1.35f, -1.35f, 0f),
                    new Vector2(0.75f, 0.6f),
                    Red);
                deathTime.AddComponent<EvidenceInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.DeathTimeInspection,
                    assets.MertDeathTimeEvidence);

                GameObject board = CreateInteractableBlock(
                    "Evidence Board",
                    new Vector3(3.45f, -0.95f, 0f),
                    new Vector2(1.4f, 1.55f),
                    new Color(0.18f, 0.52f, 0.58f, 1f));
                board.AddComponent<EvidenceBoardInteractable>().Configure(setup.Ui.EvidenceBoardController);

                GameObject recorder = CreateInteractableBlock(
                    "Damaged Recorder",
                    new Vector3(5.2f, -1.4f, 0f),
                    new Vector2(0.55f, 0.55f),
                    new Color(0.45f, 0.25f, 0.32f, 1f));
                recorder.AddComponent<DialogueInteractable>().Configure(
                    setup.Ui.DialogueController,
                    assets.MertRecorderDialogue);

                setup.Installer.Configure(
                    assets.MertLocation,
                    setup.Player,
                    setup.Detector,
                    setup.Ui.InspectController,
                    setup.Ui.DialogueController,
                    setup.Ui.TerminalController,
                    setup.Ui.MemoryController,
                    setup.Ui.EvidenceBoardController,
                    setup.Ui.InteractionPrompt,
                    setup.Ui.EvidenceNotification,
                    Array.Empty<SceneTransitionInteractable>(),
                    new[] { setup.SpawnPoint });
            });
        }

        private static SceneSetup CreateGameplayScene(string sceneTitle, LocationData location)
        {
            Sprite sprite = GetPlaceholderSprite();
            CreateCamera();
            CreateWorldBlock("Backdrop", Vector3.zero, new Vector2(15f, 9f), BackgroundColor, sprite, false, -10);
            CreateWorldBlock(
                "Floor",
                new Vector3(0f, -2.55f, 0f),
                new Vector2(15f, 1f),
                new Color(0.07f, 0.1f, 0.13f, 1f),
                sprite,
                true,
                -1);
            CreateBoundary("Left Boundary", new Vector3(-6.8f, 0f, 0f), new Vector2(0.5f, 7f));
            CreateBoundary("Right Boundary", new Vector3(6.8f, 0f, 0f), new Vector2(0.5f, 7f));

            TextMesh title = CreateWorldText(sceneTitle, new Vector3(-6.1f, 3.85f, 0f), TextAnchor.UpperLeft, Cyan);
            title.name = "Location Title";

            var spawnObject = new GameObject("SpawnPoint_entry");
            spawnObject.transform.position = new Vector3(-5.2f, -1.15f, 0f);
            SpawnPoint spawn = spawnObject.AddComponent<SpawnPoint>();
            spawn.Configure("entry");

            PlayerController player = CreatePlayer(sprite, out PlayerInteractionDetector detector);
            SceneUi ui = CreateSceneUi();
            var installerObject = new GameObject("Scene Installer");
            OpeningSceneInstaller installer = installerObject.AddComponent<OpeningSceneInstaller>();

            return new SceneSetup
            {
                Player = player,
                Detector = detector,
                Ui = ui,
                Installer = installer,
                SpawnPoint = spawn
            };
        }

        private static PlayerController CreatePlayer(Sprite sprite, out PlayerInteractionDetector detector)
        {
            var playerObject = new GameObject("Player");
            playerObject.transform.position = new Vector3(-5.2f, -1.15f, 0f);
            playerObject.transform.localScale = new Vector3(0.72f, 1.35f, 1f);
            SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.3f, 0.78f, 0.88f, 1f);
            renderer.sortingOrder = 2;
            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.freezeRotation = true;
            playerObject.AddComponent<BoxCollider2D>();
            PlayerController player = playerObject.AddComponent<PlayerController>();
            player.SetMoveSpeed(4f);
            detector = playerObject.AddComponent<PlayerInteractionDetector>();
            detector.SetDetection(1.45f, ~0);
            return player;
        }

        private static SceneUi CreateSceneUi()
        {
            var eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            InputSystemUIInputModule uiInput = eventSystemObject.AddComponent<InputSystemUIInputModule>();
            uiInput.AssignDefaultActions();

            var canvasObject = new GameObject("UI Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var systems = new GameObject("Scene UI Controllers");
            SceneUi ui = new SceneUi
            {
                InspectController = systems.AddComponent<InspectController>(),
                DialogueController = systems.AddComponent<DialogueController>(),
                TerminalController = systems.AddComponent<TerminalController>(),
                MemoryController = systems.AddComponent<MemoryController>(),
                EvidenceBoardController = systems.AddComponent<EvidenceBoardController>(),
                InteractionPrompt = systems.AddComponent<InteractionPromptController>(),
                EvidenceNotification = systems.AddComponent<EvidenceNotificationController>()
            };

            ConfigureHud(canvasObject.transform, ui);
            ConfigureInspectPanel(canvasObject.transform, ui);
            ConfigureDialoguePanel(canvasObject.transform, ui);
            ConfigureTerminalPanel(canvasObject.transform, ui);
            ConfigureMemoryPanel(canvasObject.transform, ui);
            ConfigureEvidenceBoardPanel(canvasObject.transform, ui);
            return ui;
        }

        private static void ConfigureHud(Transform canvas, SceneUi ui)
        {
            GameObject promptRoot = CreateImage(
                canvas,
                "Interaction Prompt",
                new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f),
                new Vector2(0f, 54f),
                new Vector2(620f, 58f),
                new Color(0.02f, 0.05f, 0.07f, 0.94f));
            Text prompt = CreateText(
                promptRoot.transform,
                "Prompt Label",
                string.Empty,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                Vector2.zero,
                28,
                TextAnchor.MiddleCenter,
                Cyan);
            ui.InteractionPrompt.Configure(promptRoot, prompt);
            promptRoot.SetActive(false);

            GameObject notificationRoot = CreateImage(
                canvas,
                "Evidence Notification",
                new Vector2(1f, 1f),
                new Vector2(1f, 1f),
                new Vector2(-270f, -100f),
                new Vector2(470f, 110f),
                new Color(0.02f, 0.08f, 0.1f, 0.96f));
            Text notification = CreateText(
                notificationRoot.transform,
                "Notification Label",
                string.Empty,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-30f, -20f),
                25,
                TextAnchor.MiddleLeft,
                Cyan);
            ui.EvidenceNotification.Configure(notificationRoot, notification);
            notificationRoot.SetActive(false);
        }

        private static void ConfigureInspectPanel(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateModalRoot(canvas, "Inspect Panel", new Color(0.01f, 0.025f, 0.035f, 0.94f));
            GameObject frame = CreateImage(
                root.transform,
                "Inspect Frame",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(980f, 610f),
                PanelColor);
            Text title = CreateText(frame.transform, "Title", string.Empty, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -72f), new Vector2(840f, 70f), 42, TextAnchor.MiddleLeft, Cyan);
            Text description = CreateText(frame.transform, "Description", string.Empty, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 50f), new Vector2(840f, 300f), 30, TextAnchor.UpperLeft, TextColor);
            Text status = CreateText(frame.transform, "Status", string.Empty, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-190f, 105f), new Vector2(440f, 48f), 24, TextAnchor.MiddleLeft, Amber);
            Button collect = CreateButton(frame.transform, "Collect", "KANITI KAYDET", new Vector2(170f, 65f), new Vector2(-145f, 45f), Cyan, out _);
            Button close = CreateButton(frame.transform, "Close", "KAPAT", new Vector2(170f, 65f), new Vector2(145f, 45f), new Color(0.2f, 0.28f, 0.32f, 1f), out _);
            InspectPanel panel = root.AddComponent<InspectPanel>();
            panel.Configure(root, title, description, status, collect, close);
            ui.InspectController.Configure(panel);
            root.SetActive(false);
        }

        private static void ConfigureDialoguePanel(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateModalRoot(canvas, "Dialogue Panel", new Color(0f, 0f, 0f, 0.56f));
            GameObject frame = CreateImage(root.transform, "Dialogue Frame", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 245f), new Vector2(1460f, 390f), PanelColor);
            Text speaker = CreateText(frame.transform, "Speaker", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(235f, -55f), new Vector2(420f, 55f), 30, TextAnchor.MiddleLeft, Amber);
            Text body = CreateText(frame.transform, "Body", string.Empty, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-160f, 50f), new Vector2(1000f, 180f), 30, TextAnchor.UpperLeft, TextColor);
            Button continueButton = CreateButton(frame.transform, "Continue", "DEVAM", new Vector2(175f, 58f), new Vector2(500f, -130f), Cyan, out _);
            Button closeButton = CreateButton(frame.transform, "Close", "KAPAT", new Vector2(150f, 54f), new Vector2(500f, 145f), new Color(0.2f, 0.28f, 0.32f, 1f), out _);
            var choiceButtons = new Button[3];
            var choiceLabels = new Text[3];
            for (int index = 0; index < choiceButtons.Length; index++)
            {
                choiceButtons[index] = CreateButton(frame.transform, $"Choice {index + 1}", string.Empty, new Vector2(680f, 52f), new Vector2(140f, -65f - index * 60f), new Color(0.08f, 0.2f, 0.24f, 1f), out choiceLabels[index]);
            }

            DialoguePanel panel = root.AddComponent<DialoguePanel>();
            panel.Configure(root, speaker, body, continueButton, closeButton, choiceButtons, choiceLabels);
            ui.DialogueController.Configure(panel);
            root.SetActive(false);
        }

        private static void ConfigureTerminalPanel(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateModalRoot(canvas, "Terminal Panel", new Color(0.005f, 0.02f, 0.025f, 0.98f));
            GameObject frame = CreateImage(root.transform, "Terminal Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1580f, 820f), new Color(0.015f, 0.055f, 0.055f, 1f));
            Text title = CreateText(frame.transform, "Title", string.Empty, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1400f, 65f), 38, TextAnchor.MiddleLeft, Cyan);
            Text body = CreateText(frame.transform, "Body", string.Empty, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(270f, 0f), new Vector2(820f, 540f), 27, TextAnchor.UpperLeft, TextColor);
            Text status = CreateText(frame.transform, "Status", string.Empty, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(270f, 80f), new Vector2(820f, 50f), 23, TextAnchor.MiddleLeft, Amber);
            Button close = CreateButton(frame.transform, "Close", "ÇIKIŞ", new Vector2(170f, 58f), new Vector2(610f, 330f), Red, out _);
            var entryButtons = new Button[6];
            var entryLabels = new Text[6];
            for (int index = 0; index < entryButtons.Length; index++)
            {
                entryButtons[index] = CreateButton(frame.transform, $"Entry {index + 1}", string.Empty, new Vector2(470f, 68f), new Vector2(-500f, 230f - index * 82f), new Color(0.035f, 0.15f, 0.17f, 1f), out entryLabels[index]);
            }

            TerminalPanel panel = root.AddComponent<TerminalPanel>();
            panel.Configure(root, title, body, status, close, entryButtons, entryLabels);
            ui.TerminalController.Configure(panel);
            root.SetActive(false);
        }

        private static void ConfigureMemoryPanel(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateModalRoot(canvas, "Memory Panel", Color.black);
            Image overlay = root.GetComponent<Image>();
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            Text title = CreateText(root.transform, "Title", string.Empty, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(1500f, 80f), 32, TextAnchor.MiddleCenter, Red);
            Text beat = CreateText(root.transform, "Beat", string.Empty, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1500f, 260f), 52, TextAnchor.MiddleCenter, TextColor);
            MemoryPanel panel = root.AddComponent<MemoryPanel>();
            panel.Configure(root, overlay, title, beat, group);
            AudioSource audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            ui.MemoryController.Configure(panel, audio);
            root.SetActive(false);
        }

        private static void ConfigureEvidenceBoardPanel(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateModalRoot(canvas, "Evidence Board Panel", new Color(0.01f, 0.025f, 0.035f, 0.98f));
            GameObject frame = CreateImage(root.transform, "Board Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1660f, 900f), PanelColor);
            CreateText(frame.transform, "Heading", "KANIT PANOSU // ÇALIŞMA HİPOTEZİ", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -55f), new Vector2(1500f, 65f), 38, TextAnchor.MiddleLeft, Cyan);
            Text status = CreateText(frame.transform, "Status", string.Empty, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-250f, 95f), new Vector2(920f, 56f), 23, TextAnchor.MiddleLeft, Amber);
            Text solved = CreateText(frame.transform, "Solved", string.Empty, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-385f, 40f), new Vector2(650f, 590f), 26, TextAnchor.UpperLeft, TextColor);
            Button attempt = CreateButton(frame.transform, "Attempt", "ÇIKARIMI DENE", new Vector2(230f, 62f), new Vector2(265f, -350f), Cyan, out _);
            Button reset = CreateButton(frame.transform, "Reset", "SEÇİMİ TEMİZLE", new Vector2(230f, 62f), new Vector2(515f, -350f), new Color(0.2f, 0.28f, 0.32f, 1f), out _);
            Button close = CreateButton(frame.transform, "Close", "KAPAT", new Vector2(160f, 58f), new Vector2(690f, 355f), Red, out _);
            var evidenceButtons = new Button[8];
            var evidenceLabels = new Text[8];
            for (int index = 0; index < evidenceButtons.Length; index++)
            {
                evidenceButtons[index] = CreateButton(frame.transform, $"Evidence {index + 1}", string.Empty, new Vector2(650f, 62f), new Vector2(-430f, 275f - index * 72f), new Color(0.04f, 0.16f, 0.19f, 1f), out evidenceLabels[index]);
            }

            EvidenceBoardPanel panel = root.AddComponent<EvidenceBoardPanel>();
            panel.Configure(root, status, solved, attempt, reset, close, evidenceButtons, evidenceLabels);
            ui.EvidenceBoardController.Configure(panel);
            root.SetActive(false);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0.35f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 4.8f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BackgroundColor;
        }

        private static GameObject CreateInteractableBlock(
            string name,
            Vector3 position,
            Vector2 scale,
            Color color)
        {
            GameObject block = CreateWorldBlock(name, position, scale, color, GetPlaceholderSprite(), false, 1);
            BoxCollider2D collider = block.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            return block;
        }

        private static GameObject CreateWorldBlock(
            string name,
            Vector3 position,
            Vector2 scale,
            Color color,
            Sprite sprite,
            bool hasCollider,
            int sortingOrder)
        {
            var block = new GameObject(name);
            block.transform.position = position;
            block.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            if (hasCollider)
            {
                block.AddComponent<BoxCollider2D>();
            }

            return block;
        }

        private static void CreateBoundary(string name, Vector3 position, Vector2 size)
        {
            var boundary = new GameObject(name);
            boundary.transform.position = position;
            BoxCollider2D collider = boundary.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static TextMesh CreateWorldText(string text, Vector3 position, TextAnchor anchor, Color color)
        {
            var textObject = new GameObject("World Text");
            textObject.transform.position = position;
            TextMesh label = textObject.AddComponent<TextMesh>();
            label.text = text;
            label.anchor = anchor;
            label.alignment = TextAlignment.Left;
            label.fontSize = 50;
            label.characterSize = 0.04f;
            label.color = color;
            return label;
        }

        private static GameObject CreateModalRoot(Transform parent, string name, Color color)
        {
            return CreateImage(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, color);
        }

        private static GameObject CreateImage(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color)
        {
            GameObject item = CreateRect(parent, name, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            item.AddComponent<Image>().color = color;
            return item;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string text,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            GameObject item = CreateRect(parent, name, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            Text label = item.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        private static Button CreateButton(
            Transform parent,
            string name,
            string labelText,
            Vector2 size,
            Vector2 position,
            Color color,
            out Text label)
        {
            GameObject item = CreateImage(
                parent,
                name,
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                position,
                size,
                color);
            Button button = item.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.highlightedColor = Color.Lerp(color, Color.white, 0.25f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
            button.colors = colors;
            label = CreateText(
                item.transform,
                "Label",
                labelText,
                Vector2.zero,
                Vector2.one,
                Vector2.zero,
                new Vector2(-22f, -10f),
                22,
                TextAnchor.MiddleCenter,
                TextColor);
            return button;
        }

        private static GameObject CreateRect(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var item = new GameObject(name, typeof(RectTransform));
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return item;
        }

        private static void BuildScene(string path, Action build)
        {
            Scene previous = SceneManager.GetActiveScene();
            bool hasSavedPreviousScene = previous.IsValid() &&
                                         previous.isLoaded &&
                                         !string.IsNullOrEmpty(previous.path);
            if (previous.IsValid() &&
                previous.isLoaded &&
                string.IsNullOrEmpty(previous.path) &&
                previous.isDirty &&
                !Application.isBatchMode)
            {
                throw new InvalidOperationException(
                    "Save or close the current untitled scene before rebuilding opening scenes.");
            }

            bool preservePrevious = hasSavedPreviousScene;
            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                preservePrevious ? NewSceneMode.Additive : NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);

            try
            {
                build();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, path))
                {
                    throw new InvalidOperationException($"Failed to save production scene at {path}.");
                }
            }
            finally
            {
                if (preservePrevious)
                {
                    EditorSceneManager.CloseScene(scene, true);
                    SceneManager.SetActiveScene(previous);
                }
            }
        }

        private static void ConfigureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootstrapScenePath, true),
                new EditorBuildSettingsScene(ErenScenePath, true),
                new EditorBuildSettingsScene(MertScenePath, true)
            };
        }

        private static SerializedObject BeginContent(
            AuthoredContentAsset asset,
            string stableId,
            string editorDescription)
        {
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("_stableId").stringValue = stableId;
            serialized.FindProperty("_editorDescription").stringValue = editorDescription;
            return serialized;
        }

        private static void SetStringArray(SerializedProperty property, IReadOnlyList<string> values)
        {
            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).stringValue = values[index];
            }
        }

        private static void SetObjectArray<T>(SerializedProperty property, IReadOnlyList<T> values)
            where T : Object
        {
            property.arraySize = values.Count;
            for (int index = 0; index < values.Count; index++)
            {
                property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
            }
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        private static Sprite GetPlaceholderSprite()
        {
            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (sprite == null)
            {
                throw new InvalidOperationException("Unity's built-in UI sprite could not be loaded.");
            }

            return sprite;
        }

        private sealed class OpeningAssets
        {
            public CaseData Case;
            public CharacterData Eren;
            public CharacterData Mert;
            public CharacterData Dispatch;
            public LocationData ErenLocation;
            public LocationData MertLocation;
            public InspectData MedicationInspection;
            public InspectData ForeshadowInspection;
            public InspectData MertPhotoInspection;
            public InspectData DeathTimeInspection;
            public EvidenceData MertPhotoEvidence;
            public EvidenceData MertTerminalEvidence;
            public EvidenceData MertDeathTimeEvidence;
            public DeductionData PostmortemDeduction;
            public TerminalData DispatchTerminal;
            public TerminalData MertTerminal;
            public MemoryData PhotoMemory;
            public DialogueData MertRecorderDialogue;
            public ContentCatalog Catalog;
        }

        private sealed class SceneSetup
        {
            public PlayerController Player;
            public NullPointer.Interaction.PlayerInteractionDetector Detector;
            public SceneUi Ui;
            public OpeningSceneInstaller Installer;
            public SpawnPoint SpawnPoint;
        }

        private sealed class SceneUi
        {
            public InspectController InspectController;
            public DialogueController DialogueController;
            public TerminalController TerminalController;
            public MemoryController MemoryController;
            public EvidenceBoardController EvidenceBoardController;
            public InteractionPromptController InteractionPrompt;
            public EvidenceNotificationController EvidenceNotification;
        }
    }
}
