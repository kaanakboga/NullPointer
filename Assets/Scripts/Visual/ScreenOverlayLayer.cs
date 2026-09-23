using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Screen Overlay Layer")]
    public sealed class ScreenOverlayLayer : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField, Range(0f, 1f)] private float _maximumOpacity = 0.1f;
        [SerializeField] private bool _visibleByDefault;

        public bool IsVisible => _canvasGroup != null && _canvasGroup.alpha > 0.001f;

        public void Configure(CanvasGroup canvasGroup, float maximumOpacity, bool visibleByDefault = false)
        {
            _canvasGroup = canvasGroup;
            _maximumOpacity = Mathf.Clamp01(maximumOpacity);
            _visibleByDefault = visibleByDefault;
            SetIntensity(visibleByDefault ? 1f : 0f);
        }

        public void SetIntensity(float intensity)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            _canvasGroup.alpha = Mathf.Clamp01(intensity) * _maximumOpacity;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void Awake()
        {
            SetIntensity(_visibleByDefault ? 1f : 0f);
        }
    }
}
