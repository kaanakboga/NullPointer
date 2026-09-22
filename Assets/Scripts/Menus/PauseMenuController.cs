using System;
using NullPointer.Core;
using NullPointer.Input;
using NullPointer.Journal;
using NullPointer.Settings;
using UnityEngine;

namespace NullPointer.Menus
{
    [AddComponentMenu("Null Pointer/Menus/Pause Menu Controller")]
    public sealed class PauseMenuController : MonoBehaviour
    {
        [SerializeField] private PauseMenuPanel _panel;
        [SerializeField] private JournalPanel _journal;
        [SerializeField] private SettingsPanel _settings;
        private GameModeController _gameModes;
        private IGameSessionCommands _commands;
        private SettingsManager _settingsManager;
        private PauseInputHandler _pauseInput;
        private ChildPanel _activeChild;

        public void Configure(PauseMenuPanel panel, JournalPanel journal, SettingsPanel settings)
        {
            _panel = panel;
            _journal = journal;
            _settings = settings;
        }

        public void Initialize(
            GameModeController gameModes,
            IGameSessionCommands commands,
            JournalService journalService,
            SettingsManager settingsManager,
            PauseInputHandler pauseInput)
        {
            if (_gameModes != null)
            {
                _gameModes.ModeChanged -= OnModeChanged;
            }

            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            if (_pauseInput != null)
            {
                _pauseInput.UnregisterCancelInterceptor(TryCloseChildPanel);
            }

            _pauseInput = pauseInput ?? throw new ArgumentNullException(nameof(pauseInput));
            _pauseInput.RegisterCancelInterceptor(TryCloseChildPanel);
            _panel.Bind(this);
            _journal.Bind(journalService, ShowPausePanel);
            _settings.Bind(settingsManager, ShowPausePanel);
            _gameModes.ModeChanged += OnModeChanged;
            _panel.Hide();
            _journal.Hide();
            _settings.Hide();
            _activeChild = ChildPanel.None;
        }

        public void Resume()
        {
            _gameModes.SetMode(GameMode.Gameplay);
        }

        public void OpenJournal()
        {
            _panel.Hide();
            _activeChild = ChildPanel.Journal;
            _journal.Show();
        }

        public void OpenSettings()
        {
            _panel.Hide();
            _activeChild = ChildPanel.Settings;
            _settings.Show(_settingsManager.Current);
        }

        public void ReturnToMainMenu()
        {
            _commands.SaveGame();
            _commands.ReturnToMainMenu();
        }

        private void ShowPausePanel()
        {
            _activeChild = ChildPanel.None;
            if (_gameModes.CurrentMode == GameMode.Paused)
            {
                _panel.Show();
            }
        }

        private void OnModeChanged(GameModeChanged change)
        {
            if (change.CurrentMode == GameMode.Paused)
            {
                _panel.Show();
            }
            else
            {
                _panel.Hide();
                _journal.Hide();
                _settings.Hide();
            }
        }

        private void OnDestroy()
        {
            if (_gameModes != null)
            {
                _gameModes.ModeChanged -= OnModeChanged;
            }

            if (_pauseInput != null)
            {
                _pauseInput.UnregisterCancelInterceptor(TryCloseChildPanel);
            }
        }

        private bool TryCloseChildPanel()
        {
            switch (_activeChild)
            {
                case ChildPanel.Journal:
                    _journal.Close();
                    return true;
                case ChildPanel.Settings:
                    _settings.Close();
                    return true;
                default:
                    return false;
            }
        }

        private enum ChildPanel
        {
            None = 0,
            Journal = 1,
            Settings = 2
        }
    }
}
