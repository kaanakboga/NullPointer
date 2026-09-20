using System;
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
        private bool _isBound;

        public event Action InteractPressed;
        public event Action PausePressed;

        public Vector2 Move => _moveAction?.ReadValue<Vector2>() ?? Vector2.zero;

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
            }
        }

        private void OnEnable()
        {
            if (TryBind())
            {
                _gameplayMap.Enable();
            }
        }

        private void OnDisable()
        {
            if (_gameplayMap != null)
            {
                _gameplayMap.Disable();
            }

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

            if (_gameplayMap == null || _moveAction == null || _interactAction == null || _pauseAction == null)
            {
                Debug.LogError(
                    "[Input] The configured asset must contain Gameplay/Move, Gameplay/Interact, and Gameplay/Pause actions.",
                    this);
                ClearResolvedActions();
                return false;
            }

            _interactAction.performed += OnInteractPerformed;
            _pauseAction.performed += OnPausePerformed;
            _isBound = true;
            return true;
        }

        private void Unbind()
        {
            if (_isBound)
            {
                _interactAction.performed -= OnInteractPerformed;
                _pauseAction.performed -= OnPausePerformed;
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
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            InteractPressed?.Invoke();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            PausePressed?.Invoke();
        }
    }
}
