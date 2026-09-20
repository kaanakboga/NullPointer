using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Tests.PlayMode
{
    public sealed class PlayerInteractionDetectorTests
    {
        [Test]
        public void Detector_SelectsClosestTargetAndBlocksInteractionOutsideGameplay()
        {
            var modeObject = new GameObject("GameMode");
            var playerObject = new GameObject("Player");
            var fartherObject = CreateTarget("Farther", new Vector2(2f, 0f));
            var nearerObject = CreateTarget("Nearer", new Vector2(1f, 0f));

            try
            {
                GameModeController modes = modeObject.AddComponent<GameModeController>();
                var input = new FakeGameplayInputSource();
                PlayerInteractionDetector detector = playerObject.AddComponent<PlayerInteractionDetector>();
                detector.Initialize(modes, input, playerObject.transform);
                detector.SetDetection(3f, ~0);

                Physics2D.SyncTransforms();
                detector.RefreshTargets();

                var nearer = nearerObject.GetComponent<TestInteractable>();
                var farther = fartherObject.GetComponent<TestInteractable>();
                Assert.That(detector.ActiveTarget, Is.SameAs(nearer));
                input.PressInteract();
                Assert.That(nearer.InteractionCount, Is.EqualTo(1));
                Assert.That(farther.InteractionCount, Is.Zero);

                modes.SetMode(GameMode.Terminal);

                Assert.That(detector.ActiveTarget, Is.Null);
                input.PressInteract();
                Assert.That(nearer.InteractionCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(nearerObject);
                Object.DestroyImmediate(fartherObject);
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(modeObject);
            }
        }

        private static GameObject CreateTarget(string name, Vector2 position)
        {
            var target = new GameObject(name);
            target.transform.position = position;
            var collider = target.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            target.AddComponent<TestInteractable>();
            return target;
        }

        private sealed class TestInteractable : MonoBehaviour, IInteractable
        {
            public bool CanInteract => true;

            public int InteractionPriority => 0;

            public Transform InteractionTransform => transform;

            public int InteractionCount { get; private set; }

            public void Interact()
            {
                InteractionCount++;
            }
        }
    }
}
