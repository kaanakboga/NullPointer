using System.Collections;
using NullPointer.Visual;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NullPointer.Menus;
using NullPointer.Settings;
using UnityEngine.UI;

namespace NullPointer.Tests.PlayMode
{
    public sealed class VisualMotionTests
    {
        [UnityTest]
        public IEnumerator UiTransition_UsesUnscaledTimeAndRecoversFromDirectionChange()
        {
            float previousTimeScale = Time.timeScale;
            var target = new GameObject("Transition", typeof(RectTransform), typeof(CanvasGroup));
            try
            {
                RectTransform rect = target.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(40f, 20f);
                CanvasGroup group = target.GetComponent<CanvasGroup>();
                var transition = target.AddComponent<UiTransition>();
                transition.Configure(
                    rect,
                    group,
                    true,
                    true,
                    true,
                    new Vector2(0f, -30f),
                    new Vector3(0.98f, 0.98f, 1f),
                    0.08f);

                Time.timeScale = 0f;
                transition.PlayExit();
                yield return null;
                yield return null;
                Assert.That(group.alpha, Is.LessThan(1f));

                transition.PlayEntrance();
                float timeout = Time.realtimeSinceStartup + 1f;
                while (transition.IsAnimating && Time.realtimeSinceStartup < timeout)
                {
                    yield return null;
                }

                Assert.That(transition.IsAnimating, Is.False);
                Assert.That(target.activeSelf, Is.True);
                Assert.That(group.alpha, Is.EqualTo(1f).Within(0.001f));
                Assert.That(Vector2.Distance(rect.anchoredPosition, new Vector2(40f, 20f)), Is.LessThan(0.001f));
                Assert.That(group.interactable, Is.True);
                Assert.That(group.blocksRaycasts, Is.True);
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                Object.Destroy(target);
            }
        }

        [UnityTest]
        public IEnumerator MainMenuPresentation_AcceleratesRevealAndDispatchesDepartureOnce()
        {
            var root = new GameObject("Presentation");
            try
            {
                UiTransition title = CreateTransition(root.transform, "Title");
                UiTransition options = CreateTransition(root.transform, "Options");
                GameObject curtainObject = new("Curtain", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                curtainObject.transform.SetParent(root.transform, false);
                CanvasGroup curtain = curtainObject.GetComponent<CanvasGroup>();
                MainMenuPresentation presentation = root.AddComponent<MainMenuPresentation>();
                presentation.Configure(title, options, curtain, null);
                var storage = new TestSettingsStorage();
                var settings = new SettingsManager(storage);
                settings.Update(new GameSettings { ReducedUiMotion = true, ReducedVisualFx = true }, false);
                presentation.Initialize(settings);

                presentation.AccelerateReveal();
                Assert.That(presentation.RevealComplete, Is.True);
                int dispatches = 0;
                Assert.That(presentation.BeginDeparture(false, () => dispatches++), Is.True);
                Assert.That(presentation.BeginDeparture(false, () => dispatches++), Is.False);
                float timeout = Time.realtimeSinceStartup + 1f;
                while (presentation.IsDeparting && Time.realtimeSinceStartup < timeout)
                {
                    yield return null;
                }

                Assert.That(dispatches, Is.EqualTo(1));
                Assert.That(curtain.alpha, Is.EqualTo(1f).Within(0.001f));
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        private static UiTransition CreateTransition(Transform parent, string name)
        {
            var target = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
            target.transform.SetParent(parent, false);
            var transition = target.AddComponent<UiTransition>();
            transition.Configure(target.GetComponent<RectTransform>(), target.GetComponent<CanvasGroup>(), true,
                true, false, new Vector2(0f, -20f), Vector3.one, 0.08f);
            return transition;
        }

        private sealed class TestSettingsStorage : ISettingsStorage
        {
            private string _contents;
            public bool Exists => _contents != null;
            public string Read() => _contents;
            public void Write(string contents) => _contents = contents;
        }
    }
}
