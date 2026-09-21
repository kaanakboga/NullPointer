using System;
using NullPointer.Settings;
using UnityEngine;

namespace NullPointer.Menus
{
    [AddComponentMenu("Null Pointer/Menus/Main Menu Controller")]
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private MainMenuPanel _panel;
        [SerializeField] private SettingsPanel _settingsPanel;
        private IGameSessionCommands _commands;
        private SettingsManager _settings;

        public bool ContinueEnabled => MainMenuAvailability.CanContinue(_commands);

        public void Configure(MainMenuPanel panel, SettingsPanel settingsPanel)
        {
            _panel = panel;
            _settingsPanel = settingsPanel;
        }

        public void Initialize(IGameSessionCommands commands, SettingsManager settings)
        {
            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _panel.Bind(this);
            _settingsPanel.Bind(_settings, () => _panel.Show(ContinueEnabled));
            _settingsPanel.Hide();
            _panel.Show(ContinueEnabled);
        }

        public void StartNewGame()
        {
            _commands?.StartNewGame();
        }

        public void ContinueGame()
        {
            if (ContinueEnabled)
            {
                _commands.ContinueGame();
            }
        }

        public void OpenSettings()
        {
            _panel.Hide();
            _settingsPanel.Show(_settings.Current);
        }

        public void Quit()
        {
            _commands?.QuitGame();
        }
    }
}
