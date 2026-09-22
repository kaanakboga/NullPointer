using System.Collections;
using NullPointer.Core;
using NullPointer.Menus;
using NullPointer.Runtime;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NullPointer.Tests.PlayMode
{
    public sealed class OpeningBootstrapSmokeTests
    {
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameApplication application = Object.FindAnyObjectByType<GameApplication>();
            if (application != null)
            {
                Object.Destroy(application.gameObject);
                yield return null;
            }

            Scene cleanupScene = SceneManager.CreateScene("Opening Bootstrap Smoke Cleanup");
            SceneManager.SetActiveScene(cleanupScene);
            for (int index = SceneManager.sceneCount - 1; index >= 0; index--)
            {
                Scene scene = SceneManager.GetSceneAt(index);
                if (scene == cleanupScene)
                {
                    continue;
                }

                AsyncOperation unload = SceneManager.UnloadSceneAsync(scene);
                if (unload != null)
                {
                    while (!unload.isDone)
                    {
                        yield return null;
                    }
                }
            }
        }

        [UnityTest]
        public IEnumerator Bootstrap_LoadsAndInstallsMainMenuWithoutGameplayInput()
        {
            AsyncOperation bootstrapLoad = SceneManager.LoadSceneAsync("SCN_Bootstrap", LoadSceneMode.Single);
            while (!bootstrapLoad.isDone)
            {
                yield return null;
            }

            float deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != "SCN_MainMenu" &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            GameApplication application = Object.FindAnyObjectByType<GameApplication>();
            MainMenuController menu = Object.FindAnyObjectByType<MainMenuController>();

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("SCN_MainMenu"));
            Assert.That(application, Is.Not.Null);
            Assert.That(menu, Is.Not.Null);
            Assert.That(application.GameState.LocationId, Is.EqualTo("location.system.main_menu"));
            Assert.That(application.GameModes.CurrentMode, Is.EqualTo(GameMode.Paused));

        }
    }
}
