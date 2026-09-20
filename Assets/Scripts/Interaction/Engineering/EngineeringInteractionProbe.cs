#if UNITY_EDITOR
using UnityEngine;

namespace NullPointer.Interaction.Engineering
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Engineering/Interaction Probe")]
    public sealed class EngineeringInteractionProbe : MonoBehaviour, IInteractable
    {
        [SerializeField] private bool _isAvailable = true;
        [SerializeField] private int _interactionPriority;
        [SerializeField] private SpriteRenderer _indicator;
        [SerializeField] private TextMesh _statusLabel;

        public bool CanInteract => enabled && _isAvailable;

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public int InteractionCount { get; private set; }

        public void Configure(SpriteRenderer indicator, TextMesh statusLabel)
        {
            _indicator = indicator;
            _statusLabel = statusLabel;
        }

        public void Interact()
        {
            if (!CanInteract)
            {
                return;
            }

            InteractionCount++;

            if (_indicator != null)
            {
                _indicator.color = new Color(0.35f, 0.9f, 0.7f, 1f);
            }

            if (_statusLabel != null)
            {
                _statusLabel.text = $"Interaction executed ({InteractionCount})";
            }
        }
    }
}
#endif
