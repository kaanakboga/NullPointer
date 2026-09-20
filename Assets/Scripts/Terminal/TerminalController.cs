using System;
using NullPointer.Core;
using NullPointer.Evidence;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Terminal
{
    [AddComponentMenu("Null Pointer/Terminal/Terminal Controller")]
    public sealed class TerminalController : MonoBehaviour
    {
        [SerializeField] private TerminalPanel _panel;

        private GameModeController _gameModes;
        private IGameplayInputSource _input;
        private GameState _gameState;
        private EvidenceService _evidenceService;
        private TerminalData _currentTerminal;
        private bool _isInputSubscribed;

        public bool IsOpen => _gameModes != null && _gameModes.CurrentMode == GameMode.Terminal;

        public void Configure(TerminalPanel panel)
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
            _panel.Bind(this);
        }

        public void Open(TerminalData terminal)
        {
            if (terminal == null || _gameModes == null)
            {
                return;
            }

            _currentTerminal = terminal;
            _gameModes.SetMode(GameMode.Terminal);
            _panel.Show(terminal);
            Subscribe();
        }

        public void SelectEntry(int index)
        {
            if (!IsOpen || _currentTerminal == null || index < 0 || index >= _currentTerminal.Entries.Count)
            {
                return;
            }

            TerminalEntry entry = _currentTerminal.Entries[index];
            bool collected = false;
            if (!string.IsNullOrWhiteSpace(entry.EvidenceId))
            {
                EvidenceCollectionStatus result = _evidenceService.Collect(entry.EvidenceId);
                collected = result == EvidenceCollectionStatus.Collected ||
                            result == EvidenceCollectionStatus.AlreadyCollected;
            }

            if (!string.IsNullOrWhiteSpace(entry.StoryFlagToSet))
            {
                _gameState.SetStoryFlag(entry.StoryFlagToSet);
            }

            _panel.ShowEntry(entry, collected);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            Unsubscribe();
            _currentTerminal = null;
            _panel.Hide();
            _gameModes.SetMode(GameMode.Gameplay);
        }

        private void Subscribe()
        {
            if (_isInputSubscribed)
            {
                return;
            }

            _input.PausePressed += Close;
            _isInputSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isInputSubscribed || _input == null)
            {
                return;
            }

            _input.PausePressed -= Close;
            _isInputSubscribed = false;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }
    }
}
