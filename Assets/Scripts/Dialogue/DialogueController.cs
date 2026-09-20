using System;
using NullPointer.Core;
using NullPointer.Evidence;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Dialogue
{
    [AddComponentMenu("Null Pointer/Dialogue/Dialogue Controller")]
    public sealed class DialogueController : MonoBehaviour
    {
        [SerializeField] private DialoguePanel _panel;

        private GameModeController _gameModes;
        private IGameplayInputSource _input;
        private GameState _gameState;
        private EvidenceService _evidenceService;
        private DialogueRunner _runner;
        private bool _isInputSubscribed;

        public event Action<DialogueAction> ExternalActionRequested;

        public bool IsOpen => _gameModes != null && _gameModes.CurrentMode == GameMode.Dialogue;

        public void Configure(DialoguePanel panel)
        {
            _panel = panel;
        }

        public void Initialize(
            GameModeController gameModes,
            IGameplayInputSource input,
            GameState gameState,
            EvidenceService evidenceService)
        {
            Unsubscribe();
            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _runner = new DialogueRunner(gameState);
            _runner.ActionRequested += ExecuteAction;
            _runner.Completed += Close;
            _panel.Bind(this);
        }

        public bool Open(DialogueData dialogue)
        {
            if (_runner == null || dialogue == null || !_runner.Start(dialogue))
            {
                return false;
            }

            _gameModes.SetMode(GameMode.Dialogue);
            Render();
            Subscribe();
            return true;
        }

        public void Advance()
        {
            if (!IsOpen || _runner == null)
            {
                return;
            }

            _runner.Advance();
            if (_runner.IsRunning)
            {
                Render();
            }
        }

        public void SelectChoice(int index)
        {
            if (!IsOpen || _runner == null)
            {
                return;
            }

            _runner.SelectChoice(index);
            if (_runner.IsRunning)
            {
                Render();
            }
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            Unsubscribe();
            _runner?.Stop();
            _panel.Hide();
            _gameModes.SetMode(GameMode.Gameplay);
        }

        private void ExecuteAction(DialogueAction action)
        {
            if (action == null || string.IsNullOrWhiteSpace(action.TargetId))
            {
                return;
            }

            switch (action.ActionType)
            {
                case DialogueActionType.SetStoryFlag:
                    _gameState.SetStoryFlag(action.TargetId);
                    break;
                case DialogueActionType.AddEvidence:
                    _evidenceService.Collect(action.TargetId);
                    break;
                case DialogueActionType.StartMemory:
                case DialogueActionType.SceneTransition:
                    ExternalActionRequested?.Invoke(action);
                    break;
            }
        }

        private void Render()
        {
            _panel.Show(_runner.CurrentNode, _runner.AvailableChoices);
        }

        private void Subscribe()
        {
            if (_isInputSubscribed)
            {
                return;
            }

            _input.InteractPressed += Advance;
            _input.PausePressed += Close;
            _isInputSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isInputSubscribed || _input == null)
            {
                return;
            }

            _input.InteractPressed -= Advance;
            _input.PausePressed -= Close;
            _isInputSubscribed = false;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }
    }
}
