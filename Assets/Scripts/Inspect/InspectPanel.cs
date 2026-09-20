using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

        public void Show(InspectData inspection, bool hasEvidence, bool isCollected)
        {
            _titleLabel.text = inspection == null ? string.Empty : inspection.Title;
            _descriptionLabel.text = inspection == null ? string.Empty : inspection.Description;
            SetCollectionState(hasEvidence, isCollected);
            _panelRoot.SetActive(true);

            Button initialFocus = hasEvidence && !isCollected ? _collectButton : _closeButton;
            EventSystem.current?.SetSelectedGameObject(initialFocus.gameObject);
        }

        public void SetCollectionState(bool hasEvidence, bool isCollected)
        {
            _collectButton.gameObject.SetActive(hasEvidence && !isCollected);
            _collectionStatusLabel.gameObject.SetActive(hasEvidence);
            _collectionStatusLabel.text = isCollected ? "Kanıt kaydedildi" : "Toplanabilir kanıt";
        }

        public void Hide()
        {
            _panelRoot.SetActive(false);
        }
    }
}
