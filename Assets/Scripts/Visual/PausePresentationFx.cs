using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Pause Presentation FX")]
    public sealed class PausePresentationFx : MonoBehaviour
    {
        [SerializeField] private RectTransform _scanLine;
        [SerializeField] private CanvasGroup _edgeSignal;
        [SerializeField] private float _minimumX = 52f;
        [SerializeField] private float _maximumX = 696f;
        private bool _reducedFx;

        public void Configure(RectTransform scanLine, CanvasGroup edgeSignal, float minimumX, float maximumX)
        {
            _scanLine = scanLine;
            _edgeSignal = edgeSignal;
            _minimumX = minimumX;
            _maximumX = maximumX;
        }

        public void SetReducedFx(bool reducedFx)
        {
            _reducedFx = reducedFx;
            if (_scanLine != null)
            {
                _scanLine.gameObject.SetActive(!reducedFx);
            }

            if (_edgeSignal != null)
            {
                _edgeSignal.alpha = reducedFx ? 0.18f : 0.42f;
            }
        }

        private void Update()
        {
            if (_scanLine == null || _reducedFx)
            {
                return;
            }

            float phase = Mathf.PingPong(Time.unscaledTime * 0.16f, 1f);
            Vector2 position = _scanLine.anchoredPosition;
            position.x = Mathf.Lerp(_minimumX, _maximumX, phase * phase * (3f - 2f * phase));
            _scanLine.anchoredPosition = position;
            ImageAlpha(0.04f + Mathf.Sin(Time.unscaledTime * 0.9f) * 0.012f);
        }

        private void ImageAlpha(float alpha)
        {
            UnityEngine.UI.Image image = _scanLine.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                return;
            }

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
