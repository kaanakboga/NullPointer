using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NullPointer.Core;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Interaction
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Interaction/Player Interaction Detector")]
    public sealed class PlayerInteractionDetector : MonoBehaviour
    {
        private const int DetectionBufferSize = 32;

        [SerializeField] private GameModeController _gameModeController;
        [SerializeField] private GameplayInputReader _inputReader;
        [SerializeField] private Transform _detectionOrigin;
        [SerializeField, Min(0f)] private float _detectionRadius = 1.5f;
        [SerializeField] private LayerMask _interactionMask = ~0;

        private readonly Collider2D[] _overlapResults = new Collider2D[DetectionBufferSize];
        private readonly HashSet<IInteractable> _seenInteractables = new HashSet<IInteractable>();
        private readonly List<InteractionCandidate> _candidates = new List<InteractionCandidate>(DetectionBufferSize);

        private IGameplayInputSource _inputSource;
        private IInteractable _activeTarget;
        private bool _isModeSubscribed;
        private bool _isInputSubscribed;

        public event Action<IInteractable> ActiveTargetChanged;

        public IInteractable ActiveTarget => _activeTarget;

        public bool CanInteract =>
            _gameModeController != null &&
            _gameModeController.CurrentMode == GameMode.Gameplay &&
            _activeTarget != null &&
            _activeTarget.CanInteract;

        public void Initialize(
            GameModeController gameModeController,
            IGameplayInputSource inputSource,
            Transform detectionOrigin = null)
        {
            Unsubscribe();
            _gameModeController = gameModeController;
            _inputReader = inputSource as GameplayInputReader;
            _inputSource = inputSource;
            _detectionOrigin = detectionOrigin == null ? transform : detectionOrigin;
            Subscribe();
            RefreshTargets();
        }

        public void SetDetection(float radius, LayerMask layerMask)
        {
            _detectionRadius = Mathf.Max(0f, radius);
            _interactionMask = layerMask;
        }

        public void RefreshTargets()
        {
            if (_gameModeController == null || _gameModeController.CurrentMode != GameMode.Gameplay)
            {
                SetActiveTarget(null);
                return;
            }

            Vector2 origin = _detectionOrigin == null ? transform.position : _detectionOrigin.position;
            var filter = new ContactFilter2D();
            filter.SetLayerMask(_interactionMask);
            filter.useTriggers = true;

            int hitCount = Physics2D.OverlapCircle(origin, _detectionRadius, filter, _overlapResults);
            _seenInteractables.Clear();
            _candidates.Clear();

            for (int index = 0; index < hitCount; index++)
            {
                Collider2D hit = _overlapResults[index];
                if (hit == null)
                {
                    continue;
                }

                IInteractable interactable = hit.GetComponentInParent(typeof(IInteractable)) as IInteractable;
                if (interactable == null || !_seenInteractables.Add(interactable))
                {
                    continue;
                }

                Transform targetTransform = interactable.InteractionTransform;
                if (targetTransform == null)
                {
                    continue;
                }

                int stableId = RuntimeHelpers.GetHashCode(interactable);

                _candidates.Add(new InteractionCandidate(interactable, targetTransform.position, stableId));
            }

            SetActiveTarget(InteractionTargetSelector.Select(origin, _candidates));
        }

        public bool TryInteract()
        {
            if (!CanInteract)
            {
                return false;
            }

            _activeTarget.Interact();
            return true;
        }

        private void Awake()
        {
            _inputSource ??= _inputReader;
            _detectionOrigin ??= transform;
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
            SetActiveTarget(null);
        }

        private void Update()
        {
            RefreshTargets();
        }

        private void Subscribe()
        {
            if (!_isModeSubscribed && _gameModeController != null)
            {
                _gameModeController.ModeChanged += OnGameModeChanged;
                _isModeSubscribed = true;
            }

            if (!_isInputSubscribed && _inputSource != null)
            {
                _inputSource.InteractPressed += OnInteractPressed;
                _isInputSubscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (_isModeSubscribed && _gameModeController != null)
            {
                _gameModeController.ModeChanged -= OnGameModeChanged;
                _isModeSubscribed = false;
            }

            if (_isInputSubscribed && _inputSource != null)
            {
                _inputSource.InteractPressed -= OnInteractPressed;
                _isInputSubscribed = false;
            }
        }

        private void OnGameModeChanged(GameModeChanged change)
        {
            if (change.CurrentMode != GameMode.Gameplay)
            {
                SetActiveTarget(null);
            }
        }

        private void OnInteractPressed()
        {
            TryInteract();
        }

        private void SetActiveTarget(IInteractable target)
        {
            if (ReferenceEquals(_activeTarget, target))
            {
                return;
            }

            _activeTarget = target;
            ActiveTargetChanged?.Invoke(target);
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = _detectionOrigin == null ? transform : _detectionOrigin;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin.position, _detectionRadius);
        }
    }
}
