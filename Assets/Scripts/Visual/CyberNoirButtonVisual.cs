using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("Null Pointer/Visual/Cyber-Noir Button Visual")]
    public sealed class CyberNoirButtonVisual : MonoBehaviour,
        ISelectHandler,
        IDeselectHandler,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        ISubmitHandler
    {
        [SerializeField] private VisualTheme _theme;
        [SerializeField] private Button _button;
        [SerializeField] private Image _surface;
        [SerializeField] private Text _label;
        [SerializeField] private Image _edgeLine;
        [SerializeField] private CanvasGroup _focusGlow;
        [SerializeField] private RectTransform _visualRoot;
        [SerializeField] private Vector2 _focusedTextOffset = new(5f, 0f);
        [SerializeField, Range(1f, 1.08f)] private float _focusedScale = 1.015f;

        private bool _focused;
        private bool _hovered;
        private bool _pressed;
        private bool _wasInteractable;
        private Vector2 _labelRestingPosition;
        private Vector3 _restingScale = Vector3.one;
        private float _clickPulse;

        public void Configure(
            VisualTheme theme,
            Button button,
            Image surface,
            Text label,
            Image edgeLine,
            CanvasGroup focusGlow,
            RectTransform visualRoot)
        {
            _theme = theme;
            _button = button;
            _surface = surface;
            _label = label;
            _edgeLine = edgeLine;
            _focusGlow = focusGlow;
            _visualRoot = visualRoot;
            CacheRestingState();
            PrepareEdgeLine();
            RefreshImmediate();
        }

        public void OnSelect(BaseEventData eventData) => _focused = true;
        public void OnDeselect(BaseEventData eventData) => _focused = false;
        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            _pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
        public void OnSubmit(BaseEventData eventData) => PlayClickPulse();

        private void Awake()
        {
            _button ??= GetComponent<Button>();
            _surface ??= GetComponent<Image>();
            _visualRoot ??= transform as RectTransform;
            CacheRestingState();
            PrepareEdgeLine();
        }

        private void OnEnable()
        {
            _button ??= GetComponent<Button>();
            _button.onClick.AddListener(PlayClickPulse);
            _wasInteractable = _button.interactable;
            RefreshImmediate();
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(PlayClickPulse);
            }
        }

        private void Update()
        {
            if (_button == null)
            {
                return;
            }

            if (_wasInteractable != _button.interactable)
            {
                _wasInteractable = _button.interactable;
            }

            float duration = _theme != null ? _theme.MicroDuration : 0.16f;
            float blend = 1f - Mathf.Exp(-Time.unscaledDeltaTime * 5f / Mathf.Max(0.01f, duration));
            bool active = _button.interactable && (_focused || _hovered);
            float targetScale = _pressed ? 0.985f : active ? _focusedScale : 1f;
            targetScale += _clickPulse * 0.018f;
            Vector2 targetOffset = active ? _focusedTextOffset : Vector2.zero;
            float edgeTarget = active ? 1f : 0.12f;
            float glowTarget = active ? 1f : 0f;

            if (_visualRoot != null)
            {
                _visualRoot.localScale = Vector3.Lerp(
                    _visualRoot.localScale,
                    _restingScale * targetScale,
                    blend);
            }

            if (_label != null)
            {
                _label.rectTransform.anchoredPosition = Vector2.Lerp(
                    _label.rectTransform.anchoredPosition,
                    _labelRestingPosition + targetOffset,
                    blend);
            }

            if (_edgeLine != null)
            {
                _edgeLine.fillAmount = Mathf.Lerp(_edgeLine.fillAmount, edgeTarget, blend);
            }

            if (_focusGlow != null)
            {
                _focusGlow.alpha = Mathf.Lerp(_focusGlow.alpha, glowTarget, blend);
            }

            _clickPulse = Mathf.MoveTowards(_clickPulse, 0f, Time.unscaledDeltaTime / Mathf.Max(0.01f, duration));
            ApplyColors(active);
        }

        private void PlayClickPulse()
        {
            if (_button != null && _button.interactable)
            {
                _clickPulse = 1f;
            }
        }

        private void ApplyColors(bool active)
        {
            if (_surface == null)
            {
                return;
            }

            Color normal = _theme != null ? _theme.DeepNavy : new Color(0.05f, 0.1f, 0.15f, 1f);
            Color focused = _theme != null ? _theme.PrimaryAccent : new Color(0.3f, 0.63f, 0.64f, 1f);
            Color disabled = _theme != null ? _theme.Disabled : new Color(0.25f, 0.28f, 0.31f, 0.7f);
            _surface.color = !_button.interactable
                ? disabled
                : _pressed
                    ? Color.Lerp(normal, focused, 0.28f)
                    : active
                        ? Color.Lerp(normal, focused, 0.16f)
                        : normal;
            if (_label != null)
            {
                _label.color = !_button.interactable
                    ? disabled
                    : _theme != null
                        ? _theme.PrimaryText
                        : Color.white;
            }
        }

        private void RefreshImmediate()
        {
            bool active = _button != null && _button.interactable && (_focused || _hovered);
            ApplyColors(active);
            if (_edgeLine != null)
            {
                _edgeLine.fillAmount = active ? 1f : 0.12f;
            }

            if (_focusGlow != null)
            {
                _focusGlow.alpha = active ? 1f : 0f;
            }
        }

        private void CacheRestingState()
        {
            _restingScale = _visualRoot != null ? _visualRoot.localScale : transform.localScale;
            _labelRestingPosition = _label != null ? _label.rectTransform.anchoredPosition : Vector2.zero;
        }

        private void PrepareEdgeLine()
        {
            if (_edgeLine == null)
            {
                return;
            }

            _edgeLine.type = Image.Type.Filled;
            _edgeLine.fillMethod = Image.FillMethod.Horizontal;
            _edgeLine.fillOrigin = 0;
        }
    }
}
