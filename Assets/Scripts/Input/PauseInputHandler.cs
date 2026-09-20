using NullPointer.Core;
using UnityEngine;

namespace NullPointer.Input
{
    [AddComponentMenu("Null Pointer/Input/Pause Input Handler")]
    public sealed class PauseInputHandler : MonoBehaviour
    {
        [SerializeField] private GameModeController _gameModeController;
        [SerializeField] private GameplayInputReader _inputReader;

        private IGameplayInputSource _inputSource;
        private GameMode _modeBeforePause = GameMode.Gameplay;

        public void Initialize(GameModeController gameModeController, IGameplayInputSource inputSource)
        {
            Unsubscribe();
            _gameModeController = gameModeController;
            _inputReader = inputSource as GameplayInputReader;
            _inputSource = inputSource;
            Subscribe();
        }

        public void TogglePause()
        {
            if (_gameModeController == null)
            {
                return;
            }

            if (_gameModeController.CurrentMode == GameMode.Paused)
            {
                _gameModeController.SetMode(_modeBeforePause);
                return;
            }

            if (_gameModeController.CurrentMode != GameMode.Gameplay)
            {
                return;
            }

            _modeBeforePause = _gameModeController.CurrentMode;
            _gameModeController.SetMode(GameMode.Paused);
        }

        private void Awake()
        {
            _inputSource ??= _inputReader;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_inputSource != null)
            {
                _inputSource.PausePressed -= TogglePause;
                _inputSource.PausePressed += TogglePause;
            }
        }

        private void Unsubscribe()
        {
            if (_inputSource != null)
            {
                _inputSource.PausePressed -= TogglePause;
            }
        }
    }
}
