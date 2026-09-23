using System.Collections;
using NullPointer.Visual;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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
    }
}
