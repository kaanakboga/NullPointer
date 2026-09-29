using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Dropdown))]
    [AddComponentMenu("Null Pointer/Visual/Cyber-Noir Dropdown Visual")]
    public sealed class CyberNoirDropdownVisual : MonoBehaviour, ISelectHandler, IDeselectHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private VisualTheme _theme;
        [SerializeField] private Image _surface;
        [SerializeField] private Image _edge;
        [SerializeField] private CanvasGroup _focus;
        private bool _selected;
        private bool _hovered;

        public void Configure(VisualTheme theme, Image surface, Image edge, CanvasGroup focus)
        {
            _theme = theme;
            _surface = surface;
            _edge = edge;
            _focus = focus;
            Refresh(true);
        }

        public void OnSelect(BaseEventData eventData) => _selected = true;
        public void OnDeselect(BaseEventData eventData) => _selected = false;
        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData) => _hovered = false;
        private void Update() => Refresh(false);

        public void RefreshImmediate() => Refresh(true);

        private void Refresh(bool immediate)
        {
            bool active = _selected || _hovered;
            float blend = immediate ? 1f : 1f - Mathf.Exp(-Time.unscaledDeltaTime * 14f);
            Color accent = _theme != null ? _theme.FocusAccent : new Color(0.6f, 0.78f, 0.77f, 1f);
            Color idle = _theme != null ? _theme.DeepNavy : new Color(0.05f, 0.08f, 0.14f, 1f);
            if (_surface != null) _surface.color = Color.Lerp(_surface.color, active ? Color.Lerp(idle, accent, 0.1f) : idle, blend);
            if (_edge != null) _edge.fillAmount = Mathf.Lerp(_edge.fillAmount, active ? 1f : 0.16f, blend);
            if (_focus != null) _focus.alpha = Mathf.Lerp(_focus.alpha, active ? 0.4f : 0f, blend);
        }
    }
}
