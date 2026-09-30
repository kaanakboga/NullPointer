using System.Collections;
using NullPointer.Deduction;
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
        public IEnumerator CyberNoirPanelPresentation_RevealsStagedContentAndCleansUpWhenDisabled()
        {
            var root = new GameObject("Panel Presentation", typeof(RectTransform), typeof(CanvasGroup));
            var panel = new GameObject("Panel", typeof(RectTransform));
            var stageA = new GameObject("Stage A", typeof(RectTransform), typeof(CanvasGroup));
            var stageB = new GameObject("Stage B", typeof(RectTransform), typeof(CanvasGroup));
            panel.transform.SetParent(root.transform, false);
            stageA.transform.SetParent(panel.transform, false);
            stageB.transform.SetParent(panel.transform, false);
            try
            {
                UiTransition transition = root.AddComponent<UiTransition>();
                transition.Configure(panel.GetComponent<RectTransform>(), root.GetComponent<CanvasGroup>(),
                    true, true, false, new Vector2(0f, -18f), Vector3.one, 0.08f);
                CyberNoirPanelPresentation presentation = root.AddComponent<CyberNoirPanelPresentation>();
                presentation.Configure(transition,
                    new[] { stageA.GetComponent<CanvasGroup>(), stageB.GetComponent<CanvasGroup>() },
                    null, null, 0.01f, 1f);
                presentation.SetAccessibility(true, true);

                presentation.Reveal();
                yield return null;
                yield return null;

                Assert.That(stageA.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));
                Assert.That(stageB.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f));
                Assert.That(presentation.ReducedMotion, Is.True);
                Assert.That(presentation.ReducedEffects, Is.True);

                presentation.HideAnimated();
                float timeout = Time.realtimeSinceStartup + 1f;
                while (root.activeSelf && Time.realtimeSinceStartup < timeout)
                {
                    yield return null;
                }

                Assert.That(root.activeSelf, Is.False);
                Assert.That(presentation.IsPresenting, Is.False);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

        [UnityTest]
        public IEnumerator EvidenceConnectionView_ReusesPoolAndClearsWithoutResidualLinks()
        {
            var root = new GameObject("Connections", typeof(RectTransform));
            var cardA = new GameObject("Card A", typeof(RectTransform));
            var cardB = new GameObject("Card B", typeof(RectTransform));
            var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
            cardA.transform.SetParent(root.transform, false);
            cardB.transform.SetParent(root.transform, false);
            line.transform.SetParent(root.transform, false);
            cardA.GetComponent<RectTransform>().anchoredPosition = new Vector2(-100f, 0f);
            cardB.GetComponent<RectTransform>().anchoredPosition = new Vector2(100f, 60f);
            try
            {
                EvidenceConnectionView connections = root.AddComponent<EvidenceConnectionView>();
                connections.Configure(root.GetComponent<RectTransform>(), new[] { line.GetComponent<Image>() });
                connections.Render(new[] { cardA.GetComponent<RectTransform>(), cardB.GetComponent<RectTransform>() });

                Assert.That(connections.ActiveLineCount, Is.EqualTo(1));
                Assert.That(line.activeSelf, Is.True);
                Assert.That(line.GetComponent<RectTransform>().rect.width, Is.GreaterThan(100f));

                connections.Pulse(false);
                yield return null;
                connections.Clear();

                Assert.That(connections.ActiveLineCount, Is.Zero);
                Assert.That(line.activeSelf, Is.False);
            }
            finally
            {
                Object.Destroy(root);
            }
        }

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

        [UnityTest]
        public IEnumerator EnvironmentPresentation_AnimatesPooledRainAndHonorsAccessibility()
        {
            var root = new GameObject("Environment");
            var player = new GameObject("Player");
            var layer = new GameObject("Far Layer");
            var rain = new GameObject("Rain");
            var atmosphere = new GameObject("Atmosphere", typeof(SpriteRenderer));
            player.transform.SetParent(root.transform);
            layer.transform.SetParent(root.transform);
            rain.transform.SetParent(root.transform);
            atmosphere.transform.SetParent(root.transform);
            atmosphere.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.2f);
            rain.transform.localPosition = new Vector3(1f, 3f, 0f);
            try
            {
                EnvironmentPresentationController presentation = root.AddComponent<EnvironmentPresentationController>();
                presentation.Configure(null, player.transform, new[] { layer.transform }, new[] { 0.1f },
                    new[] { rain.transform }, new[] { atmosphere.GetComponent<SpriteRenderer>() },
                    System.Array.Empty<SpriteRenderer>(), 1, 4f, -4f);

                Vector3 rainStart = rain.transform.localPosition;
                yield return null;
                yield return null;
                Assert.That(rain.transform.localPosition, Is.Not.EqualTo(rainStart));

                presentation.ApplyAccessibility(true, true);
                player.transform.position = Vector3.right * 5f;
                presentation.SetPreviewTime(2f);
                Assert.That(layer.transform.localPosition.x, Is.EqualTo(0f).Within(0.001f));
                Assert.That(presentation.ReducedMotion, Is.True);
                Assert.That(presentation.ReducedEffects, Is.True);
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
