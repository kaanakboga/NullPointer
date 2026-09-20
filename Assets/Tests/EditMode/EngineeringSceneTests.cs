using System.Linq;
using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Interaction;
using NullPointer.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.Tests.EditMode
{
    public sealed class EngineeringSceneTests
    {
        private const string ScenePath = "Assets/Scenes/Test/SCN_Test_GameplayFoundation.unity";

        [Test]
        public void GameplayFoundationScene_ContainsRequiredEngineeringComponents()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Assert.That(sceneAsset, Is.Not.Null, $"Engineering scene is missing at {ScenePath}.");

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                GameModeController gameModes = FindInRoots<GameModeController>(roots);
                PlayerController player = FindInRoots<PlayerController>(roots);
                PlayerInteractionDetector interaction = FindInRoots<PlayerInteractionDetector>(roots);

                Assert.That(gameModes, Is.Not.Null);
                Assert.That(player, Is.Not.Null);
                Assert.That(player.GetComponent<Rigidbody2D>(), Is.Not.Null);
                Assert.That(interaction, Is.Not.Null);
                Assert.That(
                    roots.Any(root => root.name == "Engineering Interactable"),
                    Is.True,
                    "The engineering interaction target is missing.");
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T FindInRoots<T>(GameObject[] roots) where T : Component
        {
            foreach (GameObject root in roots)
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}
