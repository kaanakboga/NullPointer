using System;
using NullPointer.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NullPointer.Input
{
    [AddComponentMenu("Null Pointer/Input/Gameplay Input Reader")]
    public sealed class GameplayInputReader : MonoBehaviour, IGameplayInputSource
    {
        [SerializeField] private InputActionAsset _actions;

        private InputActionMap _gameplayMap;
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;
        private InputAction _cancelAction;
        private GameModeController _gameModeController;
        private bool _isBound;
        private bool _isModeSubscribed;
        private int _lastPausePressedFrame = -1;

        public event Action InteractPressed;
        public event Action PausePressed;

        public Vector2 Move => _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;

        public void InitializeModeGate(GameModeController gameModeController)
        {
            UnsubscribeFromModeChanges();
            _gameModeController = gameModeController;
            SubscribeToModeChanges();
            ApplyModeGate();
        }

        public void Configure(InputActionAsset actions)
        {
            if (isActiveAndEnabled)
            {
                _gameplayMap?.Disable();
                Unbind();
            }

            _actions = actions;

            if (isActiveAndEnabled && TryBind())
            {
                _gameplayMap.Enable();
                _cancelAction.Enable();
            }
        }

        private void OnEnable()
        {
            if (TryBind())
            {
                _gameplayMap.Enable();
                _cancelAction?.Enable();
                ApplyModeGate();
            }

            SubscribeToModeChanges();
        }

        private void OnDisable()
        {
            UnsubscribeFromModeChanges();
            if (_gameplayMap != null)
            {
                _gameplayMap.Disable();
            }

            _cancelAction?.Disable();

            Unbind();
        }

        private bool TryBind()
        {
            if (_isBound)
            {
                return true;
            }

            if (_actions == null)
            {
                Debug.LogError("[Input] GameplayInputReader requires an InputActionAsset.", this);
                return false;
            }

            _gameplayMap = _actions.FindActionMap(GameplayInputActionNames.Map, false);
            _moveAction = _gameplayMap?.FindAction(GameplayInputActionNames.Move, false);
            _interactAction = _gameplayMap?.FindAction(GameplayInputActionNames.Interact, false);
            _pauseAction = _gameplayMap?.FindAction(GameplayInputActionNames.Pause, false);
            _cancelAction = _actions.FindAction("UI/Cancel", false);

            if (_gameplayMap == null || _moveAction == null || _interactAction == null ||
                _pauseAction == null || _cancelAction == null)
            {
                Debug.LogError(
                    "[Input] The configured asset must contain Gameplay/Move, Gameplay/Interact, and Gameplay/Pause actions.",
                    this);
                ClearResolvedActions();
                return false;
            }

            _interactAction.performed += OnInteractPerformed;
            _pauseAction.performed += OnPausePerformed;
            _cancelAction.performed += OnCancelPerformed;
            _isBound = true;
            return true;
        }

        private void Unbind()
        {
            if (_isBound)
            {
                _interactAction.performed -= OnInteractPerformed;
                _pauseAction.performed -= OnPausePerformed;
                _cancelAction.performed -= OnCancelPerformed;
            }

            _isBound = false;
            ClearResolvedActions();
        }

        private void ClearResolvedActions()
        {
            _gameplayMap = null;
            _moveAction = null;
            _interactAction = null;
            _pauseAction = null;
            _cancelAction = null;
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            InteractPressed?.Invoke();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            RaisePausePressedOncePerFrame();
        }

        private void OnCancelPerformed(InputAction.CallbackContext context)
        {
            if (_gameModeController != null && _gameModeController.CurrentMode != GameMode.Gameplay)
            {
                RaisePausePressedOncePerFrame();
            }
        }

        private void RaisePausePressedOncePerFrame()
        {
            if (_lastPausePressedFrame == Time.frameCount)
            {
                return;
            }

            _lastPausePressedFrame = Time.frameCount;
            PausePressed?.Invoke();
        }

        private void SubscribeToModeChanges()
        {
            if (_isModeSubscribed || _gameModeController == null)
            {
                return;
            }

            _gameModeController.ModeChanged += OnGameModeChanged;
            _isModeSubscribed = true;
        }

        private void UnsubscribeFromModeChanges()
        {
            if (!_isModeSubscribed || _gameModeController == null)
            {
                return;
            }

            _gameModeController.ModeChanged -= OnGameModeChanged;
            _isModeSubscribed = false;
        }

        private void OnGameModeChanged(GameModeChanged change)
        {
            ApplyModeGate();
        }

        private void ApplyModeGate()
        {
            if (!_isBound || _gameModeController == null)
            {
                return;
            }

            bool gameplayEnabled = _gameModeController.CurrentMode == GameMode.Gameplay;
            if (gameplayEnabled)
            {
                _moveAction.Enable();
                _interactAction.Enable();
            }
            else
            {
                _moveAction.Disable();
                _interactAction.Disable();
            }
        }
    }
}
