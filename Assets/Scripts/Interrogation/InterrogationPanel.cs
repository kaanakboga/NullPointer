using System;
using System.Collections.Generic;
using NullPointer.Evidence;
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

        public void Show(InterrogationClaimData claim, IReadOnlyList<EvidenceData> evidence)
        {
            _root.SetActive(true);
            _statement.text = claim?.Statement ?? string.Empty;
            _feedback.text = "İfadeyi sınamak için bir kanıt sun.";
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

        public void Hide()
        {
            _root.SetActive(false);
        }
    }
}
