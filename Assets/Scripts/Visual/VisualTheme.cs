using UnityEngine;

namespace NullPointer.Visual
{
    [CreateAssetMenu(menuName = "Null Pointer/Visual/Visual Theme", fileName = "VT_VisualTheme")]
    public sealed class VisualTheme : ScriptableObject
    {
        [Header("Surfaces")]
        [SerializeField] private Color _nearBlack = new(0.027f, 0.039f, 0.059f, 1f);
        [SerializeField] private Color _deepNavy = new(0.047f, 0.082f, 0.137f, 1f);
        [SerializeField] private Color _charcoal = new(0.086f, 0.102f, 0.125f, 1f);
        [SerializeField] private Color _panel = new(0.055f, 0.094f, 0.145f, 0.96f);

        [Header("Accents")]
        [SerializeField] private Color _primaryAccent = new(0.302f, 0.627f, 0.639f, 1f);
        [SerializeField] private Color _secondaryAccent = new(0.365f, 0.302f, 0.510f, 1f);
        [SerializeField] private Color _warningAccent = new(0.824f, 0.541f, 0.239f, 1f);
        [SerializeField] private Color _dangerAccent = new(0.722f, 0.290f, 0.337f, 1f);
        [SerializeField] private Color _focusAccent = new(0.604f, 0.784f, 0.773f, 1f);

        [Header("Text and Disabled")]
        [SerializeField] private Color _primaryText = new(0.878f, 0.894f, 0.898f, 1f);
        [SerializeField] private Color _secondaryText = new(0.529f, 0.588f, 0.627f, 1f);
        [SerializeField] private Color _disabled = new(0.247f, 0.286f, 0.318f, 0.72f);

        [Header("Motion Seconds")]
        [SerializeField, Min(0.01f)] private float _microDuration = 0.16f;
        [SerializeField, Min(0.01f)] private float _panelDuration = 0.24f;
        [SerializeField, Min(0.01f)] private float _revealDuration = 0.46f;
        [SerializeField, Min(0.01f)] private float _focusPulsePeriod = 1.2f;

        [Header("Common Opacity")]
        [SerializeField, Range(0f, 1f)] private float _panelOpacity = 0.96f;
        [SerializeField, Range(0f, 1f)] private float _backdropOpacity = 0.82f;
        [SerializeField, Range(0f, 1f)] private float _disabledOpacity = 0.46f;
        [SerializeField, Range(0f, 1f)] private float _subtleFxOpacity = 0.08f;

        public Color NearBlack => _nearBlack;
        public Color DeepNavy => _deepNavy;
        public Color Charcoal => _charcoal;
        public Color Panel => _panel;
        public Color PrimaryAccent => _primaryAccent;
        public Color SecondaryAccent => _secondaryAccent;
        public Color WarningAccent => _warningAccent;
        public Color DangerAccent => _dangerAccent;
        public Color FocusAccent => _focusAccent;
        public Color PrimaryText => _primaryText;
        public Color SecondaryText => _secondaryText;
        public Color Disabled => _disabled;
        public float MicroDuration => _microDuration;
        public float PanelDuration => _panelDuration;
        public float RevealDuration => _revealDuration;
        public float FocusPulsePeriod => _focusPulsePeriod;
        public float PanelOpacity => _panelOpacity;
        public float BackdropOpacity => _backdropOpacity;
        public float DisabledOpacity => _disabledOpacity;
        public float SubtleFxOpacity => _subtleFxOpacity;

        public bool IsValid(out string reason)
        {
            if (_microDuration is < 0.12f or > 0.22f)
            {
                reason = "Micro duration must remain within the 120–220 ms visual language.";
                return false;
            }

            if (_panelDuration is < 0.18f or > 0.32f)
            {
                reason = "Panel duration must remain within the 180–320 ms visual language.";
                return false;
            }

            if (_revealDuration is < 0.3f or > 0.65f)
            {
                reason = "Reveal duration must remain within the 300–650 ms visual language.";
                return false;
            }

            if (_primaryText.a < 0.99f || _primaryAccent.a < 0.99f || _focusAccent.a < 0.99f)
            {
                reason = "Primary semantic colors must be authored as opaque colors; opacity belongs to presentation.";
                return false;
            }

            reason = string.Empty;
            return true;
        }
    }
}
