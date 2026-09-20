using NullPointer.Core;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("Null Pointer/Player/Player Controller")]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody;
        [SerializeField] private GameModeController _gameModeController;
        [SerializeField] private GameplayInputReader _inputReader;
        [SerializeField, Min(0f)] private float _moveSpeed = 4f;

        private IGameplayInputSource _inputSource;
        private float _horizontalInput;
        private bool _isSubscribed;

        public float MoveSpeed => _moveSpeed;

        public bool CanMove =>
            _gameModeController != null &&
            _gameModeController.CurrentMode == GameMode.Gameplay;

        public void Initialize(
            Rigidbody2D body,
            GameModeController gameModeController,
            IGameplayInputSource inputSource)
        {
            UnsubscribeFromModeChanges();
            _rigidbody = body;
            _gameModeController = gameModeController;
            _inputReader = inputSource as GameplayInputReader;
            _inputSource = inputSource;
            SubscribeToModeChanges();

            if (!CanMove)
            {
                StopHorizontalMovement();
            }
        }

        public void SetMoveSpeed(float moveSpeed)
        {
            _moveSpeed = Mathf.Max(0f, moveSpeed);
        }

        private void Awake()
        {
            _rigidbody ??= GetComponent<Rigidbody2D>();
            _inputSource ??= _inputReader;
        }

        private void OnEnable()
        {
            SubscribeToModeChanges();
        }

        private void OnDisable()
        {
            UnsubscribeFromModeChanges();
            _horizontalInput = 0f;
            StopHorizontalMovement();
        }

        private void Update()
        {
            _horizontalInput = CanMove && _inputSource != null
                ? Mathf.Clamp(_inputSource.Move.x, -1f, 1f)
                : 0f;
        }

        private void FixedUpdate()
        {
            if (_rigidbody == null)
            {
                return;
            }

            if (!CanMove)
            {
                StopHorizontalMovement();
                return;
            }

            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = _horizontalInput * _moveSpeed;
            _rigidbody.linearVelocity = velocity;
        }

        private void SubscribeToModeChanges()
        {
            if (_isSubscribed || _gameModeController == null)
            {
                return;
            }

            _gameModeController.ModeChanged += OnGameModeChanged;
            _isSubscribed = true;
        }

        private void UnsubscribeFromModeChanges()
        {
            if (!_isSubscribed || _gameModeController == null)
            {
                return;
            }

            _gameModeController.ModeChanged -= OnGameModeChanged;
            _isSubscribed = false;
        }

        private void OnGameModeChanged(GameModeChanged change)
        {
            if (change.CurrentMode != GameMode.Gameplay)
            {
                _horizontalInput = 0f;
                StopHorizontalMovement();
            }
        }

        private void StopHorizontalMovement()
        {
            if (_rigidbody == null)
            {
                return;
            }

            Vector2 velocity = _rigidbody.linearVelocity;
            velocity.x = 0f;
            _rigidbody.linearVelocity = velocity;
        }
    }
}
