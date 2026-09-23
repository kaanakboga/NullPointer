using UnityEngine;

namespace NullPointer.Visual
{
    public enum VisualRootKind
    {
        MainMenu,
        GameplayLocation,
        DevelopmentPreview
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Visual Root Anchor")]
    public sealed class VisualRootAnchor : MonoBehaviour
    {
        [SerializeField] private string _stableId = string.Empty;
        [SerializeField] private VisualRootKind _kind;
        [SerializeField] private VisualTheme _theme;

        public string StableId => _stableId;
        public VisualRootKind Kind => _kind;
        public VisualTheme Theme => _theme;

        public void Configure(string stableId, VisualRootKind kind, VisualTheme theme)
        {
            _stableId = stableId?.Trim() ?? string.Empty;
            _kind = kind;
            _theme = theme;
        }
    }
}
