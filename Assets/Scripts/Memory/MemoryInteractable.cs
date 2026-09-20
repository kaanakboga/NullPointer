using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Memory
{
    [AddComponentMenu("Null Pointer/Memory/Memory Interactable")]
    public sealed class MemoryInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private MemoryController _controller;
        [SerializeField] private MemoryData _memory;
        [SerializeField] private int _interactionPriority;
        [SerializeField] private string _interactionPrompt = "Anıya odaklan";

        public bool CanInteract => isActiveAndEnabled && _controller != null && _memory != null;

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt => _interactionPrompt;

        public void Configure(MemoryController controller, MemoryData memory)
        {
            _controller = controller;
            _memory = memory;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _controller.Play(_memory);
            }
        }
    }
}
