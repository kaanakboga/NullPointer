using System;
using System.Collections.Generic;
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
        private readonly List<string> _history = new List<string>();
        private float _textSecondsPerCharacter = 0.025f;

        public event Action<DialogueAction> ExternalActionRequested;

        public bool IsOpen => _gameModes != null && _gameModes.CurrentMode == GameMode.Dialogue;

        public IReadOnlyList<string> History => _history;

        public void Configure(DialoguePanel panel)
        {
            _panel = panel;
        }

        public void SetTextSpeed(float secondsPerCharacter)
        {
            _textSecondsPerCharacter = Mathf.Clamp(secondsPerCharacter, 0f, 0.08f);
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
            _history.Clear();
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


            if (_panel.RevealImmediately())
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
            DialogueNode node = _runner.CurrentNode;
            string speaker = node?.Speaker == null ? string.Empty : node.Speaker.DisplayName;
            _history.Add(string.IsNullOrWhiteSpace(speaker)
                ? node?.Text ?? string.Empty
                : $"{speaker}: {node?.Text}");
            if (node != null)
            {
                _gameState.RecordDialogueProgress($"dialogue.progress.{node.NodeId}");
            }

            _panel.Show(node, _runner.AvailableChoices, _history, _textSecondsPerCharacter);
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
