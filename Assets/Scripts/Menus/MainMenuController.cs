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
        [SerializeField] private MainMenuPresentation _presentation;
        private IGameSessionCommands _commands;
        private SettingsManager _settings;
        private IGameplayInputSource _input;
        private bool _settingsOpen;
        private bool _commandInFlight;

        public bool ContinueEnabled => MainMenuAvailability.CanContinue(_commands);

        public void Configure(MainMenuPanel panel, SettingsPanel settingsPanel, MainMenuPresentation presentation = null)
        {
            _panel = panel;
            _settingsPanel = settingsPanel;
            _presentation = presentation;
        }

        public void Initialize(
            IGameSessionCommands commands,
            SettingsManager settings,
            IGameplayInputSource input)
        {
            if (_input != null)
            {
                _input.PausePressed -= OnCancelPressed;
                _input.InteractPressed -= OnAcceleratePressed;
            }

            _commands = commands ?? throw new ArgumentNullException(nameof(commands));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _input.PausePressed += OnCancelPressed;
            _input.InteractPressed += OnAcceleratePressed;
            _panel.Bind(this);
            _settingsPanel.Bind(_settings, CloseSettings);
            _settingsPanel.Hide();
            _settingsOpen = false;
            _commandInFlight = false;
            _panel.Show(ContinueEnabled);
            _presentation?.Initialize(_settings);
        }

        public void StartNewGame()
        {
            RunCommand(false, () => _commands?.StartNewGame());
        }

        public void ContinueGame()
        {
            if (ContinueEnabled)
            {
                RunCommand(true, () => _commands.ContinueGame());
            }
        }

        public void OpenSettings()
        {
            if (_commandInFlight)
            {
                return;
            }

            _panel.Hide();
            _settingsOpen = true;
            _settingsPanel.Show(_settings.Current);
        }

        public void Quit()
        {
            RunCommand(false, () => _commands?.QuitGame());
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

        private void OnAcceleratePressed()
        {
            _presentation?.AccelerateReveal();
        }

        private void RunCommand(bool continuation, Action command)
        {
            if (_commandInFlight || command == null)
            {
                return;
            }

            _commandInFlight = true;
            if (_presentation == null || !_presentation.BeginDeparture(continuation, command))
            {
                command.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.PausePressed -= OnCancelPressed;
                _input.InteractPressed -= OnAcceleratePressed;
            }
        }
    }
}
