using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.Memory
{
    [AddComponentMenu("Null Pointer/Memory/Memory Panel")]
    public sealed class MemoryPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Image _overlay;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Text _beatLabel;
        [SerializeField] private CanvasGroup _canvasGroup;

        public void Configure(
            GameObject panelRoot,
            Image overlay,
            Text titleLabel,
            Text beatLabel,
            CanvasGroup canvasGroup)
        {
            _panelRoot = panelRoot;
            _overlay = overlay;
            _titleLabel = titleLabel;
            _beatLabel = beatLabel;
            _canvasGroup = canvasGroup;
        }

        public void Show(MemoryData memory)
        {
            _panelRoot.SetActive(true);
            _titleLabel.text = memory == null ? string.Empty : memory.Title;
            _beatLabel.text = string.Empty;
            _canvasGroup.alpha = 1f;
        }

        public void ShowBeat(MemoryBeat beat)
        {
            _beatLabel.text = beat?.Text ?? string.Empty;
            _overlay.color = beat?.OverlayColor ?? Color.clear;
        }

        public void Hide()
        {
            _panelRoot.SetActive(false);
        }
    }
}
