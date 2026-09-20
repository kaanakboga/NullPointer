using System.Collections;
using NullPointer.Player;
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
            GameApplication application = Object.FindFirstObjectByType<GameApplication>();
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
        public IEnumerator Bootstrap_LoadsAndInstallsErenApartment()
        {
            AsyncOperation bootstrapLoad = SceneManager.LoadSceneAsync("SCN_Bootstrap", LoadSceneMode.Single);
            while (!bootstrapLoad.isDone)
            {
                yield return null;
            }

            float deadline = Time.realtimeSinceStartup + 10f;
            while (SceneManager.GetActiveScene().name != "SCN_ErenApartment" &&
                   Time.realtimeSinceStartup < deadline)
            {
                yield return null;
            }

            GameApplication application = Object.FindFirstObjectByType<GameApplication>();
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();

            Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("SCN_ErenApartment"));
            Assert.That(application, Is.Not.Null);
            Assert.That(player, Is.Not.Null);
            Assert.That(application.GameState.LocationId, Is.EqualTo("location.eren.apartment"));
            Assert.That(player.CanMove, Is.True);

        }
    }
}
