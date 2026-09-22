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

                GameObject backdrop = CreateImage(
                    canvasObject.transform,
                    "Noir Backdrop",
                    Vector2.zero,
                    Vector2.one,
                    Vector2.zero,
                    Vector2.zero,
                    BackgroundColor);
                CreateImage(backdrop.transform, "Cyan Horizon", new Vector2(0f, 0.2f), new Vector2(1f, 0.21f), Vector2.zero, Vector2.zero, new Color(0.1f, 0.55f, 0.62f, 0.45f));
                CreateText(backdrop.transform, "Title", "NULL POINTER", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(1100f, 110f), 72, TextAnchor.MiddleCenter, TextColor);
                CreateText(backdrop.transform, "Subtitle", "ANILAR SİLİNMEDEN ÖNCE", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -275f), new Vector2(900f, 60f), 28, TextAnchor.MiddleCenter, Cyan);

                GameObject menuRoot = CreateRect(backdrop.transform, "Main Menu", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), new Vector2(520f, 460f));
                Button newGame = CreateButton(menuRoot.transform, "New Game", "YENİ OYUN", new Vector2(420f, 70f), new Vector2(0f, 150f), Cyan, out _);
                Button continueGame = CreateButton(menuRoot.transform, "Continue", "DEVAM ET", new Vector2(420f, 70f), new Vector2(0f, 55f), new Color(0.1f, 0.32f, 0.36f, 1f), out _);
                Button settings = CreateButton(menuRoot.transform, "Settings", "AYARLAR", new Vector2(420f, 70f), new Vector2(0f, -40f), new Color(0.16f, 0.22f, 0.28f, 1f), out _);
                Button quit = CreateButton(menuRoot.transform, "Quit", "ÇIKIŞ", new Vector2(420f, 70f), new Vector2(0f, -135f), Red, out _);
                MainMenuPanel panel = menuRoot.AddComponent<MainMenuPanel>();
                panel.Configure(newGame, continueGame, settings, quit);
                SettingsPanel settingsPanel = CreateSettingsPanel(backdrop.transform, "Main Menu Settings");
                MainMenuController controller = backdrop.AddComponent<MainMenuController>();
                controller.Configure(panel, settingsPanel);
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

                CreateWorldBlock("Rain Window", new Vector3(-4.9f, 1.4f, 0f), new Vector2(2.2f, 2.8f), new Color(0.04f, 0.2f, 0.27f, 1f), GetPlaceholderSprite(), false, -2);
                CreateWorldBlock("Workstation Pool", new Vector3(-1.8f, -0.7f, 0f), new Vector2(2.6f, 1.8f), new Color(0.08f, 0.18f, 0.2f, 1f), GetPlaceholderSprite(), false, -2);
                CreateWorldBlock("Locked Interior", new Vector3(4.6f, 0.1f, 0f), new Vector2(3.1f, 4.4f), new Color(0.08f, 0.06f, 0.12f, 1f), GetPlaceholderSprite(), false, -3);

                GameObject photograph = CreateInteractableBlock(
                    "Mert Photograph",
                    new Vector3(-4.95f, -1.2f, 0f),
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
                    new Vector3(-2.35f, -1.1f, 0f),
                    new Vector2(1.1f, 1.25f),
                    Amber);
                terminal.AddComponent<TerminalInteractable>().Configure(
                    setup.Ui.TerminalController,
                    assets.MertTerminal);

                GameObject deathTime = CreateInteractableBlock(
                    "Biometric Death Time",
                    new Vector3(0.45f, -1.35f, 0f),
                    new Vector2(0.75f, 0.6f),
                    Red);
                deathTime.AddComponent<EvidenceInteractable>().Configure(
                    setup.Ui.InspectController,
                    assets.DeathTimeInspection,
                    assets.MertDeathTimeEvidence);

                GameObject board = CreateInteractableBlock(
                    "Evidence Board",
                    new Vector3(5.15f, -0.95f, 0f),
                    new Vector2(1.4f, 1.55f),
                    new Color(0.18f, 0.52f, 0.58f, 1f));
                board.AddComponent<EvidenceBoardInteractable>().Configure(setup.Ui.EvidenceBoardController);

                GameObject recorder = CreateInteractableBlock(
                    "Damaged Recorder",
                    new Vector3(6.15f, -1.4f, 0f),
                    new Vector2(0.55f, 0.55f),
                    new Color(0.45f, 0.25f, 0.32f, 1f));
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
            item.AddComponent<InspectInteractable>().Configure(setup.Ui.InspectController, inspection);
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
                EvidenceNotification = systems.AddComponent<EvidenceNotificationController>(),
                PauseMenuController = systems.AddComponent<PauseMenuController>(),
                ObjectivePresenter = systems.AddComponent<ObjectivePresenter>()
            };

            ConfigureHud(canvasObject.transform, ui);
            ConfigureObjectiveHud(canvasObject.transform, ui);
            ConfigurePauseInterface(canvasObject.transform, ui);
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

        private static void ConfigureObjectiveHud(Transform canvas, SceneUi ui)
        {
            GameObject root = CreateImage(
                canvas,
                "Objective HUD",
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(330f, -72f),
                new Vector2(610f, 72f),
                new Color(0.02f, 0.05f, 0.07f, 0.9f));
            Text label = CreateText(root.transform, "Objective", string.Empty, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-30f, -16f), 24, TextAnchor.MiddleLeft, TextColor);
            ui.ObjectivePresenter.Configure(root, label);
        }

        private static void ConfigurePauseInterface(Transform canvas, SceneUi ui)
        {
            GameObject pauseRoot = CreateModalRoot(canvas, "Pause Menu", new Color(0.005f, 0.012f, 0.025f, 0.94f));
            GameObject frame = CreateImage(pauseRoot.transform, "Pause Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620f, 650f), PanelColor);
            CreateText(frame.transform, "Heading", "DURAKLATILDI", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -85f), new Vector2(500f, 70f), 42, TextAnchor.MiddleCenter, Cyan);
            Button resume = CreateButton(frame.transform, "Resume", "DEVAM", new Vector2(430f, 68f), new Vector2(0f, 120f), Cyan, out _);
            Button journal = CreateButton(frame.transform, "Journal", "SORUŞTURMA DEFTERİ", new Vector2(430f, 68f), new Vector2(0f, 30f), new Color(0.08f, 0.24f, 0.28f, 1f), out _);
            Button settingsButton = CreateButton(frame.transform, "Settings", "AYARLAR", new Vector2(430f, 68f), new Vector2(0f, -60f), new Color(0.15f, 0.2f, 0.28f, 1f), out _);
            Button mainMenu = CreateButton(frame.transform, "Main Menu", "ANA MENÜ", new Vector2(430f, 68f), new Vector2(0f, -150f), Red, out _);
            PauseMenuPanel pausePanel = pauseRoot.AddComponent<PauseMenuPanel>();
            pausePanel.Configure(resume, journal, settingsButton, mainMenu);

            GameObject journalRoot = CreateModalRoot(canvas, "Investigation Journal", new Color(0.005f, 0.012f, 0.025f, 0.98f));
            GameObject journalFrame = CreateImage(journalRoot.transform, "Journal Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1660f, 900f), PanelColor);
            Text heading = CreateText(journalFrame.transform, "Heading", "DOSYALAR", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, -65f), new Vector2(480f, 60f), 38, TextAnchor.MiddleLeft, Cyan);
            Text content = CreateText(journalFrame.transform, "Content", string.Empty, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(550f, -10f), new Vector2(960f, 660f), 25, TextAnchor.UpperLeft, TextColor);
            Button cases = CreateButton(journalFrame.transform, "Cases", "DOSYALAR", new Vector2(310f, 58f), new Vector2(-625f, 260f), Cyan, out _);
            Button people = CreateButton(journalFrame.transform, "People", "KİŞİLER", new Vector2(310f, 58f), new Vector2(-625f, 188f), new Color(0.08f, 0.24f, 0.28f, 1f), out _);
            Button evidence = CreateButton(journalFrame.transform, "Evidence", "KANITLAR", new Vector2(310f, 58f), new Vector2(-625f, 116f), new Color(0.08f, 0.24f, 0.28f, 1f), out _);
            Button questions = CreateButton(journalFrame.transform, "Questions", "SORULAR", new Vector2(310f, 58f), new Vector2(-625f, 44f), new Color(0.08f, 0.24f, 0.28f, 1f), out _);
            Button timeline = CreateButton(journalFrame.transform, "Timeline", "ZAMAN ÇİZELGESİ", new Vector2(310f, 58f), new Vector2(-625f, -28f), Amber, out _);
            Button journalBack = CreateButton(journalFrame.transform, "Back", "GERİ", new Vector2(180f, 58f), new Vector2(690f, -370f), Red, out _);
            JournalPanel journalPanel = journalRoot.AddComponent<JournalPanel>();
            journalPanel.Configure(heading, content, cases, people, evidence, questions, timeline, journalBack);

            SettingsPanel settingsPanel = CreateSettingsPanel(canvas, "Pause Settings");
            ui.PauseMenuController.Configure(pausePanel, journalPanel, settingsPanel);
            pauseRoot.SetActive(false);
            journalRoot.SetActive(false);
            settingsPanel.Hide();
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
            Text history = CreateText(frame.transform, "History", string.Empty, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(170f, 35f), new Vector2(300f, 190f), 17, TextAnchor.LowerLeft, new Color(0.55f, 0.65f, 0.68f, 1f));
            Button continueButton = CreateButton(frame.transform, "Continue", "DEVAM", new Vector2(175f, 58f), new Vector2(500f, -130f), Cyan, out _);
            Button closeButton = CreateButton(frame.transform, "Close", "KAPAT", new Vector2(150f, 54f), new Vector2(500f, 145f), new Color(0.2f, 0.28f, 0.32f, 1f), out _);
            var choiceButtons = new Button[3];
            var choiceLabels = new Text[3];
            for (int index = 0; index < choiceButtons.Length; index++)
            {
                choiceButtons[index] = CreateButton(frame.transform, $"Choice {index + 1}", string.Empty, new Vector2(680f, 52f), new Vector2(140f, -65f - index * 60f), new Color(0.08f, 0.2f, 0.24f, 1f), out choiceLabels[index]);
            }

            DialoguePanel panel = root.AddComponent<DialoguePanel>();
            panel.Configure(root, speaker, body, history, continueButton, closeButton, choiceButtons, choiceLabels);
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

        private static SettingsPanel CreateSettingsPanel(Transform canvas, string name)
        {
            GameObject root = CreateModalRoot(canvas, name, new Color(0.005f, 0.012f, 0.025f, 0.98f));
            GameObject frame = CreateImage(root.transform, "Settings Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180f, 860f), PanelColor);
            CreateText(frame.transform, "Heading", "AYARLAR", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(1000f, 70f), 42, TextAnchor.MiddleLeft, Cyan);

            Slider master = CreateLabeledSlider(frame.transform, "Master", "ANA SES", 245f, 0f, 1f);
            Slider music = CreateLabeledSlider(frame.transform, "Music", "MÜZİK", 150f, 0f, 1f);
            Slider sfx = CreateLabeledSlider(frame.transform, "SFX", "EFEKTLER", 55f, 0f, 1f);
            Slider textSpeed = CreateLabeledSlider(frame.transform, "Text Speed", "METİN HIZI", -40f, 0f, 0.08f);
            Toggle fullscreen = CreateLabeledToggle(frame.transform, "Fullscreen", "TAM EKRAN", -150f);
            Dropdown resolution = CreateLabeledDropdown(frame.transform, "Resolution", "ÇÖZÜNÜRLÜK", -245f);
            Button apply = CreateButton(frame.transform, "Apply", "UYGULA", new Vector2(210f, 62f), new Vector2(300f, -350f), Cyan, out _);
            Button back = CreateButton(frame.transform, "Back", "GERİ", new Vector2(180f, 62f), new Vector2(520f, -350f), Red, out _);
            SettingsPanel panel = root.AddComponent<SettingsPanel>();
            panel.Configure(master, music, sfx, textSpeed, fullscreen, resolution, apply, back);
            return panel;
        }

        private static Slider CreateLabeledSlider(
            Transform parent,
            string name,
            string labelText,
            float y,
            float minimum,
            float maximum)
        {
            CreateText(parent, name + " Label", labelText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350f, y), new Vector2(260f, 45f), 23, TextAnchor.MiddleLeft, TextColor);
            GameObject root = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150f, y), new Vector2(640f, 36f), new Color(0.06f, 0.1f, 0.14f, 1f));
            GameObject fill = CreateImage(root.transform, "Fill", new Vector2(0f, 0f), new Vector2(0.8f, 1f), Vector2.zero, Vector2.zero, Cyan);
            GameObject handle = CreateImage(root.transform, "Handle", new Vector2(0.8f, 0.5f), new Vector2(0.8f, 0.5f), Vector2.zero, new Vector2(28f, 48f), TextColor);
            Slider slider = root.AddComponent<Slider>();
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.minValue = minimum;
            slider.maxValue = maximum;
            return slider;
        }

        private static Toggle CreateLabeledToggle(Transform parent, string name, string labelText, float y)
        {
            CreateText(parent, name + " Label", labelText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350f, y), new Vector2(260f, 45f), 23, TextAnchor.MiddleLeft, TextColor);
            GameObject root = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-140f, y), new Vector2(46f, 46f), new Color(0.08f, 0.14f, 0.18f, 1f));
            Image checkmark = CreateImage(root.transform, "Checkmark", new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f), Vector2.zero, Vector2.zero, Cyan).GetComponent<Image>();
            Toggle toggle = root.AddComponent<Toggle>();
            toggle.targetGraphic = root.GetComponent<Image>();
            toggle.graphic = checkmark;
            return toggle;
        }

        private static Dropdown CreateLabeledDropdown(Transform parent, string name, string labelText, float y)
        {
            CreateText(parent, name + " Label", labelText, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-350f, y), new Vector2(260f, 45f), 23, TextAnchor.MiddleLeft, TextColor);
            GameObject root = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(150f, y), new Vector2(640f, 52f), new Color(0.06f, 0.12f, 0.16f, 1f));
            Text caption = CreateText(root.transform, "Label", "1920 × 1080", Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-34f, -8f), 22, TextAnchor.MiddleLeft, TextColor);
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
            dropdown.targetGraphic = root.GetComponent<Image>();
            dropdown.template = template.GetComponent<RectTransform>();
            dropdown.captionText = caption;
            dropdown.itemText = itemLabel;
            template.SetActive(false);
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
            public InteractionPromptController InteractionPrompt;
            public EvidenceNotificationController EvidenceNotification;
            public PauseMenuController PauseMenuController;
            public ObjectivePresenter ObjectivePresenter;
        }
    }
}
