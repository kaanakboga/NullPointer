using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Terminal
{
    [AddComponentMenu("Null Pointer/Terminal/Terminal Interactable")]
    public sealed class TerminalInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private TerminalController _controller;
        [SerializeField] private TerminalData _terminal;
        [SerializeField] private int _interactionPriority = 5;
        [SerializeField] private string _interactionPrompt = "Terminali aç";

        public bool CanInteract => isActiveAndEnabled && _controller != null && _terminal != null;

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt => _interactionPrompt;

        public void Configure(TerminalController controller, TerminalData terminal)
        {
            _controller = controller;
            _terminal = terminal;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _controller.Open(_terminal);
            }
        }
    }
}
