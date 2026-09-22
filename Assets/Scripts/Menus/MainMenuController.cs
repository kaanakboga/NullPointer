using System;
using NullPointer.Input;
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
        private IGameplayInputSource _input;
        private bool _settingsOpen;

        public bool ContinueEnabled => MainMenuAvailability.CanContinue(_commands);

        public void Configure(MainMenuPanel panel, SettingsPanel settingsPanel)
        {
            _panel = panel;
            _settingsPanel = settingsPanel;
        }

        public void Initialize(
            IGameSessionCommands commands,
            SettingsManager settings,
            IGameplayInputSource input)
        {
            if (_input != null)
            {
                _input.PausePressed -= OnCancelPressed;
            }

            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _input.PausePressed += OnCancelPressed;
            _panel.Bind(this);
            _settingsPanel.Bind(_settings, CloseSettings);
            _settingsPanel.Hide();
            _settingsOpen = false;
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
            _settingsOpen = true;
            _settingsPanel.Show(_settings.Current);
        }

        public void Quit()
        {
            _commands?.QuitGame();
        }

        private void CloseSettings()
        {
            _settingsOpen = false;
            _panel.Show(ContinueEnabled);
        }

        private void OnCancelPressed()
        {
            if (_settingsOpen)
            {
                _settingsPanel.Close();
            }
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.PausePressed -= OnCancelPressed;
            }
        }
    }
}
