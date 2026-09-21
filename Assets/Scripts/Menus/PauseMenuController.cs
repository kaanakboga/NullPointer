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
            SettingsManager settingsManager)
        {
            if (_gameModes != null)
            {
                _gameModes.ModeChanged -= OnModeChanged;
            }

            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
            _panel.Bind(this);
            _journal.Bind(journalService, ShowPausePanel);
            _settings.Bind(settingsManager, ShowPausePanel);
            _gameModes.ModeChanged += OnModeChanged;
            _panel.Hide();
            _journal.Hide();
            _settings.Hide();
        }

        public void Resume()
        {
            _gameModes.SetMode(GameMode.Gameplay);
        }

        public void OpenJournal()
        {
            _panel.Hide();
            _journal.Show();
        }

        public void OpenSettings()
        {
            _panel.Hide();
            _settings.Show(_settingsManager.Current);
        }

        public void ReturnToMainMenu()
        {
            _commands.SaveGame();
            _commands.ReturnToMainMenu();
        }

        private void ShowPausePanel()
        {
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
        }
    }
}
