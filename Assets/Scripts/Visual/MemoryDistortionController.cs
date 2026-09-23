using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Memory Distortion Controller")]
    public sealed class MemoryDistortionController : MonoBehaviour
    {
        [SerializeField] private GameObject _fxRoot;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Image _overlay;
        [SerializeField] private Image _staticLayer;
        [SerializeField] private Image _vignetteLayer;
        [SerializeField] private Image _flashLayer;
        [SerializeField] private RectTransform _tearBand;
        [SerializeField] private RectTransform _chromaticLeft;
        [SerializeField] private RectTransform _chromaticRight;

        private Coroutine _routine;
        private Vector2 _tearRestingPosition;
        private Vector2 _leftRestingPosition;
        private Vector2 _rightRestingPosition;

        public bool IsActive => _routine != null;

        public void Configure(
            GameObject fxRoot,
            CanvasGroup canvasGroup,
            Image overlay,
            Image staticLayer,
            Image vignetteLayer,
            Image flashLayer,
            RectTransform tearBand,
            RectTransform chromaticLeft,
            RectTransform chromaticRight)
        {
            _fxRoot = fxRoot;
            _canvasGroup = canvasGroup;
            _overlay = overlay;
            _staticLayer = staticLayer;
            _vignetteLayer = vignetteLayer;
            _flashLayer = flashLayer;
            _tearBand = tearBand;
            _chromaticLeft = chromaticLeft;
            _chromaticRight = chromaticRight;
            CacheRestingPositions();
            StopImmediate();
        }

        public bool Play(MemoryDistortionProfile profile, float durationOverride = -1f)
        {
            if (profile == null)
            {
                return false;
            }

            StopActiveRoutine();
            CacheRestingPositions();
            _fxRoot?.SetActive(true);
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            float duration = durationOverride > 0f ? durationOverride : profile.Duration;
            _routine = StartCoroutine(Animate(profile, Mathf.Max(0.05f, duration)));
            return true;
        }

        public void StopImmediate()
        {
            StopActiveRoutine();
            ResetVisuals();
            _fxRoot?.SetActive(false);
        }

        private IEnumerator Animate(MemoryDistortionProfile profile, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float envelope = Mathf.Sin(normalized * Mathf.PI) * profile.Intensity;
                float authoredBeat = Mathf.Sin(elapsed * 17f) * 0.55f + Mathf.Sin(elapsed * 43f) * 0.45f;

                SetAlpha(_overlay, envelope * profile.OverlayOpacity);
                SetAlpha(_staticLayer, envelope * profile.StaticOpacity * (0.55f + Mathf.Abs(authoredBeat) * 0.45f));
                SetAlpha(_vignetteLayer, envelope * profile.VignetteShift);
                SetAlpha(_flashLayer, normalized < 0.08f || normalized is > 0.52f and < 0.57f
                    ? envelope * profile.FlashOpacity
                    : 0f);

                if (_tearBand != null)
                {
                    _tearBand.anchoredPosition = _tearRestingPosition + new Vector2(
                        authoredBeat * profile.HorizontalTearing * envelope,
                        Mathf.Sin(elapsed * 7f) * 120f);
                }

                float chromatic = authoredBeat * profile.ChromaticOffset * envelope;
                if (_chromaticLeft != null)
                {
                    _chromaticLeft.anchoredPosition = _leftRestingPosition + Vector2.left * chromatic;
                }

                if (_chromaticRight != null)
                {
                    _chromaticRight.anchoredPosition = _rightRestingPosition + Vector2.right * chromatic;
                }

                yield return null;
            }

            _routine = null;
            ResetVisuals();
            _fxRoot?.SetActive(false);
        }

        private void CacheRestingPositions()
        {
            _tearRestingPosition = _tearBand != null ? _tearBand.anchoredPosition : Vector2.zero;
            _leftRestingPosition = _chromaticLeft != null ? _chromaticLeft.anchoredPosition : Vector2.zero;
            _rightRestingPosition = _chromaticRight != null ? _chromaticRight.anchoredPosition : Vector2.zero;
        }

        private void ResetVisuals()
        {
            SetAlpha(_overlay, 0f);
            SetAlpha(_staticLayer, 0f);
            SetAlpha(_vignetteLayer, 0f);
            SetAlpha(_flashLayer, 0f);
            if (_tearBand != null)
            {
                _tearBand.anchoredPosition = _tearRestingPosition;
            }

            if (_chromaticLeft != null)
            {
                _chromaticLeft.anchoredPosition = _leftRestingPosition;
            }

            if (_chromaticRight != null)
            {
                _chromaticRight.anchoredPosition = _rightRestingPosition;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = graphic.color;
            color.a = Mathf.Clamp01(alpha);
            graphic.color = color;
        }

        private void StopActiveRoutine()
        {
            if (_routine == null)
            {
                return;
            }

            StopCoroutine(_routine);
            _routine = null;
        }

        private void OnDisable()
        {
            StopImmediate();
        }
    }
}
