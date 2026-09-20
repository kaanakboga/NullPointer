using System.Collections.Generic;
using UnityEngine;

namespace NullPointer.Interaction
{
    public static class InteractionTargetSelector
    {
        private const float DistanceEpsilon = 0.0001f;

        public static IInteractable Select(Vector2 origin, IReadOnlyList<InteractionCandidate> candidates)
        {
            IInteractable selected = null;
            int selectedPriority = int.MinValue;
            float selectedDistance = float.PositiveInfinity;
            int selectedStableId = int.MaxValue;

            if (candidates == null)
            {
                return null;
            }

            for (int index = 0; index < candidates.Count; index++)
            {
                InteractionCandidate candidate = candidates[index];
                IInteractable interactable = candidate.Interactable;

                if (interactable == null || !interactable.CanInteract || interactable.InteractionTransform == null)
                {
                    continue;
                }

                int priority = interactable.InteractionPriority;
                float distance = (candidate.Position - origin).sqrMagnitude;

                if (priority > selectedPriority ||
                    (priority == selectedPriority && distance < selectedDistance - DistanceEpsilon) ||
                    (priority == selectedPriority &&
                     Mathf.Abs(distance - selectedDistance) <= DistanceEpsilon &&
                     candidate.StableId < selectedStableId))
                {
                    selected = interactable;
                    selectedPriority = priority;
                    selectedDistance = distance;
                    selectedStableId = candidate.StableId;
                }
            }

            return selected;
        }
    }
}
