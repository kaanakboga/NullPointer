using System;
using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Evidence;
using NullPointer.Input;
using NullPointer.Interrogation;
using NullPointer.Journal;
using NullPointer.Memory;
using NullPointer.Menus;
using NullPointer.Progression;
using NullPointer.Save;
using NullPointer.SceneFlow;
using NullPointer.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NullPointer.Runtime
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Null Pointer/Runtime/Game Application")]
    public sealed class GameApplication : MonoBehaviour, IGameSessionCommands
    {
        [SerializeField] private GameModeController _gameModes;
        [SerializeField] private GameplayInputReader _inputReader;
        [SerializeField] private PauseInputHandler _pauseHandler;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private ContentCatalog _contentCatalog;
        [SerializeField] private LocationData _startingLocation;
        [SerializeField] private LocationData _mainMenuLocation;
        [SerializeField] private CheckpointData _startingCheckpoint;
        [SerializeField] private string _startingSpawnPointId = "entry";

        private bool _isInitialized;

        public GameState GameState { get; private set; }

        public EvidenceService EvidenceService { get; private set; }

        public DeductionService DeductionService { get; private set; }

        public MemoryService MemoryService { get; private set; }

        public ObjectiveService ObjectiveService { get; private set; }

        public CheckpointService CheckpointService { get; private set; }

        public ChapterProgressionService ChapterProgressionService { get; private set; }

        public JournalService JournalService { get; private set; }

        public InterrogationService InterrogationService { get; private set; }

        public SaveManager SaveManager { get; private set; }

        public SettingsManager SettingsManager { get; private set; }

        public string SaveFilePath { get; private set; } = string.Empty;

        public bool HasContinue => SaveManager != null && SaveManager.HasValidSave;

        public GameModeController GameModes => _gameModes;

        public GameplayInputReader InputReader => _inputReader;

        public PauseInputHandler PauseHandler => _pauseHandler;

        public SceneLoader SceneLoader => _sceneLoader;

        public void Configure(
            GameModeController gameModes,
            GameplayInputReader inputReader,
            PauseInputHandler pauseHandler,
            SceneLoader sceneLoader,
            ContentCatalog contentCatalog,
            LocationData startingLocation,
            LocationData mainMenuLocation,
            CheckpointData startingCheckpoint,
            string startingSpawnPointId)
        {
            _gameModes = gameModes;
            _inputReader = inputReader;
            _pauseHandler = pauseHandler;
            _sceneLoader = sceneLoader;
            _contentCatalog = contentCatalog;
            _startingLocation = startingLocation;
            _mainMenuLocation = mainMenuLocation;
            _startingCheckpoint = startingCheckpoint;
            _startingSpawnPointId = startingSpawnPointId;
        }

        private void Awake()
        {
            if (_isInitialized)
            {
                return;
            }

            if (_gameModes == null || _inputReader == null || _pauseHandler == null ||
                _sceneLoader == null || _contentCatalog == null)
            {
                throw new InvalidOperationException("[Runtime] GameApplication is missing Bootstrap dependencies.");
            }

            DontDestroyOnLoad(gameObject);
            SaveFilePath = System.IO.Path.Combine(Application.persistentDataPath, "save-v1.json");
            SaveManager = new SaveManager(
                new FileSaveStorage(SaveFilePath),
                Application.version,
                ReportSaveDiagnostic);
            SettingsManager = new SettingsManager(
                new FileSettingsStorage(System.IO.Path.Combine(Application.persistentDataPath, "settings-v1.json")));
            SettingsManager.Apply(false);
            RebuildSession(new GameState());

            _inputReader.InitializeModeGate(_gameModes);
            _pauseHandler.Initialize(_gameModes, _inputReader);
            SceneManager.sceneLoaded += OnSceneLoaded;
            _isInitialized = true;
        }

        private void Start()
        {
            if (_mainMenuLocation != null && SceneManager.GetActiveScene().name == "SCN_Bootstrap")
            {
                _sceneLoader.LoadLocation(_mainMenuLocation, string.Empty);
            }
        }

        public bool StartNewGame()
        {
            SaveManager.Delete();
            RebuildSession(new GameState());
            _pauseHandler.enabled = true;
            _gameModes.SetMode(GameMode.Gameplay);
            if (_startingCheckpoint != null)
            {
                CheckpointService.Activate(_startingCheckpoint.StableId);
            }

            return _startingLocation != null &&
                   _sceneLoader.LoadLocation(_startingLocation, _startingSpawnPointId);
        }

        public bool ContinueGame()
        {
            SaveLoadResult load = SaveManager.Load();
            if (!load.IsSuccess)
            {
                return false;
            }

            RebuildSession(load.GameState);
            _pauseHandler.enabled = true;
            string fallbackCheckpointId = _startingCheckpoint == null
                ? string.Empty
                : _startingCheckpoint.StableId;
            var recovery = new LoadedSessionRecovery(_gameModes, CheckpointService);
            CheckpointData checkpoint = recovery.Restore(
                fallbackCheckpointId,
                out bool usedFallback);
            if (usedFallback && checkpoint != null)
            {
                Debug.LogWarning(
                    $"[Save] Loaded checkpoint was invalid. Restored safe checkpoint '{checkpoint.StableId}'.");
                SaveManager.Save(GameState);
            }

            return checkpoint != null &&
                   _sceneLoader.LoadLocation(checkpoint.Location, checkpoint.SpawnPointId);
        }

        public SaveLoadResult SaveGame()
        {
            return SaveManager.Save(GameState);
        }

        public void ReturnToMainMenu()
        {
            PrepareMainMenu();
            if (_mainMenuLocation != null)
            {
                _sceneLoader.LoadLocation(_mainMenuLocation, string.Empty);
            }
        }

        public void PrepareMainMenu()
        {
            _pauseHandler.enabled = false;
            _gameModes.SetMode(GameMode.Paused);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            Debug.Log("[Runtime] Quit requested from Main Menu.");
#else
            Application.Quit();
#endif
        }

        private void RebuildSession(GameState gameState)
        {
            GameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            EvidenceService = new EvidenceService(GameState, _contentCatalog.Evidence);
            DeductionService = new DeductionService(GameState, EvidenceService, _contentCatalog.Deductions);
            MemoryService = new MemoryService(GameState);
            ObjectiveService = new ObjectiveService(GameState, _contentCatalog.Objectives);
            CheckpointService = new CheckpointService(GameState, SaveManager, _contentCatalog.Checkpoints);
            ChapterProgressionService = new ChapterProgressionService(GameState);
            JournalService = new JournalService(GameState, EvidenceService, _contentCatalog.JournalEntries);
            InterrogationService = new InterrogationService(GameState, _contentCatalog.InterrogationClaims);
            _sceneLoader.Initialize(GameState);
        }

        private static void ReportSaveDiagnostic(SaveDiagnostic diagnostic)
        {
            if (diagnostic == null)
            {
                return;
            }

            string message = $"[Save] {diagnostic.Message}";
            switch (diagnostic.Severity)
            {
                case SaveDiagnosticSeverity.Error:
                    Debug.LogError(message);
                    break;
                case SaveDiagnosticSeverity.Warning:
                    Debug.LogWarning(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }

            if (diagnostic.Exception != null)
            {
                Debug.LogException(diagnostic.Exception);
            }
        }

        private void OnDestroy()
        {
            if (_isInitialized)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                MainMenuSceneInstaller menuInstaller = root.GetComponentInChildren<MainMenuSceneInstaller>(true);
                if (menuInstaller != null)
                {
                    menuInstaller.Install(this);
                    return;
                }
            }

            OpeningSceneInstaller installer = null;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                OpeningSceneInstaller candidate = root.GetComponentInChildren<OpeningSceneInstaller>(true);
                if (candidate == null)
                {
                    continue;
                }

                if (installer != null)
                {
                    Debug.LogError($"[Runtime] Scene '{scene.name}' contains multiple OpeningSceneInstaller components.");
                    return;
                }

                installer = candidate;
            }

            if (installer != null)
            {
                installer.Install(this, _sceneLoader.PendingSpawnPointId);
            }
        }
    }
}
