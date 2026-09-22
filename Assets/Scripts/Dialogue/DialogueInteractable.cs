using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Dialogue
{
    [AddComponentMenu("Null Pointer/Dialogue/Dialogue Interactable")]
    public sealed class DialogueInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private DialogueController _controller;
        [SerializeField] private DialogueData _dialogue;
        [SerializeField] private int _interactionPriority;
        [SerializeField] private string _interactionPrompt = "Dinle";

        public bool CanInteract => isActiveAndEnabled && _controller != null && _dialogue != null;

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt => _interactionPrompt;

        public DialogueData Dialogue => _dialogue;

        public void Configure(DialogueController controller, DialogueData dialogue)
        {
            _controller = controller;
            _dialogue = dialogue;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _controller.Open(_dialogue);
            }
        }
    }
}
