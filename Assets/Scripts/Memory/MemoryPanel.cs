using UnityEngine;
using UnityEngine.UI;
using NullPointer.Visual;

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
        [SerializeField] private Text _fragmentLabel;
        [SerializeField] private CyberNoirPanelPresentation _presentation;

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
            if (_fragmentLabel != null)
            {
                _fragmentLabel.text = "BELLEK PARÇASI / BÜTÜNLÜK BELİRSİZ\nKAYNAK EŞLEŞMESİ DEVAM EDİYOR";
            }
            _canvasGroup.alpha = 1f;
            _presentation?.Reveal();
        }

        public void ConfigurePresentation(CyberNoirPanelPresentation presentation, Text fragmentLabel)
        {
            _presentation = presentation;
            _fragmentLabel = fragmentLabel;
        }

        public void ShowBeat(MemoryBeat beat)
        {
            _beatLabel.text = beat?.Text ?? string.Empty;
            Color overlay = beat?.OverlayColor ?? Color.clear;
            overlay.a = Mathf.Min(overlay.a, 0.16f);
            _overlay.color = overlay;
        }

        public void Hide()
        {
            if (_presentation != null)
            {
                _presentation.HideAnimated();
            }
            else
            {
                _panelRoot.SetActive(false);
            }
        }
    }
}
