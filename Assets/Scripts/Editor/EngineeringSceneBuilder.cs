using NullPointer.Core;
using NullPointer.Core.Engineering;
using NullPointer.Input;
using NullPointer.Interaction;
using NullPointer.Interaction.Engineering;
using NullPointer.Player;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace NullPointer.Editor
{
    public static class EngineeringSceneBuilder
    {
        private const string ScenePath = "Assets/Scenes/Test/SCN_Test_GameplayFoundation.unity";
        private const string InputActionsPath = "Assets/Settings/InputSystem_Actions.inputactions";

        [MenuItem("Null Pointer/Engineering/Rebuild Gameplay Foundation Scene")]
        public static void CreateOrUpdate()
        {
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions == null)
            {
                throw new System.InvalidOperationException($"Input action asset not found at {InputActionsPath}.");
            }

            inputActions.FindActionMap(GameplayInputActionNames.Map, true);

            Scene previousActiveScene = SceneManager.GetActiveScene();
            bool canCreateAdditively =
                previousActiveScene.IsValid() &&
                previousActiveScene.isLoaded &&
                !string.IsNullOrEmpty(previousActiveScene.path);
            NewSceneMode creationMode = canCreateAdditively ? NewSceneMode.Additive : NewSceneMode.Single;
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, creationMode);
            SceneManager.SetActiveScene(scene);

            try
            {
                Sprite placeholderSprite = GetPlaceholderSprite();

                var systems = new GameObject("Gameplay Foundation");
                GameModeController gameModes = systems.AddComponent<GameModeController>();
                GameplayInputReader inputReader = systems.AddComponent<GameplayInputReader>();
                inputReader.Configure(inputActions);
                PauseInputHandler pauseHandler = systems.AddComponent<PauseInputHandler>();
                pauseHandler.Initialize(gameModes, inputReader);

                CreateModeDisplay(gameModes);
                CreateInstructions();
                CreateCamera();
                CreateFloor();
                CreatePlayer(gameModes, inputReader, placeholderSprite);
                CreateInteractable(placeholderSprite);

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, ScenePath))
                {
                    throw new System.InvalidOperationException($"Failed to save engineering scene at {ScenePath}.");
                }

                AssetDatabase.SaveAssets();
                Debug.Log($"[Engineering] Rebuilt {ScenePath}");
            }
            finally
            {
                if (canCreateAdditively)
                {
                    EditorSceneManager.CloseScene(scene, true);
                    SceneManager.SetActiveScene(previousActiveScene);
                }
            }
        }

        [DidReloadScripts]
        private static void EnsureEngineeringSceneExists()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
            {
                return;
            }

            EditorApplication.delayCall += CreateSceneIfStillMissing;
        }

        private static void CreateSceneIfStillMissing()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                CreateOrUpdate();
            }
        }

        private static void CreatePlayer(
            GameModeController gameModes,
            GameplayInputReader inputReader,
            Sprite placeholderSprite)
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(-3f, 0f, 0f);

            var renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = placeholderSprite;
            renderer.color = new Color(0.35f, 0.75f, 0.9f, 1f);
            player.transform.localScale = new Vector3(0.75f, 1.25f, 1f);

            var body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 3f;
            body.freezeRotation = true;
            player.AddComponent<BoxCollider2D>();

            PlayerController controller = player.AddComponent<PlayerController>();
            controller.SetMoveSpeed(4f);
            controller.Initialize(body, gameModes, inputReader);

            PlayerInteractionDetector detector = player.AddComponent<PlayerInteractionDetector>();
            detector.SetDetection(1.6f, ~0);
            detector.Initialize(gameModes, inputReader, player.transform);
        }

        private static void CreateInteractable(Sprite placeholderSprite)
        {
            var interactable = new GameObject("Engineering Interactable");
            interactable.transform.position = new Vector3(1.5f, -0.1f, 0f);
            interactable.transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            var renderer = interactable.AddComponent<SpriteRenderer>();
            renderer.sprite = placeholderSprite;
            renderer.color = new Color(0.85f, 0.45f, 0.25f, 1f);

            var collider = interactable.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;

            TextMesh status = CreateWorldText(
                "Interaction Status",
                "Press E / Gamepad South near the amber target",
                new Vector3(-1.2f, 1.2f, -0.1f),
                TextAnchor.MiddleCenter);
            status.transform.SetParent(interactable.transform, false);

            EngineeringInteractionProbe probe = interactable.AddComponent<EngineeringInteractionProbe>();
            probe.Configure(renderer, status);
        }

        private static void CreateFloor()
        {
            var floor = new GameObject("Floor");
            floor.transform.position = new Vector3(0f, -1f, 0f);
            var collider = floor.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(14f, 1f);
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0.6f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.08f, 1f);
        }

        private static void CreateModeDisplay(GameModeController gameModes)
        {
            TextMesh label = CreateWorldText(
                "Mode Display",
                "Mode: Gameplay",
                new Vector3(-5.7f, 3f, 0f),
                TextAnchor.UpperLeft);
            EngineeringModeDisplay display = label.gameObject.AddComponent<EngineeringModeDisplay>();
            display.Configure(gameModes, label);
        }

        private static void CreateInstructions()
        {
            CreateWorldText(
                "Instructions",
                "A/D or arrows: move    E: interact    Esc/Start: pause/resume",
                new Vector3(0f, -2.65f, 0f),
                TextAnchor.MiddleCenter);
        }

        private static TextMesh CreateWorldText(
            string objectName,
            string text,
            Vector3 position,
            TextAnchor anchor)
        {
            var textObject = new GameObject(objectName);
            textObject.transform.position = position;
            var textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = anchor;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 48;
            textMesh.characterSize = 0.055f;
            textMesh.color = new Color(0.72f, 0.9f, 0.9f, 1f);
            return textMesh;
        }

        private static Sprite GetPlaceholderSprite()
        {
            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (sprite == null)
            {
                throw new System.InvalidOperationException("Unity's built-in UI sprite could not be loaded.");
            }

            return sprite;
        }
    }
}
