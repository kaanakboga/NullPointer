using System.Collections;
using NullPointer.Core;
using NullPointer.Evidence;
using NullPointer.Inspect;
using NullPointer.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace NullPointer.Tests.PlayMode
{
    public sealed class ModalGameplayIntegrationTests
    {
        [TestCase(GameMode.Dialogue)]
        [TestCase(GameMode.Terminal)]
        [TestCase(GameMode.Memory)]
        [TestCase(GameMode.EvidenceBoard)]
        public void BlockingMode_ImmediatelyStopsPlayerHorizontalMovement(GameMode blockingMode)
        {
            var modesObject = new GameObject("Modes");
            var playerObject = new GameObject("Player");

            try
            {
                GameModeController modes = modesObject.AddComponent<GameModeController>();
                Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                PlayerController player = playerObject.AddComponent<PlayerController>();
                player.Initialize(body, modes, new FakeGameplayInputSource());
                body.linearVelocity = new Vector2(4f, 0f);

                modes.SetMode(blockingMode);

                Assert.That(body.linearVelocity.x, Is.Zero.Within(0.001f));
                Assert.That(player.CanMove, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(playerObject);
                Object.DestroyImmediate(modesObject);
            }
        }

        [UnityTest]
        public IEnumerator InspectOpenAndCancel_TransitionsModeAndKeepsMovementBlocked()
        {
            var root = new GameObject("Inspect Test Root");
            InspectData inspection = ScriptableObject.CreateInstance<InspectData>();

            try
            {
                GameModeController modes = root.AddComponent<GameModeController>();
                var input = new FakeGameplayInputSource();
                var state = new GameState();
                var evidence = new EvidenceService(state, System.Array.Empty<EvidenceData>());

                var playerObject = new GameObject("Player");
                playerObject.transform.SetParent(root.transform);
                Rigidbody2D body = playerObject.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                PlayerController player = playerObject.AddComponent<PlayerController>();
                player.Initialize(body, modes, input);
                body.linearVelocity = new Vector2(3f, 0f);

                var panelObject = new GameObject("Panel");
                panelObject.transform.SetParent(root.transform);
                InspectPanel panel = panelObject.AddComponent<InspectPanel>();
                Text title = CreateText(panelObject.transform, "Title");
                Text description = CreateText(panelObject.transform, "Description");
                Text status = CreateText(panelObject.transform, "Status");
                Button collect = CreateButton(panelObject.transform, "Collect");
                Button close = CreateButton(panelObject.transform, "Close");
                panel.Configure(panelObject, title, description, status, collect, close);

                var controllerObject = new GameObject("Controller");
                controllerObject.transform.SetParent(root.transform);
                InspectController controller = controllerObject.AddComponent<InspectController>();
                controller.Configure(panel);
                controller.Initialize(modes, input, evidence);

                controller.Open(inspection);

                Assert.That(modes.CurrentMode, Is.EqualTo(GameMode.Inspect));
                Assert.That(body.linearVelocity.x, Is.Zero.Within(0.001f));

                input.PressPause();
                yield return null;

                Assert.That(modes.CurrentMode, Is.EqualTo(GameMode.Gameplay));
                Assert.That(panelObject.activeSelf, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(inspection);
                Object.DestroyImmediate(root);
            }
        }

        private static Text CreateText(Transform parent, string name)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent);
            return textObject.AddComponent<Text>();
        }

        private static Button CreateButton(Transform parent, string name)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent);
            return buttonObject.AddComponent<Button>();
        }
    }
}
