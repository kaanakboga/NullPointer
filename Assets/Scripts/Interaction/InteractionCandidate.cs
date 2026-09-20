using UnityEngine;

namespace NullPointer.Interaction
{
    public readonly struct InteractionCandidate
    {
        public InteractionCandidate(IInteractable interactable, Vector2 position, int stableId)
        {
            Interactable = interactable;
            Position = position;
            StableId = stableId;
        }

        public IInteractable Interactable { get; }

        public Vector2 Position { get; }

        public int StableId { get; }
    }
}
