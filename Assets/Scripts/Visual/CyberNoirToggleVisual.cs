using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("Null Pointer/Visual/Cyber-Noir Toggle Visual")]
    public sealed class CyberNoirToggleVisual : MonoBehaviour, ISelectHandler, IDeselectHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private VisualTheme _theme;
        [SerializeField] private Toggle _toggle;
        [SerializeField] private Image _surface;
        [SerializeField] private RectTransform _indicator;
        [SerializeField] private Image _edge;
        [SerializeField] private CanvasGroup _focus;
        [SerializeField] private Text _status;
        private bool _selected;
        private bool _hovered;

        public void Configure(VisualTheme theme, Toggle toggle, Image surface, RectTransform indicator,
            Image edge, CanvasGroup focus, Text status)
        {
            _theme = theme;
            _toggle = toggle;
            _surface = surface;
            _indicator = indicator;
            _edge = edge;
            _focus = focus;
            _status = status;
            _toggle.onValueChanged.AddListener(RefreshStatus);
            RefreshStatus(_toggle.isOn);
            Refresh(true);
        }

        public void OnSelect(BaseEventData eventData) => _selected = true;
        public void OnDeselect(BaseEventData eventData) => _selected = false;
        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData) => _hovered = false;
        private void Awake() => _toggle ??= GetComponent<Toggle>();
        private void Update() => Refresh(false);

        private void Refresh(bool immediate)
        {
            if (_toggle == null) return;
            float blend = immediate ? 1f : 1f - Mathf.Exp(-Time.unscaledDeltaTime * 16f);
            bool active = _selected || _hovered;
            Color accent = _theme != null ? _theme.FocusAccent : new Color(0.6f, 0.78f, 0.77f, 1f);
            Color idle = _theme != null ? _theme.DeepNavy : new Color(0.05f, 0.08f, 0.14f, 1f);
            if (_surface != null)
            {
                Color state = _toggle.isOn
                    ? Color.Lerp(idle, accent, active ? 0.26f : 0.14f)
                    : new Color(idle.r, idle.g, idle.b, active ? 0.86f : 0.62f);
                _surface.color = Color.Lerp(_surface.color, state, blend);
            }
            if (_edge != null) _edge.fillAmount = Mathf.Lerp(_edge.fillAmount, active ? 1f : 0.16f, blend);
            if (_focus != null) _focus.alpha = Mathf.Lerp(_focus.alpha, active ? 0.4f : 0f, blend);
            if (_indicator != null)
            {
                Vector2 target = new(_toggle.isOn ? 25f : -25f, 0f);
                _indicator.anchoredPosition = Vector2.Lerp(_indicator.anchoredPosition, target, blend);
                Image indicatorImage = _indicator.GetComponent<Image>();
                if (indicatorImage != null)
                {
                    Color off = new Color(0.24f, 0.34f, 0.38f, 0.92f);
                    indicatorImage.color = Color.Lerp(indicatorImage.color, _toggle.isOn ? accent : off, blend);
                }
            }
        }

        public void RefreshStatus(bool value)
        {
            if (_status != null) _status.text = value ? "AÇIK" : "KAPALI";
            if (_indicator != null)
            {
                _indicator.anchoredPosition = new Vector2(value ? 25f : -25f, 0f);
                Image indicatorImage = _indicator.GetComponent<Image>();
                if (indicatorImage != null)
                {
                    Color accent = _theme != null ? _theme.FocusAccent : new Color(0.6f, 0.78f, 0.77f, 1f);
                    indicatorImage.color = value ? accent : new Color(0.24f, 0.34f, 0.38f, 0.92f);
                }
            }
        }

        private void OnDestroy()
        {
            if (_toggle != null) _toggle.onValueChanged.RemoveListener(RefreshStatus);
        }
    }
}
