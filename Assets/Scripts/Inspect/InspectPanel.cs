using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NullPointer.Visual;

namespace NullPointer.Inspect
{
    [AddComponentMenu("Null Pointer/Inspect/Inspect Panel")]
    public sealed class InspectPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _titleLabel;
        [SerializeField] private Text _descriptionLabel;
        [SerializeField] private Text _collectionStatusLabel;
        [SerializeField] private Button _collectButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _artImage;
        [SerializeField] private Text _classificationLabel;
        [SerializeField] private Text _metadataLabel;
        [SerializeField] private CyberNoirPanelPresentation _presentation;
        [SerializeField] private GameObject _artFallback;
        [SerializeField] private RectTransform _focusBay;
        [SerializeField] private RectTransform _copyPane;

        public void Configure(
            GameObject panelRoot,
            Text titleLabel,
            Text descriptionLabel,
            Text collectionStatusLabel,
            Button collectButton,
            Button closeButton)
        {
            _panelRoot = panelRoot;
            _titleLabel = titleLabel;
            _descriptionLabel = descriptionLabel;
            _collectionStatusLabel = collectionStatusLabel;
            _collectButton = collectButton;
            _closeButton = closeButton;
        }

        public void Bind(InspectController controller)
        {
            _collectButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
            _collectButton.onClick.AddListener(controller.CollectCurrentEvidence);
            _closeButton.onClick.AddListener(controller.Close);
        }

        public void ConfigurePresentation(
            CyberNoirPanelPresentation presentation,
            Image artImage,
            Text classificationLabel,
            Text metadataLabel,
            GameObject artFallback = null,
            RectTransform focusBay = null,
            RectTransform copyPane = null)
        {
            _presentation = presentation;
            _artImage = artImage;
            _classificationLabel = classificationLabel;
            _metadataLabel = metadataLabel;
            _artFallback = artFallback;
            _focusBay = focusBay;
            _copyPane = copyPane;
        }

        public void Show(InspectData inspection, bool hasEvidence, bool isCollected)
        {
            _titleLabel.text = inspection == null ? string.Empty : inspection.Title;
            _descriptionLabel.text = inspection == null ? string.Empty : inspection.Description;
            if (_classificationLabel != null)
            {
                _classificationLabel.text = hasEvidence ? "SINIFLANDIRMA / ADLİ KANIT" : "SINIFLANDIRMA / ÇEVRESEL GÖZLEM";
            }

            if (_metadataLabel != null)
            {
                _metadataLabel.text = hasEvidence
                    ? "KAYNAK: SAHA İNCELEMESİ\nBÜTÜNLÜK: DOĞRULANABİLİR\nDURUM: İNCELEMEDE"
                    : "KAYNAK: ÇEVRESEL TARAMA\nBÜTÜNLÜK: BAĞLAMSAL\nDURUM: KAYIT AÇIK";
            }

            if (_artImage != null)
            {
                FinalArtSlot slot = _artImage.GetComponent<FinalArtSlot>();
                bool hasArt = _artImage.sprite != null || slot != null && slot.HasFinalArt;
                _artImage.enabled = hasArt;
                _artFallback?.SetActive(!hasArt);
                ApplyArtLayout(hasArt);
            }
            SetCollectionState(hasEvidence, isCollected);
            _panelRoot.SetActive(true);
            _presentation?.Reveal();

            Button initialFocus = hasEvidence && !isCollected ? _collectButton : _closeButton;
            EventSystem.current?.SetSelectedGameObject(initialFocus.gameObject);
        }

        private void ApplyArtLayout(bool hasArt)
        {
            if (_focusBay != null)
            {
                _focusBay.anchoredPosition = hasArt ? new Vector2(302f, 308f) : new Vector2(230f, 330f);
                _focusBay.sizeDelta = hasArt ? new Vector2(440f, 460f) : new Vector2(300f, 300f);
            }

            if (_metadataLabel != null)
            {
                _metadataLabel.rectTransform.anchoredPosition = hasArt ? new Vector2(302f, 62f) : new Vector2(80f, 130f);
                _metadataLabel.rectTransform.sizeDelta = hasArt ? new Vector2(440f, 104f) : new Vector2(300f, 92f);
            }

            if (_copyPane != null)
            {
                _copyPane.anchoredPosition = hasArt ? new Vector2(250f, -16f) : new Vector2(185f, -16f);
                _copyPane.sizeDelta = hasArt ? new Vector2(-600f, -210f) : new Vector2(-470f, -210f);
            }
        }

        public void SetCollectionState(bool hasEvidence, bool isCollected)
        {
            _collectButton.gameObject.SetActive(hasEvidence && !isCollected);
            _collectionStatusLabel.gameObject.SetActive(hasEvidence);
            _collectionStatusLabel.text = isCollected ? "Kanıt kaydedildi" : "Toplanabilir kanıt";
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
