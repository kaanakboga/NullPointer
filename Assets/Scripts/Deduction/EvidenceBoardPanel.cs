using System;
using System.Collections.Generic;
using NullPointer.Evidence;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Deduction
{
    [AddComponentMenu("Null Pointer/Deduction/Evidence Board Panel")]
    public sealed class EvidenceBoardPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Text _solvedLabel;
        [SerializeField] private Button _attemptButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button[] _evidenceButtons = Array.Empty<Button>();
        [SerializeField] private Text[] _evidenceLabels = Array.Empty<Text>();

        public void Configure(
            GameObject panelRoot,
            Text statusLabel,
            Text solvedLabel,
            Button attemptButton,
            Button resetButton,
            Button closeButton,
            Button[] evidenceButtons,
            Text[] evidenceLabels)
        {
            _panelRoot = panelRoot;
            _statusLabel = statusLabel;
            _solvedLabel = solvedLabel;
            _attemptButton = attemptButton;
            _resetButton = resetButton;
            _closeButton = closeButton;
            _evidenceButtons = evidenceButtons;
            _evidenceLabels = evidenceLabels;
        }

        public void Bind(EvidenceBoardController controller)
        {
            _attemptButton.onClick.RemoveAllListeners();
            _resetButton.onClick.RemoveAllListeners();
            _closeButton.onClick.RemoveAllListeners();
            _attemptButton.onClick.AddListener(controller.AttemptDeduction);
            _resetButton.onClick.AddListener(controller.ResetSelection);
            _closeButton.onClick.AddListener(controller.Close);

            for (int index = 0; index < _evidenceButtons.Length; index++)
            {
                int capturedIndex = index;
                _evidenceButtons[index].onClick.RemoveAllListeners();
                _evidenceButtons[index].onClick.AddListener(() => controller.ToggleEvidence(capturedIndex));
            }
        }

        public void Show()
        {
            _panelRoot.SetActive(true);
        }

        public void RenderEvidence(IReadOnlyList<EvidenceData> evidence, ISet<string> selectedIds)
        {
            for (int index = 0; index < _evidenceButtons.Length; index++)
            {
                bool isVisible = evidence != null && index < evidence.Count;
                _evidenceButtons[index].gameObject.SetActive(isVisible);
                if (!isVisible || index >= _evidenceLabels.Length)
                {
                    continue;
                }

                EvidenceData item = evidence[index];
                bool selected = selectedIds.Contains(item.StableId);
                _evidenceLabels[index].text = selected
                    ? $"[SEÇİLİ] {item.DisplayName}"
                    : item.DisplayName;
            }

            GameObject initialFocus = evidence != null && evidence.Count > 0
                ? _evidenceButtons[0].gameObject
                : _closeButton.gameObject;
            EventSystem.current?.SetSelectedGameObject(initialFocus);
        }

        public void SetStatus(string status)
        {
            _statusLabel.text = status ?? string.Empty;
        }

        public void SetSolved(IReadOnlyList<DeductionData> solved)
        {
            if (solved == null || solved.Count == 0)
            {
                _solvedLabel.text = "Çözülmüş çıkarım yok.";
                return;
            }

            var lines = new List<string>();
            foreach (DeductionData deduction in solved)
            {
                lines.Add($"✓ {deduction.ResultTitle}\n{deduction.ResultText}");
            }

            _solvedLabel.text = string.Join("\n\n", lines);
        }

        public void Hide()
        {
            _panelRoot.SetActive(false);
        }
    }
}
