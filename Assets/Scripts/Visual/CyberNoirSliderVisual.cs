using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Slider))]
    [AddComponentMenu("Null Pointer/Visual/Cyber-Noir Slider Visual")]
    public sealed class CyberNoirSliderVisual : MonoBehaviour, ISelectHandler, IDeselectHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private VisualTheme _theme;
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _surface;
        [SerializeField] private Image _fill;
        [SerializeField] private Image _handle;
        [SerializeField] private Image _edge;
        [SerializeField] private CanvasGroup _focus;
        [SerializeField] private Text _value;
        [SerializeField] private bool _milliseconds;
        private bool _selected;
        private bool _hovered;

        public void Configure(VisualTheme theme, Slider slider, Image surface, Image fill, Image handle,
            Image edge, CanvasGroup focus, Text value, bool milliseconds)
        {
            _theme = theme;
            _slider = slider;
            _surface = surface;
            _fill = fill;
            _handle = handle;
            _edge = edge;
            _focus = focus;
            _value = value;
            _milliseconds = milliseconds;
            _slider.onValueChanged.AddListener(RefreshValue);
            RefreshValue(_slider.value);
            Refresh(true);
        }

        public void OnSelect(BaseEventData eventData) => _selected = true;
        public void OnDeselect(BaseEventData eventData) => _selected = false;
        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData) => _hovered = false;

        private void Awake() => _slider ??= GetComponent<Slider>();

        private void Update() => Refresh(false);

        private void Refresh(bool immediate)
        {
            bool active = _selected || _hovered;
            float blend = immediate ? 1f : 1f - Mathf.Exp(-Time.unscaledDeltaTime * 14f);
            Color accent = _theme != null ? _theme.FocusAccent : new Color(0.6f, 0.78f, 0.77f, 1f);
            Color idle = _theme != null ? _theme.DeepNavy : new Color(0.05f, 0.08f, 0.14f, 1f);
            if (_surface != null) _surface.color = Color.Lerp(_surface.color, active ? Color.Lerp(idle, accent, 0.08f) : idle, blend);
            if (_fill != null) _fill.color = Color.Lerp(_fill.color, active ? accent : new Color(accent.r, accent.g, accent.b, 0.68f), blend);
            if (_handle != null)
            {
                _handle.rectTransform.localScale = Vector3.Lerp(
                    _handle.rectTransform.localScale,
                    Vector3.one * (active ? 1.15f : 1f),
                    blend);
                Color handleIdle = new Color(0.15f, 0.28f, 0.32f, 1f);
                _handle.color = Color.Lerp(_handle.color, active ? accent : handleIdle, blend);
            }
            if (_edge != null) _edge.fillAmount = Mathf.Lerp(_edge.fillAmount, active ? 1f : 0.16f, blend);
            if (_focus != null) _focus.alpha = Mathf.Lerp(_focus.alpha, active ? 0.4f : 0f, blend);
        }

        public void RefreshValue(float value)
        {
            if (_value != null)
            {
                if (_milliseconds)
                {
                    int milliseconds = Mathf.RoundToInt(value * 1000f);
                    string pace = milliseconds <= 18 ? "HIZLI" : milliseconds <= 42 ? "ORTA" : "YAVAŞ";
                    _value.text = $"{pace} · {milliseconds} ms";
                }
                else
                {
                    _value.text = $"{Mathf.RoundToInt(value * 100f)}%";
                }
            }
        }

        private void OnDestroy()
        {
            if (_slider != null) _slider.onValueChanged.RemoveListener(RefreshValue);
        }
    }
}
