using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/UI Focus Pulse")]
    public sealed class UiFocusPulse : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private VisualTheme _theme;
        [SerializeField] private Graphic _target;
        [SerializeField, Range(0f, 0.25f)] private float _alphaAmplitude = 0.08f;
        [SerializeField, Range(0f, 0.08f)] private float _scaleAmplitude = 0.012f;

        private bool _selected;
        private bool _hovered;
        private Color _baseColor;
        private Vector3 _baseScale;

        public void Configure(VisualTheme theme, Graphic target)
        {
            _theme = theme;
            _target = target;
            CacheBaseState();
        }

        public void OnSelect(BaseEventData eventData) => _selected = true;
        public void OnDeselect(BaseEventData eventData) => _selected = false;
        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData) => _hovered = false;

        private void Awake()
        {
            CacheBaseState();
        }

        private void Update()
        {
            bool active = _selected || _hovered;
            float period = _theme != null ? _theme.FocusPulsePeriod : 1.2f;
            float wave = active
                ? (Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f / Mathf.Max(0.1f, period)) + 1f) * 0.5f
                : 0f;
            transform.localScale = _baseScale * (1f + wave * _scaleAmplitude);
            if (_target != null)
            {
                Color color = _baseColor;
                color.a = Mathf.Clamp01(_baseColor.a + wave * _alphaAmplitude);
                _target.color = color;
            }
        }

        private void CacheBaseState()
        {
            _target ??= GetComponent<Graphic>();
            _baseScale = transform.localScale;
            _baseColor = _target != null ? _target.color : Color.white;
        }

        private void OnDisable()
        {
            transform.localScale = _baseScale;
            if (_target != null)
            {
                _target.color = _baseColor;
            }
        }
    }
}
