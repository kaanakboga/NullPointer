using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Deduction
{
    [AddComponentMenu("Null Pointer/Deduction/Evidence Board Interactable")]
    public sealed class EvidenceBoardInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private EvidenceBoardController _controller;
        [SerializeField] private int _interactionPriority;
        [SerializeField] private string _interactionPrompt = "Kanıt panosunu aç";

        public bool CanInteract => isActiveAndEnabled && _controller != null;

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt => _interactionPrompt;

        public void Configure(EvidenceBoardController controller)
        {
            _controller = controller;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _controller.Open();
            }
        }
    }
}
