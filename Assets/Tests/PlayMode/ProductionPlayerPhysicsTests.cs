using System.Collections;
using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NullPointer.Tests.PlayMode
{
    public sealed class ProductionPlayerPhysicsTests
    {
        private static readonly string[] ProductionGameplayScenes =
        {
            "SCN_ErenApartment",
            "SCN_MertApartment"
        };

        [UnityTest]
        public IEnumerator ProductionPlayers_RemainOnHorizontalPlaneAndCanMove()
        {
            foreach (string sceneName in ProductionGameplayScenes)
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
                while (!load.isDone)
                {
                    yield return null;
                }

                PlayerController player = Object.FindAnyObjectByType<PlayerController>();
                Assert.That(player, Is.Not.Null, $"{sceneName} has no PlayerController.");
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                Collider2D collider = player.GetComponent<Collider2D>();
                Assert.That(body, Is.Not.Null, $"{sceneName} player has no Rigidbody2D.");
                Assert.That(collider, Is.Not.Null, $"{sceneName} player has no Collider2D.");

                var modeObject = new GameObject($"{sceneName} Test GameMode");
                GameModeController modes = modeObject.AddComponent<GameModeController>();
                var input = new FakeGameplayInputSource();
                player.Initialize(body, modes, input);
                Vector2 start = body.position;

                for (int step = 0; step < 30; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(
                    body.position.y,
                    Is.EqualTo(start.y).Within(0.001f),
                    $"{sceneName} player drifted vertically while idle.");

                input.Move = Vector2.right;
                yield return null;
                for (int step = 0; step < 10; step++)
                {
                    yield return new WaitForFixedUpdate();
                }

                Assert.That(
                    body.position.x,
                    Is.GreaterThan(start.x + 0.1f),
                    $"{sceneName} player did not move horizontally.");
                Assert.That(
                    body.position.y,
                    Is.EqualTo(start.y).Within(0.001f),
                    $"{sceneName} player fell out of its horizontal movement plane.");

                Object.Destroy(modeObject);
                yield return null;
            }
        }
    }
}
