using System;
using System.Linq;
using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Inspect;
using NullPointer.Interaction;
using NullPointer.Memory;
using NullPointer.Menus;
using NullPointer.Player;
using NullPointer.Progression;
using NullPointer.SceneFlow;
using NullPointer.Terminal;
using NullPointer.UI;
using UnityEngine;

namespace NullPointer.Runtime
{
    [AddComponentMenu("Null Pointer/Runtime/Opening Scene Installer")]
    public sealed class OpeningSceneInstaller : MonoBehaviour
    {
        [SerializeField] private LocationData _location;
        [SerializeField] private PlayerController _player;
        [SerializeField] private PlayerInteractionDetector _interactionDetector;
        [SerializeField] private InspectController _inspectController;
        [SerializeField] private DialogueController _dialogueController;
        [SerializeField] private TerminalController _terminalController;
        [SerializeField] private MemoryController _memoryController;
        [SerializeField] private EvidenceBoardController _evidenceBoardController;
        [SerializeField] private InteractionPromptController _interactionPrompt;
        [SerializeField] private EvidenceNotificationController _evidenceNotification;
        [SerializeField] private PauseMenuController _pauseMenuController;
        [SerializeField] private ObjectivePresenter _objectivePresenter;
        [SerializeField] private ProgressionCoordinator _progressionCoordinator;
        [SerializeField] private ChapterEndSequenceController _chapterEndSequence;
        [SerializeField] private GameplayAudioHooks _audioHooks;
        [SerializeField] private SceneTransitionInteractable[] _sceneTransitions =
            Array.Empty<SceneTransitionInteractable>();
        [SerializeField] private SpawnPoint[] _spawnPoints = Array.Empty<SpawnPoint>();

        public void Configure(
            LocationData location,
            PlayerController player,
            PlayerInteractionDetector interactionDetector,
            InspectController inspectController,
            DialogueController dialogueController,
            TerminalController terminalController,
            MemoryController memoryController,
            EvidenceBoardController evidenceBoardController,
            InteractionPromptController interactionPrompt,
            EvidenceNotificationController evidenceNotification,
            SceneTransitionInteractable[] sceneTransitions,
            SpawnPoint[] spawnPoints)
        {
            _location = location;
            _player = player;
            _interactionDetector = interactionDetector;
            _inspectController = inspectController;
            _dialogueController = dialogueController;
            _terminalController = terminalController;
            _memoryController = memoryController;
            _evidenceBoardController = evidenceBoardController;
            _interactionPrompt = interactionPrompt;
            _evidenceNotification = evidenceNotification;
            _sceneTransitions = sceneTransitions ?? Array.Empty<SceneTransitionInteractable>();
            _spawnPoints = spawnPoints ?? Array.Empty<SpawnPoint>();
        }

        public void ConfigureProduction(
            PauseMenuController pauseMenuController,
            ObjectivePresenter objectivePresenter,
            ProgressionCoordinator progressionCoordinator,
            ChapterEndSequenceController chapterEndSequence)
        {
            _pauseMenuController = pauseMenuController;
            _objectivePresenter = objectivePresenter;
            _progressionCoordinator = progressionCoordinator;
            _chapterEndSequence = chapterEndSequence;
        }

        public void ConfigureAudioHooks(GameplayAudioHooks audioHooks)
        {
            _audioHooks = audioHooks;
        }

        public void Install(GameApplication application, string requestedSpawnPointId)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.GameModes.SetMode(GameMode.Gameplay);
            if (_location != null)
            {
                application.GameState.SetLocation(_location.StableId);
            }

            Rigidbody2D body = _player.GetComponent<Rigidbody2D>();
            _player.Initialize(body, application.GameModes, application.InputReader);
            _interactionDetector.Initialize(application.GameModes, application.InputReader, _player.transform);
            _inspectController.Initialize(application.GameModes, application.InputReader, application.EvidenceService);
            _dialogueController.Initialize(
                application.GameModes,
                application.InputReader,
                application.GameState,
                application.EvidenceService);
            _dialogueController.SetTextSpeed(application.SettingsManager.Current.TextSpeed);
            _terminalController.Initialize(
                application.GameModes,
                application.InputReader,
                application.GameState,
                application.EvidenceService);
            _memoryController.Initialize(application.GameModes, application.InputReader, application.MemoryService);
            _evidenceBoardController.Initialize(
                application.GameModes,
                application.InputReader,
                application.EvidenceService,
                application.DeductionService);
            _interactionPrompt.Initialize(_interactionDetector);
            _evidenceNotification.Initialize(application.EvidenceService);
            _objectivePresenter?.Initialize(application.ObjectiveService);
            _pauseMenuController?.Initialize(
                application.GameModes,
                application,
                application.JournalService,
                application.SettingsManager,
                application.PauseHandler);

            foreach (SceneTransitionInteractable transition in _sceneTransitions)
            {
                transition?.Initialize(application.SceneLoader, application.GameState);
            }

            SpawnAt(requestedSpawnPointId);
            _progressionCoordinator?.Initialize(application);
            _chapterEndSequence?.Initialize(application);
            _audioHooks?.Initialize(application);
        }

        private void SpawnAt(string requestedSpawnPointId)
        {
            SpawnPoint target = _spawnPoints.FirstOrDefault(point =>
                point != null && string.Equals(
                    point.SpawnPointId,
                    requestedSpawnPointId,
                    StringComparison.Ordinal));
            target ??= _spawnPoints.FirstOrDefault(point => point != null);
            if (target == null)
            {
                Debug.LogWarning($"[Runtime] Scene '{gameObject.scene.name}' has no configured spawn point.", this);
                return;
            }

            _player.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);
        }
    }
}
