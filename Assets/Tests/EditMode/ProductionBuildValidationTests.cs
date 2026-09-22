using System.Linq;
using NullPointer.Editor;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NullPointer.Tests.EditMode
{
    public sealed class ProductionBuildValidationTests
    {
        [Test]
        public void ProductionContent_CurrentChapterOneAssetsPassBuildGate()
        {
            Assert.That(ProductionContentValidator.Validate(), Is.Empty);
        }

        [TestCase(ProductionContentValidator.MainMenuScenePath)]
        [TestCase(ProductionContentValidator.ErenScenePath)]
        [TestCase(ProductionContentValidator.MertScenePath)]
        public void ProductionUiScene_HasSingleEventSystemListenerAndScalableCanvas(string scenePath)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            try
            {
                GameObject[] roots = scene.GetRootGameObjects();
                EventSystem[] eventSystems = roots
                    .SelectMany(root => root.GetComponentsInChildren<EventSystem>(true))
                    .ToArray();
                AudioListener[] listeners = roots
                    .SelectMany(root => root.GetComponentsInChildren<AudioListener>(true))
                    .ToArray();
                CanvasScaler[] scalers = roots
                    .SelectMany(root => root.GetComponentsInChildren<CanvasScaler>(true))
                    .ToArray();

                Assert.That(eventSystems, Has.Length.EqualTo(1));
                Assert.That(listeners, Has.Length.EqualTo(1));
                Assert.That(scalers, Has.Length.EqualTo(1));
                Assert.That(scalers[0].uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
                Assert.That(scalers[0].referenceResolution, Is.EqualTo(new Vector2(1920f, 1080f)));
                Assert.That(scalers[0].matchWidthOrHeight, Is.EqualTo(0.5f).Within(0.001f));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}
