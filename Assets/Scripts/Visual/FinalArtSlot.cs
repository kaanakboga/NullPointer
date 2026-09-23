using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Final Art Slot")]
    public sealed class FinalArtSlot : MonoBehaviour
    {
        [SerializeField] private string _stableId = string.Empty;
        [SerializeField] private string _manifestAssetId = string.Empty;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Image _uiImage;
        [SerializeField] private Sprite _finalArt;
        [SerializeField] private Sprite _structuralPlaceholder;
        [SerializeField] private Color _structuralSpriteColor = Color.white;
        [SerializeField] private Color _structuralUiColor = Color.white;
        [SerializeField] private bool _hasStructuralColors;

        public string StableId => _stableId;
        public string ManifestAssetId => _manifestAssetId;
        public Sprite FinalArt => _finalArt;
        public bool HasFinalArt => _finalArt != null;

        public void ConfigureStructural(
            string stableId,
            string manifestAssetId,
            SpriteRenderer spriteRenderer = null,
            Image uiImage = null)
        {
            _stableId = stableId?.Trim() ?? string.Empty;
            _manifestAssetId = manifestAssetId?.Trim() ?? string.Empty;
            _spriteRenderer = spriteRenderer;
            _uiImage = uiImage;
            if (_structuralPlaceholder == null)
            {
                _structuralPlaceholder = _spriteRenderer != null
                    ? _spriteRenderer.sprite
                    : _uiImage != null
                        ? _uiImage.sprite
                        : null;
            }

            if (!_hasStructuralColors)
            {
                _structuralSpriteColor = _spriteRenderer != null ? _spriteRenderer.color : Color.white;
                _structuralUiColor = _uiImage != null ? _uiImage.color : Color.white;
                _hasStructuralColors = true;
            }

            Apply();
        }

        public void SetFinalArt(Sprite art)
        {
            _finalArt = art;
            Apply();
        }

        public void Apply()
        {
            Sprite resolved = _finalArt != null ? _finalArt : _structuralPlaceholder;
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = resolved;
                _spriteRenderer.color = _finalArt != null ? Color.white : _structuralSpriteColor;
            }

            if (_uiImage != null)
            {
                _uiImage.sprite = resolved;
                _uiImage.color = _finalArt != null ? Color.white : _structuralUiColor;
            }
        }

        private void OnEnable()
        {
            Apply();
        }
    }
}
