using System;
using System.Collections.Generic;
using NullPointer.Evidence;
using NullPointer.Visual;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Interrogation
{
    [AddComponentMenu("Null Pointer/Interrogation/Interrogation Panel")]
    public sealed class InterrogationPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Text _statement;
        [SerializeField] private Text _feedback;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button[] _evidenceButtons = Array.Empty<Button>();
        [SerializeField] private Text[] _evidenceLabels = Array.Empty<Text>();
        [SerializeField] private Image _portraitImage;
        [SerializeField] private GameObject _portraitFallback;
        [SerializeField] private Image _evidencePreview;
        [SerializeField] private GameObject _evidencePreviewFrame;
        [SerializeField] private GameObject _evidenceFallback;
        [SerializeField] private Text _claimMetadata;
        [SerializeField] private CyberNoirPanelPresentation _presentation;

        public void Configure(
            GameObject root,
            Text statement,
            Text feedback,
            Button closeButton,
            Button[] evidenceButtons,
            Text[] evidenceLabels)
        {
            _root = root;
            _statement = statement;
            _feedback = feedback;
            _closeButton = closeButton;
            _evidenceButtons = evidenceButtons ?? Array.Empty<Button>();
            _evidenceLabels = evidenceLabels ?? Array.Empty<Text>();
        }

        public void Bind(InterrogationController controller)
        {
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(controller.Close);
            for (int index = 0; index < _evidenceButtons.Length; index++)
            {
                int captured = index;
                _evidenceButtons[index].onClick.RemoveAllListeners();
                _evidenceButtons[index].onClick.AddListener(() => controller.PresentEvidence(captured));
            }
        }

        public void ConfigurePresentation(
            CyberNoirPanelPresentation presentation,
            Image portraitImage,
            Image evidencePreview,
            Text claimMetadata,
            GameObject portraitFallback = null,
            GameObject evidencePreviewFrame = null,
            GameObject evidenceFallback = null)
        {
            _presentation = presentation;
            _portraitImage = portraitImage;
            _evidencePreview = evidencePreview;
            _claimMetadata = claimMetadata;
            _portraitFallback = portraitFallback;
            _evidencePreviewFrame = evidencePreviewFrame;
            _evidenceFallback = evidenceFallback;
        }

        public void Show(InterrogationClaimData claim, IReadOnlyList<EvidenceData> evidence)
        {
            _root.SetActive(true);
            _presentation?.Reveal();
            _statement.text = claim?.Statement ?? string.Empty;
            _feedback.text = "İfadeyi sınamak için bir kanıt sun.";
            if (_claimMetadata != null)
            {
                _claimMetadata.text = "İFADE ANALİZİ / ÇELİŞKİ ARANIYOR\nKANIT HAVUZU: " + (evidence?.Count ?? 0).ToString("00");
            }

            if (_portraitImage != null)
            {
                FinalArtSlot slot = _portraitImage.GetComponent<FinalArtSlot>();
                bool hasPortrait = _portraitImage.sprite != null || slot != null && slot.HasFinalArt;
                _portraitImage.enabled = hasPortrait;
                _portraitFallback?.SetActive(!hasPortrait);
            }

            if (_evidencePreview != null)
            {
                _evidencePreview.enabled = false;
            }
            _evidencePreviewFrame?.SetActive(false);
            for (int index = 0; index < _evidenceButtons.Length; index++)
            {
                bool visible = evidence != null && index < evidence.Count;
                _evidenceButtons[index].gameObject.SetActive(visible);
                if (visible && index < _evidenceLabels.Length)
                {
                    _evidenceLabels[index].text = evidence[index].DisplayName;
                }
            }

            EventSystem.current?.SetSelectedGameObject(
                evidence != null && evidence.Count > 0 ? _evidenceButtons[0].gameObject : _closeButton.gameObject);
        }

        public void SetFeedback(string message)
        {
            _feedback.text = message ?? string.Empty;
        }

        public void SetPresentedEvidence(EvidenceData evidence, bool contradictionFound)
        {
            if (_evidencePreview != null)
            {
                _evidencePreview.sprite = evidence?.Icon;
                _evidencePreview.enabled = evidence != null && evidence.Icon != null;
            }

            _evidencePreviewFrame?.SetActive(evidence != null);
            _evidenceFallback?.SetActive(evidence != null && evidence.Icon == null);

            if (contradictionFound)
            {
                _presentation?.PlaySuccessFeedback();
            }
            else
            {
                _presentation?.PlayInvalidFeedback();
            }
        }

        public void Hide()
        {
            if (_presentation != null)
            {
                _presentation.HideAnimated();
            }
            else
            {
                _root.SetActive(false);
            }
        }
    }
}
