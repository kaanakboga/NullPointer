using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Inspect
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Inspect/Inspect Interactable")]
    public sealed class InspectInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private InspectController _controller;
        [SerializeField] private InspectData _inspection;
        [SerializeField] private int _interactionPriority;
        [SerializeField] private string _interactionPrompt = "İncele";

        public bool CanInteract => isActiveAndEnabled && _controller != null && _inspection != null;

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt => _interactionPrompt;

        public void Configure(InspectController controller, InspectData inspection, int priority = 0)
        {
            _controller = controller;
            _inspection = inspection;
            _interactionPriority = priority;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _controller.Open(_inspection);
            }
        }
    }
}
