using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Final Art Fallback Visibility")]
    public sealed class FinalArtFallbackVisibility : MonoBehaviour
    {
        [SerializeField] private FinalArtSlot _slot;
        [SerializeField] private GameObject _fallbackRoot;

        public void Configure(FinalArtSlot slot, GameObject fallbackRoot)
        {
            _slot = slot;
            _fallbackRoot = fallbackRoot;
            Refresh();
        }

        public void Refresh()
        {
            if (_fallbackRoot != null)
            {
                _fallbackRoot.SetActive(_slot == null || !_slot.HasFinalArt);
            }
        }

        private void OnEnable()
        {
            Refresh();
        }
    }
}
