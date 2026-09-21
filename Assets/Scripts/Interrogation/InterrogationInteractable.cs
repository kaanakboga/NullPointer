using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Interrogation
{
    [AddComponentMenu("Null Pointer/Interrogation/Interrogation Interactable")]
    public sealed class InterrogationInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private InterrogationController _controller;
        [SerializeField] private InterrogationClaimData _claim;
        [SerializeField] private int _priority = 10;

        public bool CanInteract => isActiveAndEnabled && _controller != null && _claim != null;
        public int InteractionPriority => _priority;
        public Transform InteractionTransform => transform;
        public string InteractionPrompt => "İfadeyi sorgula";

        public void Configure(InterrogationController controller, InterrogationClaimData claim)
        {
            _controller = controller;
            _claim = claim;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _controller.Open(_claim);
            }
        }
    }
}
