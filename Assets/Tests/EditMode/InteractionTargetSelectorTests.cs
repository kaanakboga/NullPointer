using NUnit.Framework;
using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class InteractionTargetSelectorTests
    {
        [Test]
        public void Select_PrefersHigherPriorityBeforeDistance()
        {
            var nearObject = new GameObject("Near");
            var priorityObject = new GameObject("Priority");

            try
            {
                nearObject.transform.position = Vector2.right;
                priorityObject.transform.position = Vector2.right * 2f;
                var near = new StubInteractable(nearObject.transform, true, 0);
                var priority = new StubInteractable(priorityObject.transform, true, 10);
                var candidates = new[]
                {
                    new InteractionCandidate(near, nearObject.transform.position, 1),
                    new InteractionCandidate(priority, priorityObject.transform.position, 2)
                };

                IInteractable selected = InteractionTargetSelector.Select(Vector2.zero, candidates);

                Assert.That(selected, Is.SameAs(priority));
            }
            finally
            {
                Object.DestroyImmediate(nearObject);
                Object.DestroyImmediate(priorityObject);
            }
        }

        [Test]
        public void Select_WhenCandidatesTie_UsesStableIdRegardlessOfInputOrder()
        {
            var leftObject = new GameObject("Left");
            var rightObject = new GameObject("Right");

            try
            {
                leftObject.transform.position = Vector2.left;
                rightObject.transform.position = Vector2.right;
                var left = new StubInteractable(leftObject.transform, true, 0);
                var right = new StubInteractable(rightObject.transform, true, 0);
                var candidates = new[]
                {
                    new InteractionCandidate(right, rightObject.transform.position, 20),
                    new InteractionCandidate(left, leftObject.transform.position, 10)
                };

                IInteractable selected = InteractionTargetSelector.Select(Vector2.zero, candidates);

                Assert.That(selected, Is.SameAs(left));
            }
            finally
            {
                Object.DestroyImmediate(leftObject);
                Object.DestroyImmediate(rightObject);
            }
        }

        [Test]
        public void Select_IgnoresUnavailableCandidate()
        {
            var unavailableObject = new GameObject("Unavailable");
            var availableObject = new GameObject("Available");

            try
            {
                unavailableObject.transform.position = Vector2.right * 0.5f;
                availableObject.transform.position = Vector2.right;
                var unavailable = new StubInteractable(unavailableObject.transform, false, 0);
                var available = new StubInteractable(availableObject.transform, true, 0);
                var candidates = new[]
                {
                    new InteractionCandidate(unavailable, unavailableObject.transform.position, 1),
                    new InteractionCandidate(available, availableObject.transform.position, 2)
                };

                IInteractable selected = InteractionTargetSelector.Select(Vector2.zero, candidates);

                Assert.That(selected, Is.SameAs(available));
            }
            finally
            {
                Object.DestroyImmediate(unavailableObject);
                Object.DestroyImmediate(availableObject);
            }
        }

        private sealed class StubInteractable : IInteractable
        {
            public StubInteractable(Transform transform, bool canInteract, int interactionPriority)
            {
                InteractionTransform = transform;
                CanInteract = canInteract;
                InteractionPriority = interactionPriority;
            }

            public bool CanInteract { get; }

            public int InteractionPriority { get; }

            public Transform InteractionTransform { get; }

            public void Interact()
            {
            }
        }
    }
}
