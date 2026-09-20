using UnityEngine;

namespace NullPointer.Interaction
{
    public interface IInteractable
    {
        bool CanInteract { get; }

        int InteractionPriority { get; }

        Transform InteractionTransform { get; }

        void Interact();
    }
}
