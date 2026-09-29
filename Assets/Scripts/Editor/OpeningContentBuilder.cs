using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Audio;
using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Evidence;
using NullPointer.Input;
using NullPointer.Inspect;
using NullPointer.Interrogation;
using NullPointer.Interaction;
using NullPointer.Journal;
using NullPointer.Memory;
using NullPointer.Menus;
using NullPointer.Player;
using NullPointer.Progression;
using NullPointer.Runtime;
using NullPointer.SceneFlow;
using NullPointer.Terminal;
using NullPointer.UI;
using NullPointer.Visual;
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
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu/SCN_MainMenu.unity";
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
            VisualProductionBuilder.EnsureVisualAssets();
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                throw new InvalidOperationException($"Input action asset not found at {InputActionsPath}.");
            }

            OpeningAssets assets = CreateOrUpdateAssets();
            BuildBootstrapScene(assets, inputActions);
            BuildMainMenuScene();
            BuildErenApartmentScene(assets);
            BuildMertApartmentScene(assets);
            ConfigureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ProductionContentValidator.ValidateOrThrow();

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
            EnsureFolder("Assets/Data/Objectives");
            EnsureFolder("Assets/Data/Checkpoints");
            EnsureFolder("Assets/Data/Journal");
            EnsureFolder("Assets/Data/Interrogation");
            EnsureFolder("Assets/Data/Audio");

            var assets = new OpeningAssets
            {
                Case = GetOrCreate<CaseData>("Assets/Data/Cases/CASE_MertSuspiciousDeath.asset"),
                Eren = GetOrCreate<CharacterData>("Assets/Data/Characters/CHAR_ErenVardar.asset"),
                Mert = GetOrCreate<CharacterData>("Assets/Data/Characters/CHAR_MertErsoy.asset"),
                Dispatch = GetOrCreate<CharacterData>("Assets/Data/Characters/CHAR_Sector7Dispatch.asset"),
                ErenLocation = GetOrCreate<LocationData>("Assets/Data/Locations/LOC_ErenApartment.asset"),
                MertLocation = GetOrCreate<LocationData>("Assets/Data/Locations/LOC_MertApartment.asset"),
                MainMenuLocation = GetOrCreate<LocationData>("Assets/Data/Locations/LOC_MainMenu.asset"),
                MedicationInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_ErenMedication.asset"),
                ForeshadowInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_ErenPhotograph.asset"),
                MertPhotoInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertPhotograph.asset"),
                DeathTimeInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertDeathTime.asset"),
                CoffeeInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertCoffee.asset"),
                ImplantInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertDamagedImplant.asset"),
                DoorInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertDoorStatus.asset"),
                NotesInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertNotes.asset"),
                MedicalInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_MertMedicalDevice.asset"),
                ErenDeviceInspection = GetOrCreate<InspectData>("Assets/Data/Inspections/INSP_ErenDeviceCallHistory.asset"),
                MertPhotoEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_PHOTO.asset"),
                MertTerminalEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_TERMINAL_LOG.asset"),
                MertDeathTimeEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_DEATH_TIME.asset"),
                DamagedImplantEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_DAMAGED_IMPLANT.asset"),
                DoorStatusEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_DOOR_STATUS.asset"),
                CallRecordEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_CALL_RECORD.asset"),
                MemoryDeletionEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_MERT_MEMORY_DELETION.asset"),
                ErenCallHistoryEvidence = GetOrCreate<EvidenceData>("Assets/Data/Evidence/EV_EREN_CALL_HISTORY.asset"),
                PostmortemDeduction = GetOrCreate<DeductionData>(
                    "Assets/Data/Deductions/DED_MERT_POSTMORTEM_TERMINAL.asset"),
                SuicideInconsistentDeduction = GetOrCreate<DeductionData>(
                    "Assets/Data/Deductions/DED_MERT_SUICIDE_INCONSISTENT.asset"),
                LockedRoomDeduction = GetOrCreate<DeductionData>(
                    "Assets/Data/Deductions/DED_MERT_LOCKED_ROOM_UNCERTAIN.asset"),
                DispatchTerminal = GetOrCreate<TerminalData>("Assets/Data/Terminals/TERM_ErenDispatch.asset"),
                MertTerminal = GetOrCreate<TerminalData>("Assets/Data/Terminals/TERM_MertPersonal.asset"),
                PhotoMemory = GetOrCreate<MemoryData>("Assets/Data/Memories/MEM_MertPhotoGlitch.asset"),
                MertRecorderDialogue = GetOrCreate<DialogueData>(
                    "Assets/Data/Dialogue/DLG_MertDamagedRecorder.asset"),
                ChapterEndDialogue = GetOrCreate<DialogueData>(
                    "Assets/Data/Dialogue/DLG_CH01_EndHook.asset"),
                AudioCues = GetOrCreate<AudioCueSet>("Assets/Data/Audio/AUD_CH01_Hooks.asset"),
                Catalog = GetOrCreate<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset")
            };

            CreateObjectivesAndCheckpoints(assets);
            CreateJournalAndInterrogation(assets);

            ConfigureCase(assets.Case);
            ConfigureCharacter(assets.Eren, "character.eren.vardar", "Eren Vardar", "Soruşturmacı");
            ConfigureCharacter(assets.Mert, "character.mert.ersoy", "Mert Ersoy", "Mnemosyne Sistem Mühendisi");
            ConfigureCharacter(assets.Dispatch, "character.dispatch.sector_7", "Sektör 7 Sevk", "Operatör");
            ConfigureLocation(assets.ErenLocation, "location.eren.apartment", "Eren'in Dairesi", "SCN_ErenApartment");
            ConfigureLocation(assets.MertLocation, "location.mert.apartment", "Mert'in Dairesi", "SCN_MertApartment");
            ConfigureLocation(assets.MainMenuLocation, "location.system.main_menu", "Ana Menü", "SCN_MainMenu");

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
            ConfigureInspection(assets.CoffeeInspection, "location.mert.apartment.coffee", "Yarım Kalmış Kahve", "Kahve soğuk. Fincanın yanında açık bir teknik not var; gündelik bir kesinti, tek başına kanıt değil.");
            ConfigureInspection(assets.ImplantInspection, "location.mert.apartment.implant", "Hasarlı Bellek İmplantı", "İmplant yuvasında kontrollü söküm izleri ve yanmış bir doğrulama hattı var.");
            ConfigureInspection(assets.DoorInspection, "location.mert.apartment.door", "İç Kapı Kaydı", "Kapı içeriden kilitli görünse de acil servis mandalı kısa süre önce mekanik olarak kullanılmış.");
            ConfigureInspection(assets.NotesInspection, "location.mert.apartment.notes", "Kişisel Notlar", "Mert, temiz kayıtların güvenilir kayıtlar olmadığını yazmış. NLP-0417 kodu kenarda iki kez tekrarlanıyor.");
            ConfigureInspection(assets.MedicalInspection, "location.mert.apartment.medical", "Nöral Kalibratör", "Cihaz son seansın yarıda kesildiğini gösteriyor. Tıbbi cihazın kendisi ölüm nedenini kanıtlamıyor.");
            ConfigureInspection(assets.ErenDeviceInspection, "location.mert.apartment.eren_device", "Eren'in Cihazı", "Mert'in son temas kaydıyla karşılaştırıldığında Eren'in cihazında eşleşen gelen arama görünmüyor.");

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
            ConfigureEvidence(assets.DamagedImplantEvidence, "evidence.mert.damaged_implant", "Hasarlı Bellek İmplantı", "Gözlem: İmplant doğrulama hattı ölümden kısa süre önce fiziksel olarak devre dışı bırakılmış.", EvidenceType.Physical, assets.Case.StableId, true);
            ConfigureEvidence(assets.DoorStatusEvidence, "evidence.mert.door_status", "Kapı Durumu", "Gözlem: İç kilit kapalı; ancak mekanik acil mandalı yakın zamanda kullanılmış.", EvidenceType.Physical, assets.Case.StableId, false);
            ConfigureEvidence(assets.CallRecordEvidence, "evidence.mert.last_call_eren", "Mert'in Son Araması", "Gözlem: Mert'in son arama girişimi ölümünden kısa süre önce Eren Vardar'a yapılmış.", EvidenceType.Digital, assets.Case.StableId, true);
            ConfigureEvidence(assets.MemoryDeletionEvidence, "evidence.mert.memory_deletion_0229", "02:29 Bellek Silme Kaydı", "Gözlem: Mert'in bellek dizininde 02:29'da manuel silme işlemi başlatılmış.", EvidenceType.Digital, assets.Case.StableId, true);
            ConfigureEvidence(assets.ErenCallHistoryEvidence, "evidence.eren.call_history_gap", "Eren'in Arama Geçmişi", "Gözlem: Eren'in cihazında Mert'ten gelen karşılık bir arama bulunmuyor.", EvidenceType.Digital, assets.Case.StableId, true);
            ConfigureDeduction(assets.PostmortemDeduction, assets);
            ConfigureChapterDeductions(assets);
            ConfigureDispatchTerminal(assets.DispatchTerminal, assets);
            ConfigureMertTerminal(assets.MertTerminal, assets);
            ConfigureMemory(assets.PhotoMemory);
            ConfigureDialogue(assets.MertRecorderDialogue, assets.Mert);
            ConfigureChapterEndDialogue(assets.ChapterEndDialogue, assets.Eren);
            ConfigureCatalog(assets);

            EditorUtility.SetDirty(assets.Catalog);
            AssetDatabase.SaveAssets();
            return assets;
        }

        private static void CreateObjectivesAndCheckpoints(OpeningAssets assets)
        {
            assets.Objectives = new[]
            {
                CreateObjective("OBJ_CH01_Dispatch", "objective.ch01.inspect_dispatch", "Dispatch mesajını incele", 10),
                CreateObjective("OBJ_CH01_Travel", "objective.ch01.go_to_mert", "Mert Ersoy'un dairesine git", 20),
                CreateObjective("OBJ_CH01_Investigate", "objective.ch01.investigate_scene", "Olay yerini araştır", 30),
                CreateObjective("OBJ_CH01_DeathTime", "objective.ch01.confirm_death_time", "Mert'in ölüm zamanını doğrula", 40),
                CreateObjective("OBJ_CH01_Terminal", "objective.ch01.inspect_terminal", "Terminal kayıtlarını incele", 50),
                CreateObjective("OBJ_CH01_Photo", "objective.ch01.trace_photo", "Fotoğrafın kaynağını araştır", 60),
                CreateObjective("OBJ_CH01_Compare", "objective.ch01.compare_evidence", "Kanıtları karşılaştır", 70)
            };

            assets.ErenStartCheckpoint = CreateCheckpoint(
                "CHK_CH01_ErenStart",
                "checkpoint.ch01.eren_start",
                "Eren'in dairesi — 03:17",
                assets.ErenLocation,
                "entry");
            assets.DispatchCheckpoint = CreateCheckpoint(
                "CHK_CH01_DispatchComplete",
                "checkpoint.ch01.dispatch_complete",
                "Dispatch tamamlandı",
                assets.ErenLocation,
                "entry");
            assets.MertEntranceCheckpoint = CreateCheckpoint(
                "CHK_CH01_MertEntrance",
                "checkpoint.ch01.mert_entrance",
                "Mert'in dairesi girişi",
                assets.MertLocation,
                "entry");
            assets.CriticalEvidenceCheckpoint = CreateCheckpoint(
                "CHK_CH01_CriticalEvidence",
                "checkpoint.ch01.critical_evidence",
                "İlk kritik kanıt",
                assets.MertLocation,
                "entry");
            assets.FirstDeductionCheckpoint = CreateCheckpoint(
                "CHK_CH01_FirstDeduction",
                "checkpoint.ch01.first_deduction",
                "İlk çıkarım tamamlandı",
                assets.MertLocation,
                "entry");
        }

        private static ObjectiveData CreateObjective(string assetName, string stableId, string title, int order)
        {
            ObjectiveData data = GetOrCreate<ObjectiveData>($"Assets/Data/Objectives/{assetName}.asset");
            SerializedObject serialized = BeginContent(data, stableId, $"Chapter 1 objective: {title}.");
            serialized.FindProperty("_title").stringValue = title;
            serialized.FindProperty("_description").stringValue = title;
            serialized.FindProperty("_displayOrder").intValue = order;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private static CheckpointData CreateCheckpoint(
            string assetName,
            string stableId,
            string displayName,
            LocationData location,
            string spawnPointId)
        {
            CheckpointData data = GetOrCreate<CheckpointData>($"Assets/Data/Checkpoints/{assetName}.asset");
            SerializedObject serialized = BeginContent(data, stableId, $"Safe Chapter 1 checkpoint: {displayName}.");
            serialized.FindProperty("_displayName").stringValue = displayName;
            serialized.FindProperty("_location").objectReferenceValue = location;
            serialized.FindProperty("_spawnPointId").stringValue = spawnPointId;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
        }

        private static void CreateJournalAndInterrogation(OpeningAssets assets)
        {
            assets.JournalEntries = new[]
            {
                CreateJournal("JRN_CASE_Mert", "journal.case.mert", JournalSection.Cases, "Mert Ersoy — Şüpheli Ölüm", "02:36 ölüm kaydı. Bellek bütünlüğü anomalisi. Resmî intihar değerlendirmesi doğrulanmadı.", "", "", "", 10),
                CreateJournal("JRN_PERSON_Eren", "journal.person.eren", JournalSection.People, "Eren Vardar", "Soruşturmayı yürüten kişi. Kendi anılarındaki boşluklar dosyayla kesişiyor.", "", "", "", 10),
                CreateJournal("JRN_PERSON_Mert", "journal.person.mert", JournalSection.People, "Mert Ersoy", "Mnemosyne sistem mühendisi. Ölmeden önce bellek kayıtlarıyla çalışmış.", DispatchReceivedFlag, "", "", 20),
                CreateJournal("JRN_Q_NLP", "journal.question.nlp", JournalSection.Questions, "NLP nedir?", "NLP-0417 bir dosya, deney ya da kişi kodu olabilir. Henüz doğrulanmadı.", "flag.mert.nlp_0417_seen", "", "", 10),
                CreateJournal("JRN_Q_Knew", "journal.question.mert_knew_eren", JournalSection.Questions, "Mert beni neden tanıyordu?", "Fotoğraf fiziksel; dijital bir eşleşme hatasıyla kolayca açıklanamaz.", "", assets.MertPhotoEvidence.StableId, "", 20),
                CreateJournal("JRN_Q_0251", "journal.question.terminal_0251", JournalSection.Questions, "02:51'de terminali kim kullandı?", "Manuel erişim, ölüm kaydından on beş dakika sonra.", "", assets.MertTerminalEvidence.StableId, "", 30),
                CreateJournal("JRN_Q_Memory", "journal.question.memory_changed", JournalSection.Questions, "Mert neden ölmeden önce hafızasını değiştirdi?", "02:29 silme isteği ve implant hasarı aynı olaya bağlanabilir.", "", assets.MemoryDeletionEvidence.StableId, "", 40),
                CreateJournal("JRN_T_Return", "journal.timeline.return_home", JournalSection.Timeline, "01:58 — Mert eve döner", "Bina giriş kaydı; olayın son doğrulanmış sıradan hareketi.", DispatchReceivedFlag, "", "", 10),
                CreateJournal("JRN_T_Delete", "journal.timeline.memory_deletion", JournalSection.Timeline, "02:29 — Bellek silme", "Yerel terminalde manuel silme isteği.", "", assets.MemoryDeletionEvidence.StableId, "", 20),
                CreateJournal("JRN_T_Call", "journal.timeline.last_contact", JournalSection.Timeline, "02:31 — Son temas", "Mert, Eren Vardar'ı aramaya çalışır.", "", assets.CallRecordEvidence.StableId, "", 30),
                CreateJournal("JRN_T_Death", "journal.timeline.death", JournalSection.Timeline, "02:36 — Tahmini ölüm", "Biyometrik son yaşamsal sinyal.", "", assets.MertDeathTimeEvidence.StableId, "", 40),
                CreateJournal("JRN_T_Access", "journal.timeline.terminal_access", JournalSection.Timeline, "02:51 — Manuel terminal erişimi", "Ölümden on beş dakika sonra yerel geçersiz kılma.", "", assets.MertTerminalEvidence.StableId, "", 50)
            };

            assets.TimelineClaim = GetOrCreate<InterrogationClaimData>(
                "Assets/Data/Interrogation/CLM_MertTerminalUntouched.asset");
            SerializedObject claim = BeginContent(
                assets.TimelineClaim,
                "claim.mert.terminal_untouched",
                "Reusable Chapter 1 contradiction fixture for later testimony.");
            claim.FindProperty("_statement").stringValue = "Mert'in ölümünden sonra terminale kimse dokunmadı.";
            SetStringArray(
                claim.FindProperty("_contradictingEvidenceIds"),
                new[] { assets.MertTerminalEvidence.StableId });
            claim.FindProperty("_contradictionStoryFlag").stringValue = "flag.interrogation.postmortem_access_contradiction";
            claim.FindProperty("_successResponse").stringValue = "02:51 kaydı bu ifadeyle çelişiyor. Tanık ayrıntıyı geri çekmek zorunda kalır.";
            claim.FindProperty("_irrelevantResponse").stringValue = "Bu kanıt söz konusu erişim iddiasını doğrudan sınamıyor.";
            claim.ApplyModifiedPropertiesWithoutUndo();
        }

        private static JournalEntryData CreateJournal(
            string assetName,
            string stableId,
            JournalSection section,
            string title,
            string body,
            string flag,
            string evidence,
            string deduction,
            int order)
        {
            JournalEntryData data = GetOrCreate<JournalEntryData>($"Assets/Data/Journal/{assetName}.asset");
            SerializedObject serialized = BeginContent(data, stableId, $"Chapter 1 journal entry: {title}.");
            serialized.FindProperty("_section").enumValueIndex = (int)section;
            serialized.FindProperty("_title").stringValue = title;
            serialized.FindProperty("_body").stringValue = body;
            serialized.FindProperty("_requiredStoryFlag").stringValue = flag;
            serialized.FindProperty("_requiredEvidenceId").stringValue = evidence;
            serialized.FindProperty("_requiredDeductionId").stringValue = deduction;
            serialized.FindProperty("_displayOrder").intValue = order;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return data;
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
            SetStringArray(serialized.FindProperty("_requiredDeductionIds"), Array.Empty<string>());
            serialized.FindProperty("_resultTitle").stringValue = "Ölüm Sonrası Terminal Erişimi";
            serialized.FindProperty("_resultText").stringValue =
                "Mert'in terminaline ölümünden on beş dakika sonra manuel olarak erişildi.";
            serialized.FindProperty("_resultingEvidenceId").stringValue = string.Empty;
            SetStringArray(
                serialized.FindProperty("_storyFlagsToSet"),
                new[] { "flag.mert.postmortem_terminal_deduced" });
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureChapterDeductions(OpeningAssets assets)
        {
            ConfigureDeductionAsset(
                assets.SuicideInconsistentDeduction,
                "deduction.mert.suicide_timeline_inconsistent",
                "The apparent-suicide timeline conflicts with physical and deletion records.",
                new[]
                {
                    assets.MertTerminalEvidence.StableId,
                    assets.MertDeathTimeEvidence.StableId,
                    assets.MemoryDeletionEvidence.StableId,
                    assets.DamagedImplantEvidence.StableId
                },
                new[] { assets.PostmortemDeduction.StableId },
                "Resmî Zaman Çizelgesi Tutarsız",
                "Bellek silme işlemi, implant hasarı ve ölüm sonrası erişim; olayın basit bir intihar olarak kapanamayacağını gösteriyor.",
                "flag.mert.suicide_timeline_inconsistent");
            ConfigureDeductionAsset(
                assets.LockedRoomDeduction,
                "deduction.mert.locked_room_unreliable",
                "Chapter 1 conclusion connecting the door and mismatched call histories.",
                new[]
                {
                    assets.DoorStatusEvidence.StableId,
                    assets.CallRecordEvidence.StableId,
                    assets.ErenCallHistoryEvidence.StableId
                },
                new[] { assets.SuicideInconsistentDeduction.StableId },
                "Kilitli Oda Görünüşü Güvenilir Değil",
                "Mekanik kapı izi ve eşleşmeyen arama geçmişleri, kapalı oda anlatısının yüzeyde göründüğü gibi kabul edilemeyeceğini gösteriyor.",
                "flag.ch01.locked_room_unreliable");
        }

        private static void ConfigureDeductionAsset(
            DeductionData data,
            string stableId,
            string editorDescription,
            string[] requiredEvidence,
            string[] requiredDeductions,
            string title,
            string result,
            string flag)
        {
            SerializedObject serialized = BeginContent(data, stableId, editorDescription);
            SetStringArray(serialized.FindProperty("_requiredEvidenceIds"), requiredEvidence);
            SetStringArray(serialized.FindProperty("_requiredDeductionIds"), requiredDeductions);
            serialized.FindProperty("_resultTitle").stringValue = title;
            serialized.FindProperty("_resultText").stringValue = result;
            serialized.FindProperty("_resultingEvidenceId").stringValue = string.Empty;
            SetStringArray(serialized.FindProperty("_storyFlagsToSet"), new[] { flag });
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureDispatchTerminal(TerminalData data, OpeningAssets assets)
        {
            SerializedObject serialized = BeginContent(
                data,
                "terminal.eren.dispatch",
                "Eren apartment dispatch terminal for the opening assignment.");
            serialized.FindProperty("_menuTitle").stringValue = "SEKTÖR 7 // SEVK BAĞLANTISI // 03:17";
            SerializedProperty entries = serialized.FindProperty("_entries");
            entries.arraySize = 2;
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(0),
                "dispatch_0317",
                TerminalEntryCategory.Mail,
                "ACİL GÖREVLENDİRME",
                "03:17\nŞüpheli ölüm bildirimi.\nSektör 7.\nKurban: Mert Ersoy.\nÖn tarama, bellek bütünlüğü anomalisine işaret ediyor.\nOlay yeri incelemesi için derhal hareket edin.",
                string.Empty,
                DispatchReceivedFlag);
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(1),
                "device_call_history",
                TerminalEntryCategory.Logs,
                "CİHAZ ARAMA GEÇMİŞİ",
                "Son 24 saat: Mert Ersoy kaynaklı gelen arama kaydı yok.",
                assets.ErenCallHistoryEvidence.StableId,
                "flag.eren.call_history_checked");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureMertTerminal(TerminalData data, OpeningAssets assets)
        {
            SerializedObject serialized = BeginContent(
                data,
                "terminal.mert.personal",
                "Mert apartment terminal containing the postmortem access log.");
            serialized.FindProperty("_menuTitle").stringValue = "MERT // YEREL TERMİNAL";
            SerializedProperty entries = serialized.FindProperty("_entries");
            entries.arraySize = 6;
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(0),
                "security_access_0251",
                TerminalEntryCategory.Security,
                "ERİŞİM KAYDI",
                "02:51 — MANUEL ERİŞİM\nKimlik doğrulama: yerel geçersiz kılma\nOturum süresi: 00:03:12",
                assets.MertTerminalEvidence.StableId,
                "flag.mert.terminal_log_read");
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(1),
                "integrity_warning",
                TerminalEntryCategory.Logs,
                "BÜTÜNLÜK UYARISI",
                "Bellek eşleme dizini doğrulanamadı. Arşiv alanı çevrimdışı.",
                string.Empty,
                string.Empty);
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(2),
                "memory_deletion_0229",
                TerminalEntryCategory.Logs,
                "BELLEK DİZİNİ İŞLEMİ",
                "02:29 — manuel silme isteği\nHedef blok: kişisel anı kümesi\nDoğrulama: yerel biyometrik",
                assets.MemoryDeletionEvidence.StableId,
                "flag.mert.memory_deletion_read");
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(3),
                "file_nlp_0417",
                TerminalEntryCategory.Files,
                "NLP-0417 // BOZUK BAŞLIK",
                "Dosya gövdesi kurtarılamadı. Kimlik: NLP-0417. Son yerel değişiklik: 02:28.",
                string.Empty,
                "flag.mert.nlp_0417_seen");
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(4),
                "mail_unsent",
                TerminalEntryCategory.Mail,
                "GÖNDERİLMEMİŞ TASLAK",
                "Temiz kayıt, doğru kayıt demek değil. Fiziksel kopyayı bul.",
                string.Empty,
                string.Empty);
            ConfigureTerminalEntry(
                entries.GetArrayElementAtIndex(5),
                "last_contact",
                TerminalEntryCategory.Security,
                "SON TEMAS DENEMESİ",
                "02:31 — EREN VARDAR\nArama başlatıldı. Karşı cihaz teslim kaydı yok.",
                assets.CallRecordEvidence.StableId,
                "flag.mert.last_contact_revealed",
                "flag.mert.postmortem_terminal_deduced");
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureTerminalEntry(
            SerializedProperty entry,
            string entryId,
            TerminalEntryCategory category,
            string title,
            string body,
            string evidenceId,
            string storyFlag,
            string requiredStoryFlag = "")
        {
            entry.FindPropertyRelative("_entryId").stringValue = entryId;
            entry.FindPropertyRelative("_category").enumValueIndex = (int)category;
            entry.FindPropertyRelative("_title").stringValue = title;
            entry.FindPropertyRelative("_body").stringValue = body;
            entry.FindPropertyRelative("_evidenceId").stringValue = evidenceId;
            entry.FindPropertyRelative("_storyFlagToSet").stringValue = storyFlag;
            entry.FindPropertyRelative("_requiredStoryFlag").stringValue = requiredStoryFlag;
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
                "MERT: “Bunu başlatırsak geri dönüşü yok.”",
                "EREN // yansıma doğrulanamadı",
                "İKİ BELİRSİZ FİGÜR // kimlik çözülemedi",
                "SİNYAL KESİLDİ"
            };
            Color[] colors =
            {
                new Color(0.05f, 0.8f, 0.9f, 0.38f),
                new Color(0.85f, 0.1f, 0.16f, 0.42f),
                new Color(0.95f, 0.58f, 0.12f, 0.35f),
                new Color(0.05f, 0.65f, 0.72f, 0.42f),
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

        private static void ConfigureChapterEndDialogue(DialogueData data, CharacterData speaker)
        {
            SerializedObject serialized = BeginContent(
                data,
                "dialogue.ch01.ending_hook",
                "Chapter 1 ending reveal after the completed deduction chain.");
            serialized.FindProperty("_startNodeId").stringValue = "contact";
            SerializedProperty nodes = serialized.FindProperty("_nodes");
            nodes.arraySize = 4;
            ConfigureDialogueNode(nodes.GetArrayElementAtIndex(0), "contact", speaker, "Mert'in son temas girişimi: EREN VARDAR.", "absence");
            ConfigureDialogueNode(nodes.GetArrayElementAtIndex(1), "absence", speaker, "Arama ölümden kısa süre önce yapılmış. Benim cihazımda karşılık gelen hiçbir kayıt yok.", "identifier");
            ConfigureDialogueNode(nodes.GetArrayElementAtIndex(2), "identifier", speaker, "Fotoğraf, silinen bellek bloğu ve bozuk dosya aynı işarete çıkıyor: NLP-0417.", "hook");
            ConfigureDialogueNode(nodes.GetArrayElementAtIndex(3), "hook", speaker, "Mert beni aradı. Beni tanıyordu. Ben neden hiçbirini hatırlamıyorum?", string.Empty);
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
            AuthoredContentAsset[] allContent = new AuthoredContentAsset[]
            {
                assets.Case,
                assets.Eren,
                assets.Mert,
                assets.Dispatch,
                assets.ErenLocation,
                assets.MertLocation,
                assets.MainMenuLocation,
                assets.MedicationInspection,
                assets.ForeshadowInspection,
                assets.MertPhotoInspection,
                assets.DeathTimeInspection,
                assets.CoffeeInspection,
                assets.ImplantInspection,
                assets.DoorInspection,
                assets.NotesInspection,
                assets.MedicalInspection,
                assets.ErenDeviceInspection,
                assets.MertPhotoEvidence,
                assets.MertTerminalEvidence,
                assets.MertDeathTimeEvidence,
                assets.DamagedImplantEvidence,
                assets.DoorStatusEvidence,
                assets.CallRecordEvidence,
                assets.MemoryDeletionEvidence,
                assets.ErenCallHistoryEvidence,
                assets.PostmortemDeduction,
                assets.SuicideInconsistentDeduction,
                assets.LockedRoomDeduction,
                assets.DispatchTerminal,
                assets.MertTerminal,
                assets.PhotoMemory,
                assets.MertRecorderDialogue,
                assets.ChapterEndDialogue,
                assets.TimelineClaim
            }.Concat(assets.Objectives)
                .Concat(new[]
                {
                    assets.ErenStartCheckpoint,
                    assets.DispatchCheckpoint,
                    assets.MertEntranceCheckpoint,
                    assets.CriticalEvidenceCheckpoint,
                    assets.FirstDeductionCheckpoint
                })
                .Concat(assets.JournalEntries)
                .ToArray();
            SerializedObject serialized = new SerializedObject(assets.Catalog);
            SetObjectArray(serialized.FindProperty("_allContent"), allContent);
            SetObjectArray(
                serialized.FindProperty("_evidence"),
                new[]
                {
                    assets.MertPhotoEvidence,
                    assets.MertTerminalEvidence,
                    assets.MertDeathTimeEvidence,
                    assets.DamagedImplantEvidence,
                    assets.DoorStatusEvidence,
                    assets.CallRecordEvidence,
                    assets.MemoryDeletionEvidence,
                    assets.ErenCallHistoryEvidence
                });
            SetObjectArray(
                serialized.FindProperty("_deductions"),
                new[] { assets.PostmortemDeduction, assets.SuicideInconsistentDeduction, assets.LockedRoomDeduction });
            SetObjectArray(serialized.FindProperty("_objectives"), assets.Objectives);
            SetObjectArray(
                serialized.FindProperty("_checkpoints"),
                new[]
                {
                    assets.ErenStartCheckpoint,
                    assets.DispatchCheckpoint,
                    assets.MertEntranceCheckpoint,
                    assets.CriticalEvidenceCheckpoint,
                    assets.FirstDeductionCheckpoint
                });
            SetObjectArray(serialized.FindProperty("_journalEntries"), assets.JournalEntries);
            SetObjectArray(serialized.FindProperty("_interrogationClaims"), new[] { assets.TimelineClaim });
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
                    assets.MainMenuLocation,
                    assets.ErenStartCheckpoint,
                    "entry");

                var cameraObject = new GameObject("Bootstrap Camera");
                cameraObject.tag = "MainCamera";
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = BackgroundColor;
                cameraObject.AddComponent<AudioListener>();
            });
        }

        private static void BuildMainMenuScene()
        {
            BuildScene(MainMenuScenePath, () =>
            {
                CreateCamera();
                var eventSystemObject = new GameObject("Event System");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<InputSystemUIInputModule>();

                var canvasObject = new GameObject("Main Menu Canvas", typeof(RectTransform));
                Canvas canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObject.AddComponent<GraphicRaycaster>();

                VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
                GameObject backdrop = CreateImage(
                    canvasObject.transform,
                    "Noir Backdrop",
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    Vector2.zero,
                    BackgroundColor);
                backdrop.AddComponent<VisualRootAnchor>().Configure("visual.main_menu", VisualRootKind.MainMenu, theme);
                var distantLights = new List<CanvasGroup>();
                GameObject farGlow = CreateImage(backdrop.transform, "Far City Value Field", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.025f, 0.065f, 0.105f, 0.48f));
                CreateImage(farGlow.transform, "Horizon Value Band", new Vector2(0f, 0.08f), new Vector2(1f, 0.5f), Vector2.zero, Vector2.zero, new Color(0.06f, 0.13f, 0.17f, 0.22f));
                CreateProceduralCityLayer(farGlow.transform, "FarCity Procedural Preview", 14, 690f, 122f, 420f, new Color(0.022f, 0.055f, 0.078f, 0.96f), distantLights, true);
                CreateEmptyUiArtSlot(backdrop.transform, "Far City Final Art", "slot.main_menu.far_city", "NP-MENU-BG-FAR-001");

                GameObject horizon = CreateRect(backdrop.transform, "Mid City Architecture Preview", new Vector2(0f, 0.08f), new Vector2(1f, 0.58f), Vector2.zero, Vector2.zero);
                CreateProceduralCityLayer(horizon.transform, "MidCity Procedural Preview", 9, 820f, 176f, 610f, new Color(0.018f, 0.043f, 0.06f, 0.98f), distantLights, false);
                GameObject midArt = CreateEmptyUiArtSlot(backdrop.transform, "Mid City Final Art", "slot.main_menu.mid_city", "NP-MENU-BG-MID-001");
                RectTransform midArtRect = midArt.GetComponent<RectTransform>();
                midArtRect.anchorMin = new Vector2(0f, 0.08f);
                midArtRect.anchorMax = new Vector2(1f, 0.62f);
                midArtRect.sizeDelta = Vector2.zero;
                GameObject motif = CreateEmptyUiArtSlot(backdrop.transform, "Narrative Motif", "slot.main_menu.narrative_motif", "NP-MENU-MOTIF-001");
                RectTransform motifRect = motif.GetComponent<RectTransform>();
                motifRect.anchorMin = new Vector2(0.54f, 0.1f);
                motifRect.anchorMax = new Vector2(0.96f, 0.88f);
                motifRect.sizeDelta = Vector2.zero;

                GameObject near = CreateRect(backdrop.transform, "Near Window Procedural Preview", new Vector2(0.58f, 0f), Vector2.one, Vector2.zero, Vector2.zero);
                RectTransform nearRect = near.GetComponent<RectTransform>();
                CreateImage(near.transform, "Near Glass Shade", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.006f, 0.015f, 0.028f, 0.42f));
                CreateImage(near.transform, "Window Mullion A", new Vector2(0.28f, 0f), new Vector2(0.28f, 1f), Vector2.zero, new Vector2(24f, 0f), new Color(0.008f, 0.018f, 0.032f, 0.96f));
                CreateImage(near.transform, "Window Mullion B", new Vector2(0.77f, 0f), new Vector2(0.77f, 1f), Vector2.zero, new Vector2(14f, 0f), new Color(0.008f, 0.018f, 0.032f, 0.88f));
                CreateImage(near.transform, "Window Header", new Vector2(0f, 0.84f), new Vector2(1f, 0.84f), Vector2.zero, new Vector2(0f, 30f), new Color(0.008f, 0.018f, 0.032f, 0.94f));
                CreateImage(near.transform, "Interior Edge", new Vector2(0.93f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(0.003f, 0.008f, 0.016f, 0.94f));
                for (int reflection = 0; reflection < 5; reflection++)
                {
                    float x = 0.08f + reflection * 0.17f;
                    CreateImage(near.transform, $"Glass Reflection {reflection + 1:00}", new Vector2(x, 0.06f), new Vector2(x + 0.025f, 0.82f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.32f, 0.37f, 0.025f + reflection * 0.008f));
                }
                GameObject nearArt = CreateEmptyUiArtSlot(backdrop.transform, "Near Architecture Final Art", "slot.main_menu.near_architecture", "NP-MENU-BG-NEAR-001");
                RectTransform nearArtRect = nearArt.GetComponent<RectTransform>();
                nearArtRect.anchorMin = new Vector2(0.58f, 0f);
                nearArtRect.anchorMax = Vector2.one;
                nearArtRect.sizeDelta = Vector2.zero;

                GameObject haze = CreateRect(backdrop.transform, "Procedural Atmospheric Haze", new Vector2(-0.05f, 0.18f), new Vector2(1.05f, 0.76f), Vector2.zero, Vector2.zero);
                RectTransform hazeRect = haze.GetComponent<RectTransform>();
                CanvasGroup hazeGroup = haze.AddComponent<CanvasGroup>();
                CreateImage(haze.transform, "Haze Low", new Vector2(0f, 0.05f), new Vector2(0.72f, 0.42f), Vector2.zero, Vector2.zero, new Color(0.18f, 0.3f, 0.34f, 0.1f));
                CreateImage(haze.transform, "Haze Mid", new Vector2(0.24f, 0.28f), new Vector2(1f, 0.7f), Vector2.zero, Vector2.zero, new Color(0.12f, 0.22f, 0.29f, 0.08f));
                CreateImage(haze.transform, "Haze Warm Trace", new Vector2(0.55f, 0.48f), new Vector2(0.96f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.25f, 0.13f, 0.18f, 0.04f));
                GameObject fogArt = CreateEmptyUiArtSlot(backdrop.transform, "Fog Haze Final Art", "slot.main_menu.fog_haze", "NP-MENU-FX-FOG-001");
                RectTransform fogArtRect = fogArt.GetComponent<RectTransform>();
                fogArtRect.anchorMin = new Vector2(-0.05f, 0.18f);
                fogArtRect.anchorMax = new Vector2(1.05f, 0.76f);
                fogArtRect.sizeDelta = Vector2.zero;

                GameObject rain = CreateRect(backdrop.transform, "Procedural Rain", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                RectTransform[] streaks = CreateProceduralRain(rain.transform, out int farRainCount, out int midRainCount);
                GameObject fxRoot = new("Environment FX");
                fxRoot.transform.SetParent(backdrop.transform, false);
                EnvironmentFxLayer environmentFx = fxRoot.AddComponent<EnvironmentFxLayer>();
                environmentFx.Configure(Array.Empty<ParticleSystem>(), new[] { haze, rain }, true);

                GameObject titleRoot = CreateRect(backdrop.transform, "Title Block", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(148f, -140f), new Vector2(760f, 300f));
                titleRoot.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
                CanvasGroup titleGroup = titleRoot.AddComponent<CanvasGroup>();
                Text echo = CreateText(titleRoot.transform, "Chromatic Echo", "NULL\nPOINTER", Vector2.zero, Vector2.one, new Vector2(6f, -2f), Vector2.zero, 96, TextAnchor.MiddleLeft, new Color(0.75f, 0.18f, 0.3f, 0.9f));
                CanvasGroup echoGroup = echo.gameObject.AddComponent<CanvasGroup>();
                echoGroup.alpha = 0f;
                Text title = CreateText(titleRoot.transform, "Title", "NULL\nPOINTER", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, 96, TextAnchor.MiddleLeft, TextColor);
                title.lineSpacing = 0.78f;
                CreateText(titleRoot.transform, "Missing Reference", "[  REF: ∅  ]", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(520f, -32f), new Vector2(210f, 30f), 14, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.5f));
                CreateImage(titleRoot.transform, "Omission Block", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(438f, -172f), new Vector2(76f, 13f), new Color(Red.r, Red.g, Red.b, 0.72f));
                RectTransform titleFault = CreateImage(titleRoot.transform, "Displaced Title Segment", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(468f, -156f), new Vector2(136f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.76f)).GetComponent<RectTransform>();
                Text subtitle = CreateText(titleRoot.transform, "Subtitle", "ANILAR SİLİNMEDEN ÖNCE // NLP-0417", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 14f), new Vector2(700f, 42f), 20, TextAnchor.MiddleLeft, Cyan);
                subtitle.rectTransform.pivot = new Vector2(0f, 0.5f);
                GameObject titleRule = CreateImage(titleRoot.transform, "Title Rule A", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 54f), new Vector2(218f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f));
                titleRule.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
                GameObject titleRuleB = CreateImage(titleRoot.transform, "Title Rule B", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(244f, 54f), new Vector2(96f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.38f));
                titleRuleB.GetComponent<RectTransform>().pivot = new Vector2(0f, 0.5f);
                CreateText(titleRoot.transform, "Title Metadata", "PTR//0000:DEAD\nTARGET UNRESOLVED", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(510f, 20f), new Vector2(220f, 44f), 12, TextAnchor.MiddleLeft, new Color(TextColor.r, TextColor.g, TextColor.b, 0.34f));

                GameObject menuRoot = CreateRect(backdrop.transform, "Main Menu", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(150f, 112f), new Vector2(590f, 470f));
                menuRoot.GetComponent<RectTransform>().pivot = Vector2.zero;
                CanvasGroup menuGroup = menuRoot.AddComponent<CanvasGroup>();
                UiSoundHooks soundHooks = CreateUiSoundHooks(backdrop.transform);
                Text menuIndex = CreateText(menuRoot.transform, "Menu Index", "// ANA DİZİN", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -5f), new Vector2(560f, 32f), 17, TextAnchor.MiddleLeft, new Color(TextColor.r, TextColor.g, TextColor.b, 0.55f));
                menuIndex.rectTransform.pivot = new Vector2(0f, 1f);
                Button newGame = CreateCyberButton(menuRoot.transform, "New Game", "YENİ OYUN", "01", new Vector2(520f, 72f), new Vector2(0f, 335f), theme, soundHooks);
                Button continueGame = CreateCyberButton(menuRoot.transform, "Continue", "DEVAM ET", "02", new Vector2(480f, 66f), new Vector2(0f, 244f), theme, soundHooks);
                Button settings = CreateCyberButton(menuRoot.transform, "Settings", "AYARLAR", "03", new Vector2(440f, 62f), new Vector2(0f, 158f), theme, soundHooks);
                Button quit = CreateCyberButton(menuRoot.transform, "Quit", "ÇIKIŞ", "04", new Vector2(400f, 58f), new Vector2(0f, 76f), theme, soundHooks, true);
                Button[] mainMenuButtons = { newGame, continueGame, settings, quit };
                var optionRows = new UiTransition[mainMenuButtons.Length];
                for (int index = 0; index < mainMenuButtons.Length; index++)
                {
                    CanvasGroup rowGroup = mainMenuButtons[index].gameObject.AddComponent<CanvasGroup>();
                    UiTransition rowTransition = mainMenuButtons[index].gameObject.AddComponent<UiTransition>();
                    rowTransition.Configure(mainMenuButtons[index].GetComponent<RectTransform>(), rowGroup, true, true, false, new Vector2(-20f, 0f), Vector3.one, 0.16f);
                    optionRows[index] = rowTransition;
                }
                MainMenuPanel panel = menuRoot.AddComponent<MainMenuPanel>();
                panel.Configure(newGame, continueGame, settings, quit);

                GameObject overlay = CreateImage(backdrop.transform, "Screen Texture Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.08f, 0.17f, 0.2f, 0.12f));
                Image overlayImage = overlay.GetComponent<Image>();
                overlayImage.raycastTarget = false;
                AddFinalArtSlot(overlay, "slot.main_menu.screen_texture", "NP-MENU-FX-NOISE-001", uiImage: overlayImage);
                CanvasGroup overlayGroup = overlay.AddComponent<CanvasGroup>();
                ScreenOverlayLayer screenOverlay = overlay.AddComponent<ScreenOverlayLayer>();
                screenOverlay.Configure(overlayGroup, 0.12f, true);
                for (int index = 0; index < 18; index++)
                {
                    CreateImage(overlay.transform, $"Scanline {index + 1:00}", new Vector2(0f, index / 18f), new Vector2(1f, index / 18f), Vector2.zero, new Vector2(0f, 1f), new Color(0.35f, 0.55f, 0.58f, 0.06f));
                }

                UiTransition titleTransition = titleRoot.AddComponent<UiTransition>();
                titleTransition.Configure(titleRoot.GetComponent<RectTransform>(), titleGroup, true, true, false, new Vector2(-42f, 0f), Vector3.one, 0.46f);
                UiTransition optionsTransition = menuRoot.AddComponent<UiTransition>();
                optionsTransition.Configure(menuRoot.GetComponent<RectTransform>(), menuGroup, true, true, false, new Vector2(0f, -28f), Vector3.one, 0.38f);

                MenuAtmosphereController atmosphere = backdrop.AddComponent<MenuAtmosphereController>();
                atmosphere.Configure(
                    new[] { farGlow.GetComponent<RectTransform>(), horizon.GetComponent<RectTransform>(), motifRect, nearRect },
                    new[] { 4f, 8f, 14f, 22f },
                    streaks,
                    hazeGroup,
                    hazeRect,
                    titleRoot.GetComponent<RectTransform>(),
                    echoGroup,
                    screenOverlay,
                    environmentFx,
                    farRainCount,
                    midRainCount,
                    titleFault,
                    distantLights.ToArray());

                GameObject curtain = CreateImage(backdrop.transform, "Departure Curtain", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, BackgroundColor);
                CanvasGroup curtainGroup = curtain.AddComponent<CanvasGroup>();
                curtainGroup.alpha = 0f;
                curtainGroup.blocksRaycasts = false;
                MainMenuPresentation presentation = backdrop.AddComponent<MainMenuPresentation>();
                presentation.Configure(titleTransition, optionsTransition, curtainGroup, atmosphere, optionRows);
                SettingsPanel settingsPanel = CreateSettingsPanel(backdrop.transform, "Main Menu Settings");
                MainMenuController controller = backdrop.AddComponent<MainMenuController>();
                controller.Configure(panel, settingsPanel, presentation);
                MainMenuSceneInstaller installer = backdrop.AddComponent<MainMenuSceneInstaller>();
                installer.Configure(controller);
                settingsPanel.Hide();
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
                AddFinalArtSlot(medication, "slot.eren_apartment.medication", "NP-PROP-EREN-MEDICATION-001", medication.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                medication.AddComponent<InspectInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.MedicationInspection);

                GameObject photograph = CreateInteractableBlock(
                    "Cut Photograph",
                    new Vector3(-0.8f, -1.2f, 0f),
                    new Vector2(0.75f, 0.9f),
                    new Color(0.3f, 0.4f, 0.48f, 1f));
                AddFinalArtSlot(photograph, "slot.eren_apartment.cut_photo", "NP-PROP-EREN-CUTPHOTO-001", photograph.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                photograph.AddComponent<InspectInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.ForeshadowInspection);

                GameObject dispatch = CreateInteractableBlock(
                    "Dispatch Terminal",
                    new Vector3(1.75f, -1.15f, 0f),
                    new Vector2(1.15f, 1.15f),
                    Amber);
                AddFinalArtSlot(dispatch, "slot.eren_apartment.workstation", "NP-PROP-EREN-WORKSTATION-001", dispatch.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                dispatch.AddComponent<TerminalInteractable>().Configure(
                    setup.Ui.TerminalController,
                    assets.DispatchTerminal);

                GameObject exit = CreateInteractableBlock(
                    "Exit to Mert Apartment",
                    new Vector3(4.65f, -0.75f, 0f),
                    new Vector2(0.9f, 2f),
                    Red);
                AddFinalArtSlot(exit, "slot.eren_apartment.doorway", "NP-PROP-EREN-DOOR-001", exit.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
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
                    setup.Ui.InterrogationController,
                    setup.Ui.InteractionPrompt,
                    setup.Ui.EvidenceNotification,
                    new[] { transition },
                    new[] { setup.SpawnPoint });
                ProgressionCoordinator progression = setup.Installer.gameObject.AddComponent<ProgressionCoordinator>();
                progression.Configure(
                    setup.Ui.TerminalController,
                    new[]
                    {
                        new ProgressionMilestone(
                            ProgressionTriggerKind.SceneEntered,
                            assets.ErenLocation.StableId,
                            objectiveToStart: assets.Objectives[0],
                            checkpoint: assets.ErenStartCheckpoint),
                        new ProgressionMilestone(
                            ProgressionTriggerKind.TerminalEntryOpened,
                            "dispatch_0317",
                            assets.Objectives[0],
                            assets.Objectives[1],
                            assets.DispatchCheckpoint)
                    });
                setup.Installer.ConfigureProduction(
                    setup.Ui.PauseMenuController,
                    setup.Ui.ObjectivePresenter,
                    progression,
                    null);
                setup.Installer.ConfigureAudioHooks(CreateAudioHooks(
                    assets,
                    setup.Ui.TerminalController,
                    LocationAmbienceKind.DistantTraffic));
            });
        }

        private static void BuildMertApartmentScene(OpeningAssets assets)
        {
            BuildScene(MertScenePath, () =>
            {
                SceneSetup setup = CreateGameplayScene("MERT'İN DAİRESİ // OLAY YERİ", assets.MertLocation);

                GameObject rainWindow = CreateWorldBlock("Rain Window", new Vector3(-4.9f, 1.4f, 0f), new Vector2(2.2f, 2.8f), new Color(0.04f, 0.2f, 0.27f, 1f), GetPlaceholderSprite(), false, -2);
                AddFinalArtSlot(rainWindow, "slot.mert_apartment.rain_window", "NP-ENV-MERT-WINDOW-001", rainWindow.GetComponent<SpriteRenderer>());
                GameObject workstationPool = CreateWorldBlock("Workstation Pool", new Vector3(-1.8f, -0.7f, 0f), new Vector2(2.6f, 1.8f), new Color(0.08f, 0.18f, 0.2f, 1f), GetPlaceholderSprite(), false, -2);
                AddFinalArtSlot(workstationPool, "slot.mert_apartment.workstation", "NP-PROP-MERT-WORKSTATION-001", workstationPool.GetComponent<SpriteRenderer>());
                GameObject lockedInterior = CreateWorldBlock("Locked Interior", new Vector3(4.6f, 0.1f, 0f), new Vector2(3.1f, 4.4f), new Color(0.08f, 0.06f, 0.12f, 1f), GetPlaceholderSprite(), false, -3);
                AddFinalArtSlot(lockedInterior, "slot.mert_apartment.architecture", "NP-ENV-MERT-ARCH-001", lockedInterior.GetComponent<SpriteRenderer>());

                GameObject photograph = CreateInteractableBlock(
                    "Mert Photograph",
                    new Vector3(-4.95f, -1.2f, 0f),
                    new Vector2(0.75f, 0.95f),
                    Cyan);
                AddFinalArtSlot(photograph, "slot.mert_apartment.photograph", "NP-PROP-MERT-PHOTO-001", photograph.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                MemoryEvidenceTrigger memoryTrigger = photograph.AddComponent<MemoryEvidenceTrigger>();
                memoryTrigger.Configure(setup.Ui.MemoryController, assets.PhotoMemory);
                photograph.AddComponent<EvidenceInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.MertPhotoInspection,
                    assets.MertPhotoEvidence,
                    memoryTrigger);

                GameObject terminal = CreateInteractableBlock(
                    "Mert Terminal",
                    new Vector3(-2.35f, -1.1f, 0f),
                    new Vector2(1.1f, 1.25f),
                    Amber);
                AddFinalArtSlot(terminal, "slot.mert_apartment.terminal", "NP-PROP-MERT-TERMINAL-001", terminal.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                terminal.AddComponent<TerminalInteractable>().Configure(
                    setup.Ui.TerminalController,
                    assets.MertTerminal);

                GameObject deathTime = CreateInteractableBlock(
                    "Biometric Death Time",
                    new Vector3(0.45f, -1.35f, 0f),
                    new Vector2(0.75f, 0.6f),
                    Red);
                AddFinalArtSlot(deathTime, "slot.mert_apartment.death_record", "NP-PROP-MERT-DEATH-TIME-001", deathTime.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                deathTime.AddComponent<EvidenceInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.DeathTimeInspection,
                    assets.MertDeathTimeEvidence);

                GameObject board = CreateInteractableBlock(
                    "Evidence Board",
                    new Vector3(5.15f, -0.95f, 0f),
                    new Vector2(1.4f, 1.55f),
                    new Color(0.18f, 0.52f, 0.58f, 1f));
                AddFinalArtSlot(board, "slot.mert_apartment.evidence_board", "NP-PROP-MERT-EVIDENCE-BOARD-001", board.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                board.AddComponent<EvidenceBoardInteractable>().Configure(setup.Ui.EvidenceBoardController);

                GameObject recorder = CreateInteractableBlock(
                    "Damaged Recorder",
                    new Vector3(6.15f, -1.4f, 0f),
                    new Vector2(0.55f, 0.55f),
                    new Color(0.45f, 0.25f, 0.32f, 1f));
                AddFinalArtSlot(recorder, "slot.mert_apartment.recorder", "NP-PROP-MERT-RECORDER-001", recorder.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
                recorder.AddComponent<DialogueInteractable>().Configure(
                    setup.Ui.DialogueController,
                    assets.MertRecorderDialogue);

                CreateEvidenceObject("Damaged Memory Implant", -0.9f, assets.ImplantInspection, assets.DamagedImplantEvidence, setup);
                CreateEvidenceObject("Internal Door Latch", 1.75f, assets.DoorInspection, assets.DoorStatusEvidence, setup);
                CreateInspectionObject("Unfinished Coffee", -3.75f, assets.CoffeeInspection, setup, new Color(0.48f, 0.31f, 0.16f, 1f));
                CreateInspectionObject("Personal Notes", 3f, assets.NotesInspection, setup, new Color(0.55f, 0.48f, 0.3f, 1f));
                CreateInspectionObject("Medical Calibrator", 4.05f, assets.MedicalInspection, setup, new Color(0.3f, 0.4f, 0.52f, 1f));
                CreateEvidenceObject("Eren Device History", -6.1f, assets.ErenDeviceInspection, assets.ErenCallHistoryEvidence, setup);

                setup.Installer.Configure(
                    assets.MertLocation,
                    setup.Player,
                    setup.Detector,
                    setup.Ui.InspectController,
                    setup.Ui.DialogueController,
                    setup.Ui.TerminalController,
                    setup.Ui.MemoryController,
                    setup.Ui.EvidenceBoardController,
                    setup.Ui.InterrogationController,
                    setup.Ui.InteractionPrompt,
                    setup.Ui.EvidenceNotification,
                    Array.Empty<SceneTransitionInteractable>(),
                    new[] { setup.SpawnPoint });
                ProgressionCoordinator progression = setup.Installer.gameObject.AddComponent<ProgressionCoordinator>();
                progression.Configure(
                    setup.Ui.TerminalController,
                    new[]
                    {
                        new ProgressionMilestone(ProgressionTriggerKind.SceneEntered, assets.MertLocation.StableId, assets.Objectives[1], assets.Objectives[2], assets.MertEntranceCheckpoint),
                        new ProgressionMilestone(ProgressionTriggerKind.EvidenceCollected, assets.DamagedImplantEvidence.StableId, assets.Objectives[2], assets.Objectives[3]),
                        new ProgressionMilestone(ProgressionTriggerKind.EvidenceCollected, assets.MertDeathTimeEvidence.StableId, assets.Objectives[3], assets.Objectives[4], assets.CriticalEvidenceCheckpoint),
                        new ProgressionMilestone(ProgressionTriggerKind.EvidenceCollected, assets.MertTerminalEvidence.StableId, assets.Objectives[4], assets.Objectives[5]),
                        new ProgressionMilestone(ProgressionTriggerKind.MemoryUnlocked, assets.PhotoMemory.StableId, assets.Objectives[5], assets.Objectives[6]),
                        new ProgressionMilestone(ProgressionTriggerKind.DeductionCompleted, assets.PostmortemDeduction.StableId, checkpoint: assets.FirstDeductionCheckpoint),
                        new ProgressionMilestone(ProgressionTriggerKind.DeductionCompleted, assets.LockedRoomDeduction.StableId, assets.Objectives[6], storyFlagToSet: "flag.ch01.completed", chapterIdToComplete: "chapter.01")
                    });
                ChapterEndSequenceController chapterEnd = setup.Installer.gameObject.AddComponent<ChapterEndSequenceController>();
                chapterEnd.Configure(
                    assets.LockedRoomDeduction.StableId,
                    assets.ChapterEndDialogue,
                    setup.Ui.DialogueController);
                setup.Installer.ConfigureProduction(
                    setup.Ui.PauseMenuController,
                    setup.Ui.ObjectivePresenter,
                    progression,
                    chapterEnd);
                setup.Installer.ConfigureAudioHooks(CreateAudioHooks(
                    assets,
                    setup.Ui.TerminalController,
                    LocationAmbienceKind.Rain,
                    assets.LockedRoomDeduction.StableId));
            });
        }

        private static GameplayAudioHooks CreateAudioHooks(
            OpeningAssets assets,
            TerminalController terminal,
            LocationAmbienceKind ambience,
            string chapterEndDeductionId = "")
        {
            var root = new GameObject("Authored Audio Hooks");
            AudioSource ambienceSource = root.AddComponent<AudioSource>();
            ambienceSource.playOnAwake = false;
            ambienceSource.loop = true;
            AudioSource sfxSource = root.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            AudioCuePlayer player = root.AddComponent<AudioCuePlayer>();
            player.Configure(assets.AudioCues, ambienceSource, sfxSource);
            GameplayAudioHooks hooks = root.AddComponent<GameplayAudioHooks>();
            hooks.Configure(player, terminal, ambience, chapterEndDeductionId);
            return hooks;
        }

        private static void CreateEvidenceObject(
            string name,
            float x,
            InspectData inspection,
            EvidenceData evidence,
            SceneSetup setup)
        {
            GameObject item = CreateInteractableBlock(name, new Vector3(x, -1.3f, 0f), new Vector2(0.62f, 0.72f), Cyan);
            (string slotId, string assetId) = ResolveMertPropSlot(name);
            AddFinalArtSlot(item, slotId, assetId, item.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
            item.AddComponent<EvidenceInteractable>().Configure(setup.Ui.InspectController, inspection, evidence);
        }

        private static void CreateInspectionObject(
            string name,
            float x,
            InspectData inspection,
            SceneSetup setup,
            Color color)
        {
            GameObject item = CreateInteractableBlock(name, new Vector3(x, -1.35f, 0f), new Vector2(0.55f, 0.58f), color);
            (string slotId, string assetId) = ResolveMertPropSlot(name);
            AddFinalArtSlot(item, slotId, assetId, item.GetComponent<SpriteRenderer>(), showStructuralPlaceholder: false);
            item.AddComponent<InspectInteractable>().Configure(setup.Ui.InspectController, inspection);
        }

        private static SceneSetup CreateGameplayScene(string sceneTitle, LocationData location)
        {
            Sprite sprite = GetPlaceholderSprite();
            CreateCamera();
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            var visualRoot = new GameObject("Visual Root");
            visualRoot.AddComponent<VisualRootAnchor>().Configure(
                $"visual.{location.StableId}",
                VisualRootKind.GameplayLocation,
                theme);
            GameObject backdrop = CreateWorldBlock("Backdrop", Vector3.zero, new Vector2(15f, 9f), BackgroundColor, sprite, false, -10);
            AddFinalArtSlot(backdrop, $"slot.{location.StableId}.room_architecture", ResolveEnvironmentManifestId(location, "ARCH"), backdrop.GetComponent<SpriteRenderer>());
            GameObject floor = CreateWorldBlock(
                "Floor",
                new Vector3(0f, -2.55f, 0f),
                new Vector2(15f, 1f),
                new Color(0.07f, 0.1f, 0.13f, 1f),
                sprite,
                true,
                -1);
            AddFinalArtSlot(floor, $"slot.{location.StableId}.floor_walls", ResolveEnvironmentManifestId(location, "ROOM"), floor.GetComponent<SpriteRenderer>());
            CreateBoundary("Left Boundary", new Vector3(-6.8f, 0f, 0f), new Vector2(0.5f, 7f));
            CreateBoundary("Right Boundary", new Vector3(6.8f, 0f, 0f), new Vector2(0.5f, 7f));

            TextMesh title = CreateWorldText(sceneTitle, new Vector3(-6.1f, 3.85f, 0f), TextAnchor.UpperLeft, Cyan);
            title.name = "Location Title";

            var spawnObject = new GameObject("SpawnPoint_entry");
            spawnObject.transform.position = new Vector3(-5.2f, -1.15f, 0f);
            SpawnPoint spawn = spawnObject.AddComponent<SpawnPoint>();
            spawn.Configure("entry");

            PlayerController player = CreatePlayer(sprite, location.StableId, out PlayerInteractionDetector detector);
            SceneUi ui = CreateSceneUi(location.StableId);
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

        private static PlayerController CreatePlayer(
            Sprite sprite,
            string locationId,
            out PlayerInteractionDetector detector)
        {
            var playerObject = new GameObject("Player");
            playerObject.transform.position = new Vector3(-5.2f, -1.15f, 0f);
            playerObject.transform.localScale = new Vector3(0.72f, 1.35f, 1f);
            SpriteRenderer renderer = playerObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = new Color(0.12f, 0.18f, 0.22f, 1f);
            renderer.sortingOrder = 2;
            AddFinalArtSlot(playerObject, $"slot.{locationId}.eren", "NP-CHR-EREN-IDLE-001", renderer);
            Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezePositionY |
                               RigidbodyConstraints2D.FreezeRotation;
            playerObject.AddComponent<BoxCollider2D>();
            PlayerController player = playerObject.AddComponent<PlayerController>();
            player.SetMoveSpeed(4f);
            detector = playerObject.AddComponent<PlayerInteractionDetector>();
            detector.SetDetection(1.45f, ~0);
            return player;
        }

        private static SceneUi CreateSceneUi(string locationId)
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
                InterrogationController = systems.AddComponent<InterrogationController>(),
                InteractionPrompt = systems.AddComponent<InteractionPromptController>(),
                EvidenceNotification = systems.AddComponent<EvidenceNotificationController>(),
                PauseMenuController = systems.AddComponent<PauseMenuController>(),
                ObjectivePresenter = systems.AddComponent<ObjectivePresenter>()
            };

            ConfigureHud(canvasObject.transform, ui);
            ConfigureObjectiveHud(canvasObject.transform, ui);
            ConfigurePauseInterface(canvasObject.transform, ui);
            ConfigureInspectPanel(canvasObject.transform, ui);
            ConfigureDialoguePanel(canvasObject.transform, ui, locationId);
            ConfigureTerminalPanel(canvasObject.transform, ui);
            ConfigureMemoryPanel(canvasObject.transform, ui);
            ConfigureEvidenceBoardPanel(canvasObject.transform, ui);
            ConfigureInterrogationPanel(canvasObject.transform, ui, locationId);
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
                new Vector2(-390f, -150f),
                new Vector2(680f, 220f),
                new Color(0.012f, 0.032f, 0.048f, 0.98f));
            CreateImage(notificationRoot.transform, "Card Shadow", Vector2.zero, Vector2.one, new Vector2(14f, -14f), new Vector2(-4f, -4f), new Color(0f, 0f, 0f, 0.42f));
            CreateImage(notificationRoot.transform, "Signal Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(5f, 0f), new Vector2(4f, -24f), Cyan);
            Text category = CreateText(notificationRoot.transform, "Category", string.Empty, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(51f, -34f), new Vector2(-222f, 30f), 15, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.78f));
            GameObject thumbnailFrame = CreateImage(notificationRoot.transform, "Thumbnail Frame", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(76f, -8f), new Vector2(112f, 132f), new Color(0.06f, 0.1f, 0.12f, 0.9f));
            Image thumbnail = CreateImage(thumbnailFrame.transform, "Thumbnail", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-12f, -12f), Color.white).GetComponent<Image>();
            thumbnail.enabled = false;
            GameObject thumbnailFallback = CreateProceduralEvidenceFallback(thumbnailFrame.transform, "Procedural Evidence Fallback", Cyan);
            Text notification = CreateText(
                notificationRoot.transform,
                "Notification Label",
                string.Empty,
                new Vector2(0f, 0.5f),
                new Vector2(1f, 0.5f),
                new Vector2(51f, 14f),
                new Vector2(-222f, 68f),
                25,
                TextAnchor.MiddleLeft,
                TextColor);
            Text metadata = CreateText(notificationRoot.transform, "Metadata", string.Empty, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(51f, 32f), new Vector2(-222f, 38f), 12, TextAnchor.MiddleLeft, new Color(Amber.r, Amber.g, Amber.b, 0.75f));
            RectTransform scan = CreateImage(notificationRoot.transform, "Acquire Scan", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(138f, 0f), new Vector2(2f, 162f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.18f)).GetComponent<RectTransform>();
            GameObject pulseObject = CreateImage(notificationRoot.transform, "Acquire Pulse", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-10f, -10f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.16f));
            CanvasGroup pulse = pulseObject.AddComponent<CanvasGroup>();
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(notificationRoot, notificationRoot, new[] { category.gameObject, notification.gameObject, metadata.gameObject }, scan, pulse, new Vector2(28f, 0f), 0.2f);
            ui.EvidenceNotification.Configure(notificationRoot, notification);
            ui.EvidenceNotification.ConfigurePresentation(presentation, category, metadata, thumbnail, thumbnailFallback);
            notificationRoot.SetActive(false);
        }

        private static void ConfigureObjectiveHud(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateImage(
                canvas,
                "Objective HUD",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(340f, -94f),
                new Vector2(620f, 116f),
                new Color(0.01f, 0.03f, 0.047f, 0.9f));
            CreateImage(root.transform, "Objective Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(5f, 0f), new Vector2(4f, -18f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.82f));
            Text state = CreateText(root.transform, "Objective State", string.Empty, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -22f), new Vector2(-46f, 26f), 13, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f));
            Text label = CreateText(root.transform, "Objective", string.Empty, Vector2.zero, Vector2.one, new Vector2(28f, -12f), new Vector2(-46f, -50f), 23, TextAnchor.MiddleLeft, TextColor);
            CreateImage(root.transform, "Objective Rule", new Vector2(0f, 0f), new Vector2(0.7f, 0f), new Vector2(30f, 7f), new Vector2(-28f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.24f));
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(root, root, new[] { state.gameObject, label.gameObject }, null, null, new Vector2(-20f, 0f), 0.2f);
            ui.ObjectivePresenter.Configure(root, label);
            ui.ObjectivePresenter.ConfigurePresentation(presentation, state);
        }

        private static void ConfigurePauseInterface(Transform canvas, SceneUi ui)
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            GameObject pauseRoot = CreateModalRoot(canvas, "Pause Menu", new Color(0.008f, 0.016f, 0.028f, 0.64f));
            CanvasGroup pauseGroup = pauseRoot.AddComponent<CanvasGroup>();
            CreateImage(pauseRoot.transform, "Scene Desaturation Veil", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.055f, 0.075f, 0.09f, 0.34f));
            CreateImage(pauseRoot.transform, "Left Depth Field", new Vector2(0f, 0f), new Vector2(0.48f, 1f), Vector2.zero, Vector2.zero, new Color(0.012f, 0.027f, 0.045f, 0.82f));
            CreateImage(pauseRoot.transform, "Scene Light Pool", new Vector2(0.58f, 0.12f), new Vector2(0.88f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.08f, 0.18f, 0.21f, 0.11f));
            CreateImage(pauseRoot.transform, "Frozen Scene Band", new Vector2(0.48f, 0.69f), new Vector2(1f, 0.73f), Vector2.zero, Vector2.zero, new Color(Cyan.r, Cyan.g, Cyan.b, 0.035f));
            CreateImage(pauseRoot.transform, "Vignette Left", new Vector2(0f, 0f), new Vector2(0.035f, 1f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.48f));
            CreateImage(pauseRoot.transform, "Vignette Right", new Vector2(0.965f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.48f));
            GameObject scanlines = CreateRect(pauseRoot.transform, "Pause Screen Texture", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup scanlineGroup = scanlines.AddComponent<CanvasGroup>();
            for (int index = 0; index < 16; index++)
            {
                CreateImage(scanlines.transform, $"Line {index + 1:00}", new Vector2(0f, index / 16f), new Vector2(1f, index / 16f), Vector2.zero, new Vector2(0f, 1f), new Color(0.3f, 0.48f, 0.52f, 0.06f));
            }
            ScreenOverlayLayer pauseOverlay = scanlines.AddComponent<ScreenOverlayLayer>();
            pauseOverlay.Configure(scanlineGroup, 0.1f, true);

            CreateImage(pauseRoot.transform, "Pause Shadow", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(408f, -14f), new Vector2(678f, 838f), new Color(0f, 0f, 0f, 0.52f));
            GameObject frame = CreateImage(pauseRoot.transform, "Pause Frame", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(390f, 0f), new Vector2(650f, 810f), new Color(0.016f, 0.037f, 0.052f, 0.96f));
            CreateImage(frame.transform, "Inner Value Field", new Vector2(0f, 0f), new Vector2(1f, 0.78f), new Vector2(18f, 10f), new Vector2(-42f, -24f), new Color(0.035f, 0.063f, 0.075f, 0.44f));
            CreateImage(frame.transform, "Accent Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(6f, 0f), new Vector2(4f, -34f), Cyan);
            GameObject edgeSignal = CreateImage(frame.transform, "Edge Signal", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(98f, -7f), new Vector2(176f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f));
            CanvasGroup edgeSignalGroup = edgeSignal.AddComponent<CanvasGroup>();
            edgeSignalGroup.alpha = 0.42f;
            Text pauseHeading = CreateText(frame.transform, "Heading", "DURAKLATILDI", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -76f), new Vector2(500f, 64f), 38, TextAnchor.MiddleLeft, TextColor);
            pauseHeading.rectTransform.pivot = new Vector2(0f, 0.5f);
            Text pauseMode = CreateText(frame.transform, "Mode", "// SAHNE AKIŞI BEKLEMEDE", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(67f, -126f), new Vector2(480f, 34f), 16, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.65f));
            pauseMode.rectTransform.pivot = new Vector2(0f, 0.5f);
            CreateImage(frame.transform, "Header Rule A", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(67f, -164f), new Vector2(280f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.28f));
            CreateImage(frame.transform, "Header Rule B", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(390f, -164f), new Vector2(112f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.1f));
            UiSoundHooks soundHooks = CreateUiSoundHooks(pauseRoot.transform);
            Button resume = CreateCyberButton(frame.transform, "Resume", "DEVAM", "01", new Vector2(500f, 68f), new Vector2(66f, 502f), theme, soundHooks);
            Button journal = CreateCyberButton(frame.transform, "Journal", "SORUŞTURMA DEFTERİ", "02", new Vector2(470f, 64f), new Vector2(66f, 414f), theme, soundHooks);
            Button settingsButton = CreateCyberButton(frame.transform, "Settings", "AYARLAR", "03", new Vector2(440f, 60f), new Vector2(66f, 330f), theme, soundHooks);
            Button mainMenu = CreateCyberButton(frame.transform, "Main Menu", "ANA MENÜ", "04", new Vector2(410f, 58f), new Vector2(66f, 250f), theme, soundHooks, true);
            Text pauseHint = CreateText(frame.transform, "Hint", "ESC  GERİ DÖN  •  ENTER  ONAYLA", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(67f, 76f), new Vector2(500f, 36f), 15, TextAnchor.MiddleLeft, new Color(TextColor.r, TextColor.g, TextColor.b, 0.5f));
            pauseHint.rectTransform.pivot = new Vector2(0f, 0.5f);
            CreateText(pauseRoot.transform, "Scene Telemetry", "SECTOR 07 / FRAME HELD\nLOCAL MEMORY LINK: STABLE\nCASE NLP-0417 // ACTIVE", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-390f, 130f), new Vector2(620f, 120f), 18, TextAnchor.LowerRight, new Color(Cyan.r, Cyan.g, Cyan.b, 0.34f));
            CreateText(pauseRoot.transform, "Freeze Metadata", "SIMULATION CLOCK\n00:00:00 // SUSPENDED", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-312f, -110f), new Vector2(470f, 70f), 14, TextAnchor.UpperRight, new Color(TextColor.r, TextColor.g, TextColor.b, 0.26f));
            RectTransform scanLine = CreateImage(pauseRoot.transform, "Pause Scan Line", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(58f, 0f), new Vector2(2f, 760f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.04f)).GetComponent<RectTransform>();
            PausePresentationFx presentationFx = pauseRoot.AddComponent<PausePresentationFx>();
            presentationFx.Configure(scanLine, edgeSignalGroup, 58f, 704f);
            PauseMenuPanel pausePanel = pauseRoot.AddComponent<PauseMenuPanel>();
            pausePanel.Configure(resume, journal, settingsButton, mainMenu);
            UiTransition pauseTransition = pauseRoot.AddComponent<UiTransition>();
            pauseTransition.Configure(frame.GetComponent<RectTransform>(), pauseGroup, true, true, false, new Vector2(-38f, 0f), Vector3.one, 0.22f);
            Button[] pauseButtons = { resume, journal, settingsButton, mainMenu };
            var rowTransitions = new UiTransition[pauseButtons.Length];
            for (int index = 0; index < pauseButtons.Length; index++)
            {
                CanvasGroup rowGroup = pauseButtons[index].gameObject.AddComponent<CanvasGroup>();
                UiTransition rowTransition = pauseButtons[index].gameObject.AddComponent<UiTransition>();
                rowTransition.Configure(pauseButtons[index].GetComponent<RectTransform>(), rowGroup, true, true, false, new Vector2(-22f, 0f), Vector3.one, 0.16f);
                rowTransitions[index] = rowTransition;
            }
            pausePanel.ConfigurePresentation(pauseTransition, pauseOverlay, presentationFx, rowTransitions);

            GameObject journalRoot = CreateModalRoot(canvas, "Investigation Journal", Color.clear);
            CreatePhase5CBackdrop(journalRoot, Cyan);
            GameObject journalFrame = CreateImage(journalRoot.transform, "Journal Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1660f, 900f), new Color(0.014f, 0.032f, 0.048f, 0.985f));
            CreateImage(journalFrame.transform, "Paper Memory", new Vector2(0.22f, 0f), Vector2.one, Vector2.zero, new Vector2(-44f, -44f), new Color(0.12f, 0.13f, 0.13f, 0.18f));
            CreateImage(journalFrame.transform, "Binding Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(26f, 0f), new Vector2(5f, -44f), new Color(Amber.r, Amber.g, Amber.b, 0.62f));
            CreateText(journalFrame.transform, "Identity", "EREN VARDAR / SORUŞTURMA DEFTERİ", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(80f, -42f), new Vector2(620f, 30f), 15, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f));
            Text heading = CreateText(journalFrame.transform, "Heading", "DOSYALAR", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(430f, -86f), new Vector2(680f, 62f), 39, TextAnchor.MiddleLeft, TextColor);
            Text sectionMetadata = CreateText(journalFrame.transform, "Section Metadata", string.Empty, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-250f, -68f), new Vector2(400f, 52f), 14, TextAnchor.MiddleRight, new Color(Cyan.r, Cyan.g, Cyan.b, 0.55f));
            CreateImage(journalFrame.transform, "Header Rule", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(90f, -132f), new Vector2(-180f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.2f));
            GameObject contentPane = CreateImage(journalFrame.transform, "Case Content Pane", new Vector2(0f, 0f), Vector2.one, new Vector2(240f, -44f), new Vector2(-520f, -198f), new Color(0.025f, 0.05f, 0.062f, 0.56f));
            CreateImage(contentPane.transform, "Section Spine", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(26f, 0f), new Vector2(2f, -48f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.24f));
            for (int index = 0; index < 4; index++)
            {
                float anchorY = 0.82f - index * 0.2f;
                CreateImage(contentPane.transform, $"Section Node {index + 1}", new Vector2(0f, anchorY), new Vector2(0f, anchorY), new Vector2(26f, 0f), new Vector2(8f, 8f), new Color(Amber.r, Amber.g, Amber.b, index == 0 ? 0.7f : 0.22f));
                CreateImage(contentPane.transform, $"Section Guide {index + 1}", new Vector2(0f, anchorY), new Vector2(0.72f, anchorY), new Vector2(52f, 0f), new Vector2(-86f, 1f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.08f));
            }
            Text content = CreateText(contentPane.transform, "Content", string.Empty, Vector2.zero, Vector2.one, new Vector2(58f, -16f), new Vector2(-98f, -74f), 23, TextAnchor.UpperLeft, TextColor);
            VisualTheme journalTheme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            UiSoundHooks journalSounds = CreateUiSoundHooks(journalRoot.transform);
            Button cases = CreatePhase5CButton(journalFrame.transform, "Cases", "DOSYALAR", "01", new Vector2(300f, 62f), new Vector2(-630f, 245f), journalTheme, journalSounds);
            Button people = CreatePhase5CButton(journalFrame.transform, "People", "KİŞİLER", "02", new Vector2(300f, 62f), new Vector2(-630f, 170f), journalTheme, journalSounds);
            Button evidence = CreatePhase5CButton(journalFrame.transform, "Evidence", "KANITLAR", "03", new Vector2(300f, 62f), new Vector2(-630f, 95f), journalTheme, journalSounds);
            Button questions = CreatePhase5CButton(journalFrame.transform, "Questions", "SORULAR", "04", new Vector2(300f, 62f), new Vector2(-630f, 20f), journalTheme, journalSounds);
            Button timeline = CreatePhase5CButton(journalFrame.transform, "Timeline", "ZAMAN ÇİZELGESİ", "05", new Vector2(300f, 62f), new Vector2(-630f, -55f), journalTheme, journalSounds);
            CreateText(journalFrame.transform, "Question Signal", "SORULAR, ÇÖZÜLMEMİŞ BAĞLANTILARI\nVE ÇELİŞKİLERİ ÖNCELİKLENDİRİR.", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(190f, 80f), new Vector2(300f, 64f), 13, TextAnchor.LowerLeft, new Color(Amber.r, Amber.g, Amber.b, 0.58f));
            Button journalBack = CreatePhase5CButton(journalFrame.transform, "Back", "GERİ", "ESC", new Vector2(220f, 56f), new Vector2(680f, -378f), journalTheme, journalSounds, true);
            RectTransform journalScan = CreateImage(journalFrame.transform, "Journal Scan", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(390f, 0f), new Vector2(2f, 680f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.08f)).GetComponent<RectTransform>();
            CyberNoirPanelPresentation journalPresentation = CreatePhase5CPresentation(journalRoot, journalFrame, new[] { heading.gameObject, contentPane, cases.gameObject, people.gameObject, evidence.gameObject, questions.gameObject, timeline.gameObject }, journalScan, null, new Vector2(-24f, 0f));
            JournalPanel journalPanel = journalRoot.AddComponent<JournalPanel>();
            journalPanel.Configure(heading, content, cases, people, evidence, questions, timeline, journalBack);
            journalPanel.ConfigurePresentation(journalPresentation, sectionMetadata, contentPane.GetComponent<RectTransform>());

            SettingsPanel settingsPanel = CreateSettingsPanel(canvas, "Pause Settings");
            ui.PauseMenuController.Configure(pausePanel, journalPanel, settingsPanel);
            pauseRoot.SetActive(false);
            journalRoot.SetActive(false);
            settingsPanel.Hide();
        }

        private static void ConfigureInspectPanel(Transform canvas, SceneUi ui)
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            GameObject root = CreateModalRoot(canvas, "Inspect Panel", Color.clear);
            CreatePhase5CBackdrop(root, Cyan);
            GameObject frame = CreateImage(
                root.transform,
                "Inspect Frame",
                new Vector2(0.5f, 0.5f),
                new Vector2(0.5f, 0.5f),
                Vector2.zero,
                new Vector2(1360f, 760f),
                new Color(0.014f, 0.034f, 0.05f, 0.985f));
            CreateImage(frame.transform, "Frame Shadow", Vector2.zero, Vector2.one, new Vector2(16f, -16f), new Vector2(8f, 8f), new Color(0f, 0f, 0f, 0.42f));
            CreateImage(frame.transform, "Analysis Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(7f, 0f), new Vector2(4f, -32f), Cyan);
            Text classification = CreateText(frame.transform, "Classification", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, -45f), new Vector2(560f, 28f), 15, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f));
            Text title = CreateText(frame.transform, "Title", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(82f, -94f), new Vector2(760f, 64f), 40, TextAnchor.MiddleLeft, TextColor);
            CreateImage(frame.transform, "Header Rule", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(80f, -140f), new Vector2(-160f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.2f));
            GameObject focusBay = CreateImage(frame.transform, "Object Focus Bay", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(302f, 308f), new Vector2(440f, 460f), new Color(0.025f, 0.048f, 0.057f, 0.84f));
            CreateImage(focusBay.transform, "Bracket Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(8f, 0f), new Vector2(3f, -28f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.65f));
            CreateImage(focusBay.transform, "Bracket Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -8f), new Vector2(-28f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.32f));
            Image art = CreateImage(focusBay.transform, "Object Art Slot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(330f, 330f), Color.white).GetComponent<Image>();
            art.enabled = false;
            GameObject artFallback = CreateProceduralEvidenceFallback(focusBay.transform, "Forensic Object Fallback", Cyan);
            Text metadata = CreateText(frame.transform, "Metadata", string.Empty, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(302f, 62f), new Vector2(440f, 104f), 15, TextAnchor.UpperLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.62f));
            GameObject copyPane = CreateImage(frame.transform, "Analysis Copy", new Vector2(0f, 0f), Vector2.one, new Vector2(250f, -16f), new Vector2(-600f, -210f), new Color(0.02f, 0.045f, 0.06f, 0.58f));
            Text description = CreateText(copyPane.transform, "Description", string.Empty, Vector2.zero, Vector2.one, new Vector2(52f, -32f), new Vector2(-90f, -120f), 26, TextAnchor.UpperLeft, TextColor);
            Text status = CreateText(frame.transform, "Status", string.Empty, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(818f, 92f), new Vector2(510f, 42f), 19, TextAnchor.MiddleLeft, Amber);
            UiSoundHooks sounds = CreateUiSoundHooks(root.transform);
            Button collect = CreatePhase5CButton(frame.transform, "Collect", "KANITI KAYDET", "01", new Vector2(260f, 58f), new Vector2(760f, -315f), theme, sounds);
            Button close = CreatePhase5CButton(frame.transform, "Close", "KAPAT", "ESC", new Vector2(220f, 58f), new Vector2(500f, -315f), theme, sounds, true);
            RectTransform scan = CreateImage(focusBay.transform, "Forensic Scan", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(-24f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.24f)).GetComponent<RectTransform>();
            GameObject pulseObject = CreateImage(frame.transform, "Evidence Resolve Pulse", new Vector2(0.44f, 0f), Vector2.one, Vector2.zero, new Vector2(-24f, -24f), new Color(Amber.r, Amber.g, Amber.b, 0.1f));
            CanvasGroup pulse = pulseObject.AddComponent<CanvasGroup>();
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(root, frame, new[] { classification.gameObject, title.gameObject, focusBay, copyPane, status.gameObject }, scan, pulse, new Vector2(0f, -24f));
            InspectPanel panel = root.AddComponent<InspectPanel>();
            panel.Configure(root, title, description, status, collect, close);
            panel.ConfigurePresentation(
                presentation,
                art,
                classification,
                metadata,
                artFallback,
                focusBay.GetComponent<RectTransform>(),
                copyPane.GetComponent<RectTransform>());
            ui.InspectController.Configure(panel);
            root.SetActive(false);
        }

        private static void ConfigureDialoguePanel(Transform canvas, SceneUi ui, string locationId)
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            GameObject root = CreateModalRoot(canvas, "Dialogue Panel", Color.clear);
            CreateImage(root.transform, "Dialogue Dim", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.004f, 0.008f, 0.015f, 0.48f));
            GameObject frame = CreateImage(root.transform, "Dialogue Frame", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 238f), new Vector2(1660f, 430f), new Color(0.012f, 0.028f, 0.044f, 0.97f));
            CreateImage(frame.transform, "Lower Shadow", Vector2.zero, Vector2.one, new Vector2(16f, -14f), new Vector2(10f, 6f), new Color(0f, 0f, 0f, 0.48f));
            CreateImage(frame.transform, "Speaker Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(6f, 0f), new Vector2(4f, -28f), Amber);
            GameObject portraitBay = CreateImage(frame.transform, "Portrait Bay", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(155f, 0f), new Vector2(250f, -38f), new Color(0.025f, 0.047f, 0.06f, 0.88f));
            CreateImage(portraitBay.transform, "Portrait Bracket", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(7f, 0f), new Vector2(3f, -20f), new Color(Amber.r, Amber.g, Amber.b, 0.72f));
            Image portrait = CreateImage(portraitBay.transform, "Eren Portrait Neutral", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, -20f), Color.white).GetComponent<Image>();
            AddFinalArtSlot(portrait.gameObject, $"slot.{locationId}.dialogue.eren.neutral", "NP-CHR-EREN-PORTRAIT-N-001", uiImage: portrait, showStructuralPlaceholder: false);
            string[] portraitNames = { "Concerned", "Suspicious", "Stressed", "Corrupted" };
            string[] portraitIds = { "NP-CHR-EREN-PORTRAIT-C-001", "NP-CHR-EREN-PORTRAIT-S-001", "NP-CHR-EREN-PORTRAIT-A-001", "NP-CHR-EREN-PORTRAIT-X-001" };
            for (int index = 0; index < portraitNames.Length; index++)
            {
                Image variant = CreateImage(portraitBay.transform, "Eren Portrait " + portraitNames[index], Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-20f, -20f), Color.white).GetComponent<Image>();
                AddFinalArtSlot(variant.gameObject, $"slot.{locationId}.dialogue.eren.{portraitNames[index].ToLowerInvariant()}", portraitIds[index], uiImage: variant, showStructuralPlaceholder: false);
                variant.gameObject.SetActive(false);
            }
            GameObject portraitFallback = CreateProceduralPortraitFallback(portraitBay.transform, "Portrait Unassigned", Cyan);
            Text speaker = CreateText(frame.transform, "Speaker", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(330f, -58f), new Vector2(560f, 52f), 29, TextAnchor.MiddleLeft, Amber);
            Text mode = CreateText(frame.transform, "Dialogue Mode", "NORMAL / KAYITLI İLETİŞİM", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-350f, -54f), new Vector2(390f, 28f), 14, TextAnchor.MiddleRight, new Color(Cyan.r, Cyan.g, Cyan.b, 0.55f));
            CreateImage(frame.transform, "Speaker Rule", new Vector2(0f, 1f), new Vector2(0.82f, 1f), new Vector2(320f, -96f), new Vector2(-350f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.2f));
            Text body = CreateText(frame.transform, "Body", string.Empty, new Vector2(0f, 0f), Vector2.one, new Vector2(330f, -18f), new Vector2(-420f, -150f), 28, TextAnchor.UpperLeft, TextColor);
            Text history = CreateText(portraitBay.transform, "History", string.Empty, Vector2.zero, Vector2.one, new Vector2(20f, 16f), new Vector2(-38f, -276f), 14, TextAnchor.LowerLeft, new Color(0.53f, 0.59f, 0.63f, 0.72f));
            UiSoundHooks sounds = CreateUiSoundHooks(root.transform);
            Button continueButton = CreatePhase5CButton(frame.transform, "Continue", "DEVAM", "ENTER", new Vector2(250f, 56f), new Vector2(610f, -165f), theme, sounds);
            Button closeButton = CreatePhase5CButton(frame.transform, "Close", "KAPAT", "ESC", new Vector2(210f, 52f), new Vector2(660f, 165f), theme, sounds, true);
            var choiceButtons = new Button[3];
            var choiceLabels = new Text[3];
            for (int index = 0; index < choiceButtons.Length; index++)
            {
                choiceButtons[index] = CreatePhase5CButton(frame.transform, $"Choice {index + 1}", string.Empty, $"0{index + 1}", new Vector2(650f, 50f), new Vector2(210f, -66f - index * 58f), theme, sounds);
                choiceLabels[index] = choiceButtons[index].transform.Find("Label").GetComponent<Text>();
                CanvasGroup choiceGroup = choiceButtons[index].gameObject.AddComponent<CanvasGroup>();
                UiTransition choiceTransition = choiceButtons[index].gameObject.AddComponent<UiTransition>();
                choiceTransition.Configure(choiceButtons[index].GetComponent<RectTransform>(), choiceGroup, true, true,
                    false, new Vector2(18f, 0f), Vector3.one, 0.14f + index * 0.02f);
            }
            RectTransform scan = CreateImage(frame.transform, "Dialogue Scan", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(-380f, 1f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.07f)).GetComponent<RectTransform>();
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(root, frame, new[] { portraitBay, speaker.gameObject, mode.gameObject, body.gameObject }, scan, null, new Vector2(0f, -28f), 0.22f);
            DialoguePanel panel = root.AddComponent<DialoguePanel>();
            panel.Configure(root, speaker, body, history, continueButton, closeButton, choiceButtons, choiceLabels);
            panel.ConfigurePresentation(presentation, portrait, portraitFallback, mode);
            ui.DialogueController.Configure(panel);
            root.SetActive(false);
        }

        private static void ConfigureTerminalPanel(Transform canvas, SceneUi ui)
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            GameObject root = CreateModalRoot(canvas, "Terminal Panel", Color.clear);
            CreatePhase5CBackdrop(root, Cyan);
            GameObject frame = CreateImage(root.transform, "Terminal Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1660f, 900f), new Color(0.008f, 0.031f, 0.034f, 0.99f));
            CreateImage(frame.transform, "Phosphor Field", new Vector2(0f, 0f), Vector2.one, new Vector2(190f, -34f), new Vector2(-430f, -180f), new Color(0.02f, 0.08f, 0.078f, 0.46f));
            CreateImage(frame.transform, "Terminal Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(7f, 0f), new Vector2(4f, -34f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.82f));
            Text system = CreateText(frame.transform, "System", "MNEMOSYNE RESEARCH SYSTEM / LOCAL NODE", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(74f, -40f), new Vector2(660f, 26f), 14, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.68f));
            Text title = CreateText(frame.transform, "Title", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(74f, -88f), new Vector2(920f, 58f), 36, TextAnchor.MiddleLeft, TextColor);
            Text timestamp = CreateText(frame.transform, "Timestamp", "LOCAL 03:17:42\nSESSION / FORENSIC-READ", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-430f, -60f), new Vector2(430f, 54f), 14, TextAnchor.MiddleRight, new Color(Cyan.r, Cyan.g, Cyan.b, 0.48f));
            CreateImage(frame.transform, "Header Rule", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(74f, -128f), new Vector2(-150f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.22f));
            GameObject directory = CreateImage(frame.transform, "Directory Pane", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(300f, -62f), new Vector2(520f, -190f), new Color(0.018f, 0.055f, 0.06f, 0.88f));
            CreateText(directory.transform, "Directory Heading", "/ARCHIVE/LOCAL/INDEX", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(28f, -24f), new Vector2(-54f, 28f), 14, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.58f));
            GameObject contentPane = CreateImage(frame.transform, "Record Pane", new Vector2(0f, 0f), Vector2.one, new Vector2(280f, -62f), new Vector2(-640f, -190f), new Color(0.012f, 0.045f, 0.048f, 0.72f));
            Text body = CreateText(contentPane.transform, "Body", string.Empty, Vector2.zero, Vector2.one, new Vector2(46f, -40f), new Vector2(-86f, -120f), 23, TextAnchor.UpperLeft, new Color(0.72f, 0.88f, 0.84f, 1f));
            Text cursor = CreateText(contentPane.transform, "Cursor", "█", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(54f, 42f), new Vector2(36f, 36f), 22, TextAnchor.MiddleLeft, Cyan);
            Text status = CreateText(frame.transform, "Status", string.Empty, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(584f, 52f), new Vector2(-760f, 38f), 15, TextAnchor.MiddleLeft, Amber);
            UiSoundHooks sounds = CreateUiSoundHooks(root.transform);
            Button close = CreatePhase5CButton(frame.transform, "Close", "OTURUMU KAPAT", "ESC", new Vector2(260f, 56f), new Vector2(680f, 374f), theme, sounds, true);
            var entryButtons = new Button[6];
            var entryLabels = new Text[6];
            for (int index = 0; index < entryButtons.Length; index++)
            {
                entryButtons[index] = CreatePhase5CButton(directory.transform, $"Entry {index + 1}", string.Empty, $"{index + 1:00}", new Vector2(450f, 72f), new Vector2(0f, 220f - index * 82f), theme, sounds);
                entryLabels[index] = entryButtons[index].transform.Find("Label").GetComponent<Text>();
            }
            RectTransform scan = CreateImage(contentPane.transform, "Terminal Scan", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(-36f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.14f)).GetComponent<RectTransform>();
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(root, frame, new[] { system.gameObject, title.gameObject, timestamp.gameObject, directory, contentPane, status.gameObject }, scan, null, new Vector2(18f, 0f));
            TerminalPanel panel = root.AddComponent<TerminalPanel>();
            panel.Configure(root, title, body, status, close, entryButtons, entryLabels);
            panel.ConfigurePresentation(presentation, cursor);
            ui.TerminalController.Configure(panel);
            root.SetActive(false);
        }

        private static void ConfigureMemoryPanel(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateModalRoot(canvas, "Memory Panel", new Color(0.015f, 0.012f, 0.025f, 0.92f));
            Image overlay = root.GetComponent<Image>();
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            CreateImage(root.transform, "Memory Desaturation", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.03f, 0.035f, 0.05f, 0.3f));
            CreateImage(root.transform, "Violet Field", new Vector2(0.24f, 0.2f), new Vector2(0.76f, 0.8f), Vector2.zero, Vector2.zero, new Color(0.22f, 0.12f, 0.3f, 0.08f));
            CreateImage(root.transform, "Omission Left", new Vector2(0.04f, 0.18f), new Vector2(0.31f, 0.205f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.82f));
            CreateImage(root.transform, "Omission Right", new Vector2(0.69f, 0.72f), new Vector2(0.96f, 0.752f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.72f));
            CreateImage(root.transform, "Omission Upper", new Vector2(0.12f, 0.81f), new Vector2(0.43f, 0.83f), new Vector2(22f, 0f), Vector2.zero, new Color(0f, 0f, 0f, 0.54f));
            CreateImage(root.transform, "Omission Lower", new Vector2(0.52f, 0.12f), new Vector2(0.77f, 0.143f), new Vector2(-18f, 0f), Vector2.zero, new Color(0f, 0f, 0f, 0.48f));
            CreateImage(root.transform, "Temporal Echo Cyan", new Vector2(0.09f, 0.29f), new Vector2(0.34f, 0.43f), new Vector2(-8f, 5f), Vector2.zero, new Color(Cyan.r, Cyan.g, Cyan.b, 0.055f));
            CreateImage(root.transform, "Temporal Echo Red", new Vector2(0.67f, 0.56f), new Vector2(0.91f, 0.67f), new Vector2(10f, -4f), Vector2.zero, new Color(Red.r, Red.g, Red.b, 0.045f));
            Text fragment = CreateText(root.transform, "Fragment Metadata", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(110f, -88f), new Vector2(620f, 54f), 15, TextAnchor.UpperLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.54f));
            Text title = CreateText(root.transform, "Title", string.Empty, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -132f), new Vector2(1500f, 72f), 30, TextAnchor.MiddleCenter, Red);
            CreateImage(root.transform, "Protected Text Field", new Vector2(0.18f, 0.31f), new Vector2(0.82f, 0.69f), Vector2.zero, Vector2.zero, new Color(0.012f, 0.018f, 0.03f, 0.5f));
            CreateImage(root.transform, "Memory Rule", new Vector2(0.3f, 0.5f), new Vector2(0.7f, 0.5f), new Vector2(0f, 110f), new Vector2(0f, 2f), new Color(Red.r, Red.g, Red.b, 0.3f));
            Text beat = CreateText(root.transform, "Beat", string.Empty, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1120f, 220f), 46, TextAnchor.MiddleCenter, TextColor);
            Text echo = CreateText(root.transform, "Frame Echo", "KARE 03 // KAYIP\nKAYNAK UYUŞMAZLIĞI", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-170f, 88f), new Vector2(420f, 70f), 14, TextAnchor.LowerRight, new Color(0.36f, 0.3f, 0.51f, 0.58f));
            RectTransform scan = CreateImage(root.transform, "Memory Scan", new Vector2(0.08f, 0.5f), new Vector2(0.92f, 0.5f), Vector2.zero, new Vector2(0f, 2f), new Color(Red.r, Red.g, Red.b, 0.12f)).GetComponent<RectTransform>();
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(root, root, new[] { fragment.gameObject, title.gameObject, beat.gameObject, echo.gameObject }, scan, null, Vector2.zero, 0.18f);
            MemoryPanel panel = root.AddComponent<MemoryPanel>();
            panel.Configure(root, overlay, title, beat, group);
            panel.ConfigurePresentation(presentation, fragment);
            AudioSource audio = root.AddComponent<AudioSource>();
            audio.playOnAwake = false;
            ui.MemoryController.Configure(panel, audio);

            var distortionObject = new GameObject("Memory Distortion Controller");
            distortionObject.transform.SetParent(root.transform, false);
            MemoryDistortionController distortion = distortionObject.AddComponent<MemoryDistortionController>();
            GameObject fxRoot = CreateRect(root.transform, "Memory Distortion FX", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup fxGroup = fxRoot.AddComponent<CanvasGroup>();
            Image distortionOverlay = CreateImage(fxRoot.transform, "Chromatic Wash", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.12f, 0.02f, 0.18f, 0f)).GetComponent<Image>();
            Image staticLayer = CreateImage(fxRoot.transform, "Static Blocks", new Vector2(0.12f, 0.22f), new Vector2(0.38f, 0.5f), new Vector2(-12f, 8f), Vector2.zero, new Color(Cyan.r, Cyan.g, Cyan.b, 0f)).GetComponent<Image>();
            Image vignette = CreateImage(fxRoot.transform, "Vignette Shift", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.12f, 0f, 0.18f, 0f)).GetComponent<Image>();
            Image flash = CreateImage(fxRoot.transform, "Short Flash", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(TextColor.r, TextColor.g, TextColor.b, 0f)).GetComponent<Image>();
            RectTransform tear = CreateImage(fxRoot.transform, "Horizontal Tear", new Vector2(0.12f, 0.54f), new Vector2(0.88f, 0.54f), new Vector2(24f, 0f), new Vector2(-48f, 28f), new Color(Red.r, Red.g, Red.b, 0.16f)).GetComponent<RectTransform>();
            RectTransform chromaticLeft = CreateImage(fxRoot.transform, "Chromatic Left", new Vector2(0f, 0f), new Vector2(0.012f, 1f), Vector2.zero, Vector2.zero, new Color(Cyan.r, Cyan.g, Cyan.b, 0.11f)).GetComponent<RectTransform>();
            RectTransform chromaticRight = CreateImage(fxRoot.transform, "Chromatic Right", new Vector2(0.988f, 0f), Vector2.one, Vector2.zero, Vector2.zero, new Color(Red.r, Red.g, Red.b, 0.1f)).GetComponent<RectTransform>();
            fxRoot.transform.SetSiblingIndex(4);
            distortion.Configure(fxRoot, fxGroup, distortionOverlay, staticLayer, vignette, flash, tear, chromaticLeft, chromaticRight);
            ui.MemoryController.ConfigureVisualEffects(
                distortion,
                AssetDatabase.LoadAssetAtPath<MemoryDistortionProfile>(VisualProductionBuilder.MemoryProfilePath));
            root.SetActive(false);
        }

        private static void ConfigureEvidenceBoardPanel(Transform canvas, SceneUi ui)
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            GameObject root = CreateModalRoot(canvas, "Evidence Board Panel", Color.clear);
            CreatePhase5CBackdrop(root, Amber);
            GameObject frame = CreateImage(root.transform, "Board Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1720f, 920f), new Color(0.014f, 0.03f, 0.044f, 0.99f));
            CreateImage(frame.transform, "Board Texture", Vector2.zero, Vector2.one, new Vector2(-120f, -24f), new Vector2(-520f, -160f), new Color(0.12f, 0.1f, 0.075f, 0.12f));
            CreateImage(frame.transform, "System Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(7f, 0f), new Vector2(4f, -34f), Amber);
            Text heading = CreateText(frame.transform, "Heading", "KANIT PANOSU", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(76f, -68f), new Vector2(680f, 62f), 40, TextAnchor.MiddleLeft, TextColor);
            Text boardMode = CreateText(frame.transform, "Board Mode", "DİJİTAL ADLİ ÇALIŞMA ALANI / BAĞLANTI MODU", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(78f, -112f), new Vector2(820f, 28f), 14, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.62f));
            CreateImage(frame.transform, "Header Rule", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(76f, -142f), new Vector2(-152f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.2f));
            GameObject workspace = CreateImage(frame.transform, "Deduction Workspace", new Vector2(0f, 0f), Vector2.one, new Vector2(-225f, -40f), new Vector2(-590f, -210f), new Color(0.022f, 0.045f, 0.055f, 0.6f));
            GameObject conclusionPane = CreateImage(frame.transform, "Solved Deductions", new Vector2(1f, 0f), Vector2.one, new Vector2(-256f, -40f), new Vector2(470f, -210f), new Color(0.02f, 0.04f, 0.052f, 0.82f));
            CreateImage(frame.transform, "Workspace Divider", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-492f, -38f), new Vector2(2f, -216f), new Color(Amber.r, Amber.g, Amber.b, 0.3f));
            CreateImage(conclusionPane.transform, "Solved Signal", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -4f), new Vector2(-26f, 3f), new Color(Amber.r, Amber.g, Amber.b, 0.4f));
            CreateText(conclusionPane.transform, "Solved Heading", "ÇÖZÜLEN BAĞLANTILAR", new Vector2(0f, 1f), Vector2.one, new Vector2(30f, -24f), new Vector2(-60f, 28f), 15, TextAnchor.MiddleLeft, new Color(Amber.r, Amber.g, Amber.b, 0.75f));
            Text solved = CreateText(conclusionPane.transform, "Solved", string.Empty, Vector2.zero, Vector2.one, new Vector2(32f, -62f), new Vector2(-64f, -112f), 21, TextAnchor.UpperLeft, TextColor);
            GameObject lineRootObject = CreateRect(workspace.transform, "Evidence Connections", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform lineRoot = lineRootObject.GetComponent<RectTransform>();
            var linePool = new Image[7];
            for (int index = 0; index < linePool.Length; index++)
            {
                linePool[index] = CreateImage(lineRoot, $"Connection {index + 1}", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(10f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.62f)).GetComponent<Image>();
                linePool[index].raycastTarget = false;
            }
            EvidenceConnectionView connections = lineRootObject.AddComponent<EvidenceConnectionView>();
            connections.Configure(lineRoot, linePool);
            Text status = CreateText(frame.transform, "Status", string.Empty, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(82f, 54f), new Vector2(-720f, 42f), 18, TextAnchor.MiddleLeft, Amber);
            UiSoundHooks sounds = CreateUiSoundHooks(root.transform);
            Button attempt = CreatePhase5CButton(frame.transform, "Attempt", "ÇIKARIMI DOĞRULA", "ENTER", new Vector2(300f, 58f), new Vector2(250f, -394f), theme, sounds);
            Button reset = CreatePhase5CButton(frame.transform, "Reset", "BAĞLANTILARI TEMİZLE", "R", new Vector2(300f, 58f), new Vector2(580f, -394f), theme, sounds);
            Button close = CreatePhase5CButton(frame.transform, "Close", "KAPAT", "ESC", new Vector2(220f, 54f), new Vector2(720f, 388f), theme, sounds, true);
            var evidenceButtons = new Button[8];
            var evidenceLabels = new Text[8];
            var evidenceCards = new RectTransform[8];
            var selectedPlates = new Image[8];
            for (int index = 0; index < evidenceButtons.Length; index++)
            {
                int column = index % 2;
                int row = index / 2;
                Vector2 position = new Vector2(-245f + column * 490f, 235f - row * 146f);
                evidenceButtons[index] = CreatePhase5CButton(workspace.transform, $"Evidence {index + 1}", string.Empty, $"E-{index + 1:00}", new Vector2(440f, 118f), position, theme, sounds);
                evidenceLabels[index] = evidenceButtons[index].transform.Find("Label").GetComponent<Text>();
                evidenceLabels[index].fontSize = 18;
                evidenceLabels[index].alignment = TextAnchor.MiddleLeft;
                evidenceCards[index] = evidenceButtons[index].GetComponent<RectTransform>();
                selectedPlates[index] = CreateImage(evidenceButtons[index].transform, "Selected Evidence Field", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-8f, -8f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.16f)).GetComponent<Image>();
                selectedPlates[index].raycastTarget = false;
                selectedPlates[index].transform.SetSiblingIndex(1);
                selectedPlates[index].gameObject.SetActive(false);
                CreateImage(evidenceButtons[index].transform, "Relation Pin", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-14f, 0f), new Vector2(9f, 9f), new Color(Amber.r, Amber.g, Amber.b, 0.82f));
            }
            RectTransform scan = CreateImage(workspace.transform, "Workspace Scan", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(-30f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.07f)).GetComponent<RectTransform>();
            GameObject pulseObject = CreateImage(workspace.transform, "Deduction Resolve Pulse", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-16f, -16f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.16f));
            CanvasGroup pulse = pulseObject.AddComponent<CanvasGroup>();
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(root, frame, new[] { heading.gameObject, boardMode.gameObject, workspace, conclusionPane, status.gameObject }, scan, pulse, new Vector2(0f, 26f));
            EvidenceBoardPanel panel = root.AddComponent<EvidenceBoardPanel>();
            panel.Configure(root, status, solved, attempt, reset, close, evidenceButtons, evidenceLabels);
            panel.ConfigurePresentation(presentation, connections, evidenceCards, selectedPlates, conclusionPane.GetComponent<Image>());
            ui.EvidenceBoardController.Configure(panel);
            root.SetActive(false);
        }

        private static void ConfigureInterrogationPanel(Transform canvas, SceneUi ui, string locationId)
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            GameObject root = CreateModalRoot(canvas, "Interrogation Panel", Color.clear);
            CreatePhase5CBackdrop(root, Red);
            GameObject frame = CreateImage(root.transform, "Interrogation Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600f, 860f), new Color(0.016f, 0.028f, 0.043f, 0.985f));
            CreateImage(frame.transform, "Tension Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(7f, 0f), new Vector2(4f, -34f), Red);
            Text heading = CreateText(frame.transform, "Heading", "İFADE ÇELİŞKİ ANALİZİ", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(76f, -62f), new Vector2(720f, 58f), 36, TextAnchor.MiddleLeft, TextColor);
            Text claimMetadata = CreateText(frame.transform, "Claim Metadata", string.Empty, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-390f, -62f), new Vector2(430f, 56f), 14, TextAnchor.MiddleRight, new Color(Red.r, Red.g, Red.b, 0.66f));
            CreateImage(frame.transform, "Header Rule", new Vector2(0f, 1f), Vector2.one, new Vector2(76f, -124f), new Vector2(-152f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.18f));
            GameObject portraitBay = CreateImage(frame.transform, "Witness Portrait", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(215f, -72f), new Vector2(330f, -220f), new Color(0.03f, 0.045f, 0.055f, 0.82f));
            Image portrait = CreateImage(portraitBay.transform, "Portrait Art Slot", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-22f, -22f), Color.white).GetComponent<Image>();
            AddFinalArtSlot(portrait.gameObject, $"slot.{locationId}.interrogation.portrait", "NP-CHR-EREN-PORTRAIT-S-001", uiImage: portrait, showStructuralPlaceholder: false);
            GameObject portraitFallback = CreateProceduralPortraitFallback(portraitBay.transform, "Portrait Fallback", Red);
            GameObject claimPane = CreateImage(frame.transform, "Claim Pane", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(210f, 205f), new Vector2(940f, 150f), new Color(0.025f, 0.048f, 0.062f, 0.9f));
            CreateImage(claimPane.transform, "Contradiction Accent", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(-30f, 4f), new Color(Red.r, Red.g, Red.b, 0.62f));
            CreateText(claimPane.transform, "Claim Label", "İFADE / DOĞRULAMA BEKLİYOR", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -25f), new Vector2(876f, 22f), 13, TextAnchor.MiddleLeft, new Color(Amber.r, Amber.g, Amber.b, 0.72f));
            Text statement = CreateText(claimPane.transform, "Statement", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -57f), new Vector2(876f, 50f), 24, TextAnchor.UpperLeft, TextColor);
            Text feedback = CreateText(claimPane.transform, "Feedback", string.Empty, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -118f), new Vector2(876f, 26f), 16, TextAnchor.MiddleLeft, Amber);
            GameObject evidencePreviewFrame = CreateImage(frame.transform, "Presented Evidence", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-202f, 126f), new Vector2(220f, 150f), new Color(0.04f, 0.055f, 0.06f, 0.88f));
            Image evidencePreview = CreateImage(evidencePreviewFrame.transform, "Evidence Thumbnail", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-18f, -18f), Color.white).GetComponent<Image>();
            evidencePreview.enabled = false;
            GameObject evidenceFallback = CreateProceduralEvidenceFallback(evidencePreviewFrame.transform, "Presented Evidence Fallback", Amber);
            evidencePreviewFrame.SetActive(false);
            UiSoundHooks sounds = CreateUiSoundHooks(root.transform);
            Button close = CreatePhase5CButton(frame.transform, "Close", "İFADEYİ KAPAT", "ESC", new Vector2(300f, 54f), new Vector2(620f, 354f), theme, sounds, true);
            var evidenceButtons = new Button[8];
            var evidenceLabels = new Text[8];
            for (int index = 0; index < evidenceButtons.Length; index++)
            {
                int column = index % 2;
                int row = index / 2;
                evidenceButtons[index] = CreatePhase5CButton(frame.transform, $"Interrogation Evidence {index + 1}", string.Empty, $"E-{index + 1:00}", new Vector2(430f, 58f), new Vector2(135f + column * 430f, -44f - row * 70f), theme, sounds);
                evidenceLabels[index] = evidenceButtons[index].transform.Find("Label").GetComponent<Text>();
                evidenceLabels[index].fontSize = 18;
            }
            RectTransform scan = CreateImage(claimPane.transform, "Claim Scan", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(-24f, 2f), new Color(Red.r, Red.g, Red.b, 0.08f)).GetComponent<RectTransform>();
            GameObject pulseObject = CreateImage(claimPane.transform, "Contradiction Pulse", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(3f, 0f), new Vector2(5f, -20f), new Color(Red.r, Red.g, Red.b, 0.34f));
            CanvasGroup pulse = pulseObject.AddComponent<CanvasGroup>();
            CyberNoirPanelPresentation presentation = CreatePhase5CPresentation(root, frame, new[] { heading.gameObject, portraitBay, claimPane, evidencePreviewFrame }, scan, pulse, new Vector2(0f, 22f));
            InterrogationPanel panel = root.AddComponent<InterrogationPanel>();
            panel.Configure(root, statement, feedback, close, evidenceButtons, evidenceLabels);
            panel.ConfigurePresentation(presentation, portrait, evidencePreview, claimMetadata, portraitFallback, evidencePreviewFrame, evidenceFallback);
            ui.InterrogationController.Configure(panel);
            root.SetActive(false);
        }

        private static SettingsPanel CreateSettingsPanel(Transform canvas, string name)
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);
            GameObject root = CreateModalRoot(canvas, name, new Color(0.004f, 0.009f, 0.018f, 0.82f));
            CanvasGroup rootGroup = root.AddComponent<CanvasGroup>();
            CreateImage(root.transform, "Settings Shadow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(18f, -18f), new Vector2(1284f, 944f), new Color(0f, 0f, 0f, 0.54f));
            GameObject frame = CreateImage(root.transform, "Settings Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1240f, 920f), new Color(0.018f, 0.04f, 0.058f, 0.99f));
            CreateImage(frame.transform, "Inner Content", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -38f), new Vector2(1120f, 690f), new Color(0.035f, 0.065f, 0.082f, 0.55f));
            CreateImage(frame.transform, "Inner Shadow", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(14f, -52f), new Vector2(1094f, 664f), new Color(0.002f, 0.008f, 0.014f, 0.24f));
            CreateImage(frame.transform, "Left Rail", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(7f, 0f), new Vector2(4f, -42f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.75f));
            CreateImage(frame.transform, "Top Signal", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(66f, -7f), new Vector2(126f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.7f));
            CreateImage(frame.transform, "Bottom Signal", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-78f, 7f), new Vector2(96f, 3f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.42f));
            Text settingsHeading = CreateText(frame.transform, "Heading", "AYARLAR", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(88f, -72f), new Vector2(720f, 70f), 42, TextAnchor.MiddleLeft, TextColor);
            settingsHeading.rectTransform.pivot = new Vector2(0f, 0.5f);
            CreateText(frame.transform, "System Label", "// SİSTEM YAPILANDIRMASI\nCFG: LOCAL / PROFILE 01", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-260f, -70f), new Vector2(380f, 58f), 15, TextAnchor.MiddleRight, new Color(Cyan.r, Cyan.g, Cyan.b, 0.58f));
            CreateImage(frame.transform, "Heading Rule", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -130f), new Vector2(-120f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.2f));
            GameObject contentRoot = CreateRect(frame.transform, "Settings Content", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup contentGroup = contentRoot.AddComponent<CanvasGroup>();

            CreateSettingsSection(contentRoot.transform, "Audio Section", "AUDIO / SES", 285f);
            Slider master = CreateLabeledSlider(contentRoot.transform, "Master", "ANA SES", 230f, 0f, 1f, theme, false);
            Slider music = CreateLabeledSlider(contentRoot.transform, "Music", "MÜZİK", 170f, 0f, 1f, theme, false);
            Slider sfx = CreateLabeledSlider(contentRoot.transform, "SFX", "EFEKTLER", 110f, 0f, 1f, theme, false);
            CreateSettingsSection(contentRoot.transform, "Display Section", "DISPLAY / GÖRÜNTÜ", 50f);
            Toggle fullscreen = CreateLabeledToggle(contentRoot.transform, "Fullscreen", "TAM EKRAN", -5f, theme);
            Dropdown resolution = CreateLabeledDropdown(contentRoot.transform, "Resolution", "ÇÖZÜNÜRLÜK", -62f, theme);
            CreateSettingsSection(contentRoot.transform, "Access Section", "ACCESSIBILITY / INTERFACE", -130f);
            Slider textSpeed = CreateLabeledSlider(contentRoot.transform, "Text Speed", "METİN HIZI", -184f, 0f, 0.08f, theme, true);
            Toggle reducedMotion = CreateLabeledToggle(contentRoot.transform, "Reduced Motion", "ARAYÜZ HAREKETİ AZALT", -246f, theme);
            Toggle reducedFx = CreateLabeledToggle(contentRoot.transform, "Reduced FX", "GÖRSEL EFEKTLERİ AZALT", -304f, theme);
            UiSoundHooks soundHooks = CreateUiSoundHooks(root.transform);
            Button apply = CreateCyberButton(contentRoot.transform, "Apply", "UYGULA", "[E]", new Vector2(240f, 54f), new Vector2(724f, 28f), theme, soundHooks);
            Button back = CreateCyberButton(contentRoot.transform, "Back", "GERİ", "[ESC]", new Vector2(226f, 54f), new Vector2(980f, 28f), theme, soundHooks, true);
            SettingsPanel panel = root.AddComponent<SettingsPanel>();
            panel.Configure(master, music, sfx, textSpeed, fullscreen, resolution, reducedMotion, reducedFx, apply, back);
            UiTransition transition = root.AddComponent<UiTransition>();
            transition.Configure(frame.GetComponent<RectTransform>(), rootGroup, true, true, true, new Vector2(0f, -24f), new Vector3(0.985f, 0.985f, 1f), 0.24f);
            UiTransition contentTransition = contentRoot.AddComponent<UiTransition>();
            contentTransition.Configure(contentRoot.GetComponent<RectTransform>(), contentGroup, true, true, false, new Vector2(18f, 0f), Vector3.one, 0.2f);
            panel.ConfigurePresentation(transition, contentTransition);
            return panel;
        }

        private static void CreateSettingsSection(Transform parent, string name, string label, float y)
        {
            Text heading = CreateText(parent, name, label, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-410f, y), new Vector2(390f, 28f), 15, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f));
            heading.rectTransform.pivot = new Vector2(0f, 0.5f);
            CreateImage(parent, name + " Rule", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(155f, y), new Vector2(520f, 1f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.16f));
            CreateImage(parent, name + " Tick", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-434f, y), new Vector2(8f, 8f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.62f));
        }

        private static Slider CreateLabeledSlider(
            Transform parent,
            string name,
            string labelText,
            float y,
            float minimum,
            float maximum,
            VisualTheme theme,
            bool milliseconds)
        {
            CreateText(parent, name + " Label", labelText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-392f, y), new Vector2(300f, 38f), 18, TextAnchor.MiddleLeft, TextColor);
            GameObject root = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(112f, y), new Vector2(548f, 16f), new Color(0.035f, 0.065f, 0.09f, 0.96f));
            Image surface = root.GetComponent<Image>();
            surface.raycastTarget = true;
            GameObject glow = CreateImage(root.transform, "Focus Glow", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(16f, 14f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.22f));
            glow.GetComponent<Image>().raycastTarget = false;
            CanvasGroup glowGroup = glow.AddComponent<CanvasGroup>();
            glowGroup.alpha = 0f;
            GameObject fill = CreateImage(root.transform, "Fill", new Vector2(0f, 0f), new Vector2(0.8f, 1f), Vector2.zero, Vector2.zero, new Color(Cyan.r, Cyan.g, Cyan.b, 0.64f));
            GameObject handle = CreateImage(root.transform, "Handle", new Vector2(0.8f, 0.5f), new Vector2(0.8f, 0.5f), Vector2.zero, new Vector2(17f, 17f), new Color(0.15f, 0.28f, 0.32f, 1f));
            handle.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            CreateImage(handle.transform, "Handle Core", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(7f, 7f), Cyan).GetComponent<Image>().raycastTarget = false;
            Image edge = CreateImage(root.transform, "Edge", new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 2f), Cyan).GetComponent<Image>();
            edge.type = Image.Type.Filled;
            edge.fillMethod = Image.FillMethod.Horizontal;
            edge.fillAmount = 0.16f;
            Text value = CreateText(parent, name + " Value", milliseconds ? "ORTA · 25 ms" : "100%", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(488f, y), new Vector2(156f, 38f), 16, TextAnchor.MiddleRight, Cyan);
            Slider slider = root.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = minimum;
            slider.maxValue = maximum;
            root.AddComponent<CyberNoirSliderVisual>().Configure(theme, slider, surface, fill.GetComponent<Image>(), handle.GetComponent<Image>(), edge, glowGroup, value, milliseconds);
            return slider;
        }

        private static Toggle CreateLabeledToggle(Transform parent, string name, string labelText, float y, VisualTheme theme)
        {
            CreateText(parent, name + " Label", labelText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-392f, y), new Vector2(350f, 38f), 18, TextAnchor.MiddleLeft, TextColor);
            GameObject root = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(8f, y), new Vector2(92f, 32f), new Color(0.035f, 0.065f, 0.09f, 0.74f));
            Image surface = root.GetComponent<Image>();
            surface.raycastTarget = true;
            GameObject glow = CreateImage(root.transform, "Focus Glow", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(14f, 12f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.2f));
            glow.GetComponent<Image>().raycastTarget = false;
            CanvasGroup glowGroup = glow.AddComponent<CanvasGroup>();
            glowGroup.alpha = 0f;
            CreateImage(root.transform, "Track Inner", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(78f, 22f), new Color(0.01f, 0.024f, 0.035f, 0.9f)).GetComponent<Image>().raycastTarget = false;
            Image checkmark = CreateImage(root.transform, "Indicator", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-25f, 0f), new Vector2(20f, 20f), new Color(0.24f, 0.34f, 0.38f, 1f)).GetComponent<Image>();
            Image edge = CreateImage(root.transform, "Edge", new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 2f), Cyan).GetComponent<Image>();
            edge.type = Image.Type.Filled;
            edge.fillMethod = Image.FillMethod.Horizontal;
            edge.fillAmount = 0.16f;
            Text status = CreateText(parent, name + " Status", "KAPALI", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(93f, y), new Vector2(100f, 38f), 15, TextAnchor.MiddleLeft, Cyan);
            Toggle toggle = root.AddComponent<Toggle>();
            toggle.transition = Selectable.Transition.None;
            toggle.targetGraphic = root.GetComponent<Image>();
            toggle.graphic = null;
            root.AddComponent<CyberNoirToggleVisual>().Configure(theme, toggle, surface, checkmark.rectTransform, edge, glowGroup, status);
            return toggle;
        }

        private static Dropdown CreateLabeledDropdown(Transform parent, string name, string labelText, float y, VisualTheme theme)
        {
            CreateText(parent, name + " Label", labelText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-392f, y), new Vector2(300f, 38f), 18, TextAnchor.MiddleLeft, TextColor);
            GameObject root = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(112f, y), new Vector2(548f, 42f), new Color(0.035f, 0.065f, 0.09f, 0.86f));
            Image surface = root.GetComponent<Image>();
            surface.raycastTarget = true;
            GameObject glow = CreateImage(root.transform, "Focus Glow", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(14f, 12f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.2f));
            glow.GetComponent<Image>().raycastTarget = false;
            CanvasGroup glowGroup = glow.AddComponent<CanvasGroup>();
            glowGroup.alpha = 0f;
            CreateText(root.transform, "Previous", "‹", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(22f, 0f), new Vector2(32f, 0f), 22, TextAnchor.MiddleCenter, new Color(Cyan.r, Cyan.g, Cyan.b, 0.6f));
            Text caption = CreateText(root.transform, "Label", "1920 × 1080", Vector2.zero, Vector2.one, new Vector2(28f, 0f), new Vector2(-96f, -6f), 20, TextAnchor.MiddleCenter, TextColor);
            CreateText(root.transform, "Disclosure", "›  ◇", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-42f, 0f), new Vector2(68f, 0f), 20, TextAnchor.MiddleCenter, Cyan);
            Image edge = CreateImage(root.transform, "Edge", new Vector2(0f, 0f), new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 2f), Cyan).GetComponent<Image>();
            edge.type = Image.Type.Filled;
            edge.fillMethod = Image.FillMethod.Horizontal;
            edge.fillAmount = 0.16f;
            GameObject template = CreateImage(root.transform, "Template", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, -122f), new Vector2(0f, 190f), new Color(0.03f, 0.07f, 0.09f, 1f));
            var scrollRect = template.AddComponent<ScrollRect>();
            GameObject viewport = CreateImage(template.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.white);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            GameObject content = CreateRect(viewport.transform, "Content", new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, 40f));
            GameObject item = CreateImage(content.transform, "Item", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(0f, 40f), new Color(0.06f, 0.13f, 0.17f, 1f));
            Toggle toggle = item.AddComponent<Toggle>();
            toggle.targetGraphic = item.GetComponent<Image>();
            Text itemLabel = CreateText(item.transform, "Item Label", "Resolution", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-24f, -4f), 20, TextAnchor.MiddleLeft, TextColor);
            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = content.GetComponent<RectTransform>();
            scrollRect.horizontal = false;
            Dropdown dropdown = root.AddComponent<Dropdown>();
            dropdown.transition = Selectable.Transition.None;
            dropdown.targetGraphic = root.GetComponent<Image>();
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = caption;
            dropdown.itemText = itemLabel;
            template.SetActive(false);
            root.AddComponent<CyberNoirDropdownVisual>().Configure(theme, surface, edge, glowGroup);
            return dropdown;
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
            cameraObject.AddComponent<AudioListener>();
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
            Image image = item.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
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
            if (Vector2.Distance(anchorMin, anchorMax) < 0.001f)
            {
                RectTransform rect = item.GetComponent<RectTransform>();
                float pivotX = alignment is TextAnchor.UpperLeft or TextAnchor.MiddleLeft or TextAnchor.LowerLeft
                    ? 0f
                    : alignment is TextAnchor.UpperRight or TextAnchor.MiddleRight or TextAnchor.LowerRight
                        ? 1f
                        : 0.5f;
                float pivotY = alignment is TextAnchor.UpperLeft or TextAnchor.UpperCenter or TextAnchor.UpperRight
                    ? 1f
                    : alignment is TextAnchor.LowerLeft or TextAnchor.LowerCenter or TextAnchor.LowerRight
                        ? 0f
                        : 0.5f;
                rect.pivot = new Vector2(pivotX, pivotY);
            }
            return label;
        }

        private static UiSoundHooks CreateUiSoundHooks(Transform parent)
        {
            var root = new GameObject("UI Sound Hooks");
            root.transform.SetParent(parent, false);
            AudioSource source = root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.ignoreListenerPause = true;
            UiSoundHooks hooks = root.AddComponent<UiSoundHooks>();
            // Deliberately nullable until approved UI clips are supplied.
            hooks.Configure(source, null, null, null, null);
            return hooks;
        }

        private static void CreateProceduralCityLayer(
            Transform parent,
            string name,
            int buildingCount,
            float startX,
            float minimumHeight,
            float heightRange,
            Color buildingColor,
            List<CanvasGroup> lightGroups,
            bool far)
        {
            GameObject city = CreateRect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            float x = startX;
            for (int index = 0; index < buildingCount; index++)
            {
                float width = (far ? 62f : 96f) + Mathf.Repeat(index * 47f, far ? 76f : 118f);
                float height = minimumHeight + Mathf.Repeat(index * 131f, heightRange);
                GameObject building = CreateImage(city.transform, $"Tower {index + 1:00}", Vector2.zero, Vector2.zero, new Vector2(x, 56f), new Vector2(width, height), Color.Lerp(buildingColor, Color.black, (index % 4) * 0.055f));
                RectTransform buildingRect = building.GetComponent<RectTransform>();
                buildingRect.pivot = Vector2.zero;
                CreateImage(building.transform, "Roof Line", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -2f), new Vector2(0f, 2f), new Color(0.18f, 0.38f, 0.42f, far ? 0.12f : 0.19f));
                if (index % 3 == 1)
                {
                    CreateImage(building.transform, "Antenna", new Vector2(0.62f, 1f), new Vector2(0.62f, 1f), new Vector2(0f, 22f), new Vector2(2f, 44f), new Color(0.13f, 0.26f, 0.29f, 0.42f));
                }

                int rows = far ? 3 : 5;
                for (int row = 0; row < rows; row++)
                {
                    for (int column = 0; column < 2; column++)
                    {
                        if ((index * 5 + row * 3 + column) % 4 == 0)
                        {
                            continue;
                        }

                        float windowX = 0.3f + column * 0.38f;
                        float windowY = 0.18f + row * (far ? 0.18f : 0.135f);
                        Color lightColor = (index + row) % 5 == 0
                            ? new Color(0.78f, 0.36f, 0.2f, far ? 0.16f : 0.28f)
                            : new Color(0.25f, 0.72f, 0.74f, far ? 0.1f : 0.2f);
                        GameObject window = CreateImage(building.transform, $"Window {row:00}-{column:00}", new Vector2(windowX, windowY), new Vector2(windowX, windowY), Vector2.zero, new Vector2(far ? 5f : 8f, far ? 2f : 3f), lightColor);
                        if (column == 0 && row == 1 && index % 3 == 0)
                        {
                            CanvasGroup light = window.AddComponent<CanvasGroup>();
                            light.alpha = 0.3f;
                            lightGroups.Add(light);
                        }
                    }
                }

                x += width * 0.74f;
            }
        }

        private static RectTransform[] CreateProceduralRain(
            Transform parent,
            out int farCount,
            out int midCount)
        {
            farCount = 30;
            midCount = 18;
            const int nearCount = 6;
            var streaks = new RectTransform[farCount + midCount + nearCount];
            int cursor = 0;
            CreateRainBand(parent, "Far", farCount, 0, ref cursor, streaks, 0.65f, 14f, 29f, 0.018f, 0.044f, -4f);
            CreateRainBand(parent, "Mid", midCount, 101, ref cursor, streaks, 1.15f, 42f, 88f, 0.045f, 0.09f, -7f);
            CreateRainBand(parent, "Near", nearCount, 211, ref cursor, streaks, 2.4f, 118f, 218f, 0.065f, 0.13f, -10f);
            return streaks;
        }

        private static void CreateRainBand(
            Transform parent,
            string band,
            int count,
            int seed,
            ref int cursor,
            RectTransform[] output,
            float width,
            float minimumLength,
            float maximumLength,
            float minimumAlpha,
            float maximumAlpha,
            float baseAngle)
        {
            for (int index = 0; index < count; index++)
            {
                int sequence = seed + index;
                float cluster = sequence % 4 == 0 ? 0.18f : sequence % 4 == 1 ? 0.36f : sequence % 4 == 2 ? 0.7f : 0.86f;
                float jitter = Mathf.Repeat(sequence * 0.173f, 0.14f) - 0.07f;
                float x = Mathf.Lerp(-900f, 900f, Mathf.Clamp01(cluster + jitter));
                float y = -540f + Mathf.Repeat(sequence * 257f, 1180f);
                float length = Mathf.Lerp(minimumLength, maximumLength, Mathf.Repeat(sequence * 0.377f, 1f));
                float alpha = Mathf.Lerp(minimumAlpha, maximumAlpha, Mathf.Repeat(sequence * 0.619f, 1f));
                GameObject streak = CreateImage(
                    parent,
                    $"Rain {band} {index + 1:00}",
                    new Vector2(0.5f, 0.5f),
                    new Vector2(0.5f, 0.5f),
                    new Vector2(x, y),
                    new Vector2(width + Mathf.Repeat(sequence * 0.43f, width * 0.65f), length),
                    new Color(0.38f, 0.68f, 0.72f, alpha));
                Image image = streak.GetComponent<Image>();
                image.raycastTarget = false;
                streak.transform.localRotation = Quaternion.Euler(0f, 0f, baseAngle + Mathf.Repeat(sequence * 1.91f, 5f) - 2.5f);
                output[cursor++] = streak.GetComponent<RectTransform>();
            }

        }

        private static Button CreateCyberButton(
            Transform parent,
            string name,
            string labelText,
            string indexText,
            Vector2 size,
            Vector2 position,
            VisualTheme theme,
            UiSoundHooks soundHooks,
            bool backAction = false)
        {
            GameObject item = CreateImage(
                parent,
                name,
                new Vector2(0f, 0f),
                new Vector2(0f, 0f),
                position,
                size,
                new Color(0.025f, 0.055f, 0.075f, 0.34f));
            item.GetComponent<RectTransform>().pivot = Vector2.zero;
            Image surface = item.GetComponent<Image>();
            surface.raycastTarget = true;
            Button button = item.AddComponent<Button>();
            button.transition = Selectable.Transition.None;

            GameObject glow = CreateImage(item.transform, "Focus Glow", Vector2.zero, new Vector2(0.68f, 1f), Vector2.zero, new Vector2(10f, 8f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.12f));
            Image glowImage = glow.GetComponent<Image>();
            glowImage.raycastTarget = false;
            CanvasGroup focusGlow = glow.AddComponent<CanvasGroup>();
            focusGlow.alpha = 0f;
            Image edge = CreateImage(item.transform, "Edge Line", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(34f, 4f), new Vector2(42f, 2f), Cyan).GetComponent<Image>();
            edge.rectTransform.pivot = new Vector2(0f, 0.5f);
            edge.raycastTarget = false;
            Text index = CreateText(item.transform, "Index", indexText, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(34f, 0f), new Vector2(42f, 0f), 13, TextAnchor.MiddleLeft, new Color(Cyan.r, Cyan.g, Cyan.b, 0.65f));
            index.horizontalOverflow = HorizontalWrapMode.Overflow;
            Text label = CreateText(item.transform, "Label", labelText, Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-104f, -6f), 22, TextAnchor.MiddleLeft, TextColor);
            GameObject bracket = CreateRect(item.transform, "Focus Bracket", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup bracketGroup = bracket.AddComponent<CanvasGroup>();
            bracketGroup.alpha = 0f;
            CreateImage(bracket.transform, "Bracket Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(5f, 0f), new Vector2(2f, -12f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.86f));
            CreateImage(bracket.transform, "Bracket Top", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(17f, -5f), new Vector2(26f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.86f));
            CreateImage(bracket.transform, "Bracket Bottom", new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(17f, 5f), new Vector2(26f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.86f));
            RectTransform accent = CreateImage(item.transform, "Moving Accent", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(104f, -4f), new Vector2(56f, 2f), new Color(Cyan.r, Cyan.g, Cyan.b, 0.76f)).GetComponent<RectTransform>();
            CyberNoirButtonVisual visual = item.AddComponent<CyberNoirButtonVisual>();
            visual.Configure(theme, button, surface, label, edge, focusGlow, item.GetComponent<RectTransform>(), bracketGroup, accent, index);
            visual.ConfigureSound(soundHooks, backAction);
            return button;
        }

        private static Button CreatePhase5CButton(
            Transform parent,
            string name,
            string label,
            string index,
            Vector2 size,
            Vector2 position,
            VisualTheme theme,
            UiSoundHooks sounds,
            bool backAction = false)
        {
            Button button = CreateCyberButton(
                parent,
                name,
                label,
                index,
                size,
                Vector2.zero,
                theme,
                sounds,
                backAction);
            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            return button;
        }

        private static GameObject CreateProceduralPortraitFallback(Transform parent, string name, Color accent)
        {
            GameObject fallback = CreateRect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateImage(fallback.transform, "Portrait Plate", new Vector2(0.12f, 0.1f), new Vector2(0.88f, 0.9f), Vector2.zero, Vector2.zero, new Color(0.01f, 0.025f, 0.038f, 0.92f));
            CreateImage(fallback.transform, "Silhouette Head", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 42f), new Vector2(70f, 76f), new Color(accent.r, accent.g, accent.b, 0.13f));
            CreateImage(fallback.transform, "Silhouette Shoulders", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -54f), new Vector2(158f, 94f), new Color(accent.r, accent.g, accent.b, 0.1f));
            CreateImage(fallback.transform, "Signal Left", new Vector2(0f, 0.66f), new Vector2(0.38f, 0.66f), new Vector2(18f, 0f), new Vector2(-30f, 2f), new Color(accent.r, accent.g, accent.b, 0.28f));
            CreateImage(fallback.transform, "Signal Right", new Vector2(0.62f, 0.32f), new Vector2(1f, 0.32f), new Vector2(-18f, 0f), new Vector2(-30f, 2f), new Color(accent.r, accent.g, accent.b, 0.2f));
            CreateImage(fallback.transform, "Signal Pulse", new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(8f, 8f), new Color(accent.r, accent.g, accent.b, 0.5f));
            return fallback;
        }

        private static GameObject CreateProceduralEvidenceFallback(Transform parent, string name, Color accent)
        {
            GameObject fallback = CreateRect(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CreateImage(fallback.transform, "Classification Plate", new Vector2(0.14f, 0.14f), new Vector2(0.86f, 0.86f), Vector2.zero, Vector2.zero, new Color(0.01f, 0.028f, 0.038f, 0.86f));
            RectTransform glyph = CreateImage(fallback.transform, "Evidence Glyph", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(48f, 48f), new Color(accent.r, accent.g, accent.b, 0.16f)).GetComponent<RectTransform>();
            glyph.localRotation = Quaternion.Euler(0f, 0f, 45f);
            CreateImage(fallback.transform, "Scan A", new Vector2(0.2f, 0.66f), new Vector2(0.8f, 0.66f), Vector2.zero, new Vector2(0f, 2f), new Color(accent.r, accent.g, accent.b, 0.3f));
            CreateImage(fallback.transform, "Scan B", new Vector2(0.3f, 0.34f), new Vector2(0.7f, 0.34f), Vector2.zero, new Vector2(0f, 2f), new Color(accent.r, accent.g, accent.b, 0.2f));
            CreateImage(fallback.transform, "Index Dot", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8f, 8f), new Color(accent.r, accent.g, accent.b, 0.72f));
            return fallback;
        }

        private static void CreatePhase5CBackdrop(GameObject root, Color accent)
        {
            CreateImage(root.transform, "World De-emphasis", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.006f, 0.012f, 0.021f, 0.68f));
            CreateImage(root.transform, "Left Vignette", new Vector2(0f, 0f), new Vector2(0.08f, 1f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.32f));
            CreateImage(root.transform, "Right Vignette", new Vector2(0.92f, 0f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.32f));
            CreateImage(root.transform, "Context Signal", new Vector2(0f, 0.76f), new Vector2(1f, 0.765f), Vector2.zero, Vector2.zero, new Color(accent.r, accent.g, accent.b, 0.055f));
        }

        private static CyberNoirPanelPresentation CreatePhase5CPresentation(
            GameObject root,
            GameObject frame,
            IReadOnlyList<GameObject> staged,
            RectTransform scanLine,
            CanvasGroup feedbackPulse,
            Vector2 entranceOffset,
            float duration = 0.24f)
        {
            CanvasGroup rootGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
            UiTransition transition = root.AddComponent<UiTransition>();
            transition.Configure(
                frame.GetComponent<RectTransform>(),
                rootGroup,
                true,
                true,
                true,
                entranceOffset,
                new Vector3(0.99f, 0.99f, 1f),
                duration);
            CanvasGroup[] groups = staged
                .Where(item => item != null)
                .Select(item => item.GetComponent<CanvasGroup>() ?? item.AddComponent<CanvasGroup>())
                .ToArray();
            CyberNoirPanelPresentation presentation = root.AddComponent<CyberNoirPanelPresentation>();
            presentation.Configure(transition, groups, scanLine, feedbackPulse);
            return presentation;
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
            item.GetComponent<Image>().raycastTarget = true;
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
            IReadOnlyDictionary<string, Sprite> preservedArt = ArtSlotPreservation.CaptureScene(path);
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
                ArtSlotPreservation.Restore(scene, preservedArt);
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
                new EditorBuildSettingsScene(MainMenuScenePath, true),
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

        private static void AddFinalArtSlot(
            GameObject target,
            string stableId,
            string manifestAssetId,
            SpriteRenderer spriteRenderer = null,
            Image uiImage = null,
            bool showStructuralPlaceholder = true)
        {
            target.AddComponent<FinalArtSlot>().ConfigureStructural(
                stableId,
                manifestAssetId,
                spriteRenderer,
                uiImage,
                showStructuralPlaceholder);
        }

        private static GameObject CreateEmptyUiArtSlot(
            Transform parent,
            string name,
            string stableId,
            string manifestAssetId)
        {
            GameObject root = CreateImage(parent, name, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Color.clear);
            AddFinalArtSlot(root, stableId, manifestAssetId, uiImage: root.GetComponent<Image>());
            return root;
        }

        private static string ResolveEnvironmentManifestId(LocationData location, string suffix)
        {
            string locationToken = location != null && location.StableId.Contains("mert", StringComparison.Ordinal)
                ? "MERT"
                : "EREN";
            return $"NP-ENV-{locationToken}-{suffix}-001";
        }

        private static (string SlotId, string AssetId) ResolveMertPropSlot(string name)
        {
            return name switch
            {
                "Damaged Memory Implant" => ("slot.mert_apartment.damaged_implant", "NP-PROP-MERT-IMPLANT-001"),
                "Internal Door Latch" => ("slot.mert_apartment.door_latch", "NP-PROP-MERT-DOOR-001"),
                "Unfinished Coffee" => ("slot.mert_apartment.coffee", "NP-PROP-MERT-COFFEE-001"),
                "Personal Notes" => ("slot.mert_apartment.notes", "NP-PROP-MERT-NOTES-001"),
                "Medical Calibrator" => ("slot.mert_apartment.medical_calibrator", "NP-PROP-MERT-MEDICAL-001"),
                "Eren Device History" => ("slot.mert_apartment.eren_device", "NP-PROP-MERT-EREN-DEVICE-001"),
                _ => throw new InvalidOperationException($"No Phase 5A art slot mapping exists for '{name}'.")
            };
        }

        private sealed class OpeningAssets
        {
            public CaseData Case;
            public CharacterData Eren;
            public CharacterData Mert;
            public CharacterData Dispatch;
            public LocationData ErenLocation;
            public LocationData MertLocation;
            public LocationData MainMenuLocation;
            public InspectData MedicationInspection;
            public InspectData ForeshadowInspection;
            public InspectData MertPhotoInspection;
            public InspectData DeathTimeInspection;
            public InspectData CoffeeInspection;
            public InspectData ImplantInspection;
            public InspectData DoorInspection;
            public InspectData NotesInspection;
            public InspectData MedicalInspection;
            public InspectData ErenDeviceInspection;
            public EvidenceData MertPhotoEvidence;
            public EvidenceData MertTerminalEvidence;
            public EvidenceData MertDeathTimeEvidence;
            public EvidenceData DamagedImplantEvidence;
            public EvidenceData DoorStatusEvidence;
            public EvidenceData CallRecordEvidence;
            public EvidenceData MemoryDeletionEvidence;
            public EvidenceData ErenCallHistoryEvidence;
            public DeductionData PostmortemDeduction;
            public DeductionData SuicideInconsistentDeduction;
            public DeductionData LockedRoomDeduction;
            public TerminalData DispatchTerminal;
            public TerminalData MertTerminal;
            public MemoryData PhotoMemory;
            public DialogueData MertRecorderDialogue;
            public DialogueData ChapterEndDialogue;
            public ObjectiveData[] Objectives;
            public CheckpointData ErenStartCheckpoint;
            public CheckpointData DispatchCheckpoint;
            public CheckpointData MertEntranceCheckpoint;
            public CheckpointData CriticalEvidenceCheckpoint;
            public CheckpointData FirstDeductionCheckpoint;
            public JournalEntryData[] JournalEntries;
            public InterrogationClaimData TimelineClaim;
            public AudioCueSet AudioCues;
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
            public InterrogationController InterrogationController;
            public InteractionPromptController InteractionPrompt;
            public EvidenceNotificationController EvidenceNotification;
            public PauseMenuController PauseMenuController;
            public ObjectivePresenter ObjectivePresenter;
        }
    }
}
