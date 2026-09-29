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
        [SerializeField] private CanvasGroup _focusBracket;
        [SerializeField] private RectTransform _movingAccent;
        [SerializeField] private Text _index;
        [SerializeField] private Vector2 _focusedTextOffset = new(5f, 0f);
        [SerializeField, Range(1f, 1.08f)] private float _focusedScale = 1.015f;
        [SerializeField] private UiSoundHooks _soundHooks;
        [SerializeField] private bool _backAction;

        private bool _focused;
        private bool _hovered;
        private bool _pressed;
        private bool _wasInteractable;
        private Vector2 _labelRestingPosition;
        private Vector3 _restingScale = Vector3.one;
        private Vector2 _rootRestingPosition;
        private float _edgeRestingWidth;
        private float _edgeFocusedWidth;
        private Vector2 _accentRestingPosition;
        private float _clickPulse;

        public void Configure(
            VisualTheme theme,
            Button button,
            Image surface,
            Text label,
            Image edgeLine,
            CanvasGroup focusGlow,
            RectTransform visualRoot,
            CanvasGroup focusBracket = null,
            RectTransform movingAccent = null,
            Text index = null)
        {
            _theme = theme;
            _button = button;
            _surface = surface;
            _label = label;
            _edgeLine = edgeLine;
            _focusGlow = focusGlow;
            _visualRoot = visualRoot;
            _focusBracket = focusBracket;
            _movingAccent = movingAccent;
            _index = index;
            CacheRestingState();
            PrepareEdgeLine();
            RefreshImmediate();
        }

        public void ConfigureSound(UiSoundHooks soundHooks, bool backAction = false)
        {
            _soundHooks = soundHooks;
            _backAction = backAction;
        }

        public void SetPreviewFocused(bool focused)
        {
            _focused = focused;
            RefreshImmediate();
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (!_focused)
            {
                _soundHooks?.PlayFocus();
            }

            _focused = true;
        }
        public void OnDeselect(BaseEventData eventData) => _focused = false;
        public void OnPointerEnter(PointerEventData eventData) => _hovered = true;
        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            _pressed = false;
        }

        public void OnPointerDown(PointerEventData eventData) => _pressed = true;
        public void OnPointerUp(PointerEventData eventData) => _pressed = false;
        public void OnSubmit(BaseEventData eventData)
        {
            if (_button != null && !_button.interactable)
            {
                _soundHooks?.PlayInvalid();
                return;
            }

            _clickPulse = 1f;
        }

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
            float edgeTarget = active ? _edgeFocusedWidth : _edgeRestingWidth;
            float glowTarget = active ? 1f : 0f;

            if (_visualRoot != null)
            {
                _visualRoot.localScale = Vector3.Lerp(
                    _visualRoot.localScale,
                    _restingScale * targetScale,
                    blend);
                _visualRoot.anchoredPosition = Vector2.Lerp(
                    _visualRoot.anchoredPosition,
                    _rootRestingPosition + (active ? new Vector2(7f, 0f) : Vector2.zero),
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
                RectTransform edgeRect = _edgeLine.rectTransform;
                edgeRect.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    Mathf.Lerp(edgeRect.rect.width, edgeTarget, blend));
            }

            if (_focusGlow != null)
            {
                _focusGlow.alpha = Mathf.Lerp(_focusGlow.alpha, glowTarget, blend);
            }

            if (_focusBracket != null)
            {
                _focusBracket.alpha = Mathf.Lerp(_focusBracket.alpha, active ? 1f : 0f, blend);
            }

            if (_movingAccent != null)
            {
                Vector2 pulseOffset = active
                    ? new Vector2(20f + Mathf.Sin(Time.unscaledTime * 3.2f) * 16f, 0f)
                    : Vector2.zero;
                _movingAccent.anchoredPosition = Vector2.Lerp(
                    _movingAccent.anchoredPosition,
                    _accentRestingPosition + pulseOffset,
                    blend);
            }

            _clickPulse = Mathf.MoveTowards(_clickPulse, 0f, Time.unscaledDeltaTime / Mathf.Max(0.01f, duration));
            ApplyColors(active);
        }

        private void PlayClickPulse()
        {
            if (_button != null && _button.interactable)
            {
                _clickPulse = 1f;
                if (_backAction)
                {
                    _soundHooks?.PlayBack();
                }
                else
                {
                    _soundHooks?.PlayConfirm();
                }
            }
            else
            {
                _soundHooks?.PlayInvalid();
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
            normal.a = 0.18f;
            focused.a = 0.72f;
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

            if (_index != null)
            {
                Color indexColor = active
                    ? (_theme != null ? _theme.FocusAccent : Color.cyan)
                    : new Color(0.34f, 0.56f, 0.59f, 0.62f);
                _index.color = indexColor;
            }
        }

        private void RefreshImmediate()
        {
            bool active = _button != null && _button.interactable && (_focused || _hovered);
            ApplyColors(active);
            if (_edgeLine != null)
            {
                _edgeLine.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    active ? _edgeFocusedWidth : _edgeRestingWidth);
            }

            if (_focusGlow != null)
            {
                _focusGlow.alpha = active ? 1f : 0f;
            }


            if (_focusBracket != null)
            {
                _focusBracket.alpha = active ? 1f : 0f;
            }
        }

        private void CacheRestingState()
        {
            _restingScale = _visualRoot != null ? _visualRoot.localScale : transform.localScale;
            _rootRestingPosition = _visualRoot != null ? _visualRoot.anchoredPosition : Vector2.zero;
            _labelRestingPosition = _label != null ? _label.rectTransform.anchoredPosition : Vector2.zero;
            _accentRestingPosition = _movingAccent != null ? _movingAccent.anchoredPosition : Vector2.zero;
            _edgeRestingWidth = 42f;
            _edgeFocusedWidth = _visualRoot != null ? Mathf.Max(100f, _visualRoot.rect.width - 64f) : 320f;
        }

        private void PrepareEdgeLine()
        {
            if (_edgeLine == null)
            {
                return;
            }

            _edgeLine.type = Image.Type.Simple;
        }
    }
}
