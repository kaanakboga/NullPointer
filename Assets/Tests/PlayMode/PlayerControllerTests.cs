using System.Collections;
using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Player;
using UnityEngine;
using UnityEngine.TestTools;

namespace NullPointer.Tests.PlayMode
{
    public sealed class PlayerControllerTests
    {
        [UnityTest]
        public IEnumerator Movement_IsAppliedOnlyInGameplayAndStopsImmediatelyWhenBlocked()
        {
            var modeObject = new GameObject("GameMode");
            var playerObject = new GameObject("Player");

            try
            {
                GameModeController modes = modeObject.AddComponent<GameModeController>();
                Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                var input = new FakeGameplayInputSource { Move = Vector2.right };
                PlayerController controller = playerObject.AddComponent<PlayerController>();
                controller.SetMoveSpeed(5f);
                controller.Initialize(body, modes, input);

                yield return null;
                yield return new WaitForFixedUpdate();

                Assert.That(body.linearVelocity.x, Is.EqualTo(5f).Within(0.01f));

                modes.SetMode(GameMode.Dialogue);

                Assert.That(body.linearVelocity.x, Is.Zero.Within(0.001f));

                yield return new WaitForFixedUpdate();
                Assert.That(body.linearVelocity.x, Is.Zero.Within(0.001f));
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(modeObject);
            }
        }
    }
}
