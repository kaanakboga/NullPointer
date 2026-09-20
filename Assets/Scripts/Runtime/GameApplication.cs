using System;
using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Evidence;
using NullPointer.Input;
using NullPointer.Memory;
using NullPointer.SceneFlow;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NullPointer.Runtime
{
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("Null Pointer/Runtime/Game Application")]
    public sealed class GameApplication : MonoBehaviour
    {
        [SerializeField] private GameModeController _gameModes;
        [SerializeField] private GameplayInputReader _inputReader;
        [SerializeField] private PauseInputHandler _pauseHandler;
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private ContentCatalog _contentCatalog;
        [SerializeField] private LocationData _startingLocation;
        [SerializeField] private string _startingSpawnPointId = "entry";

        private bool _isInitialized;

        public GameState GameState { get; private set; }

        public EvidenceService EvidenceService { get; private set; }

        public DeductionService DeductionService { get; private set; }

        public MemoryService MemoryService { get; private set; }

        public GameModeController GameModes => _gameModes;

        public GameplayInputReader InputReader => _inputReader;

        public SceneLoader SceneLoader => _sceneLoader;

        public void Configure(
            GameModeController gameModes,
            GameplayInputReader inputReader,
            PauseInputHandler pauseHandler,
            SceneLoader sceneLoader,
            ContentCatalog contentCatalog,
            LocationData startingLocation,
            string startingSpawnPointId)
        {
            _gameModes = gameModes;
            _inputReader = inputReader;
            _pauseHandler = pauseHandler;
            _sceneLoader = sceneLoader;
            _contentCatalog = contentCatalog;
            _startingLocation = startingLocation;
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
            GameState = new GameState();
            EvidenceService = new EvidenceService(GameState, _contentCatalog.Evidence);
            DeductionService = new DeductionService(GameState, EvidenceService, _contentCatalog.Deductions);
            MemoryService = new MemoryService(GameState);

            _inputReader.InitializeModeGate(_gameModes);
            _pauseHandler.Initialize(_gameModes, _inputReader);
            _sceneLoader.Initialize(GameState);
            SceneManager.sceneLoaded += OnSceneLoaded;
            _isInitialized = true;
        }

        private void Start()
        {
            if (_startingLocation != null && SceneManager.GetActiveScene().name == "SCN_Bootstrap")
            {
                _sceneLoader.LoadLocation(_startingLocation, _startingSpawnPointId);
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
