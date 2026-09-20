using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;
using NullPointer.Evidence;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Deduction
{
    [AddComponentMenu("Null Pointer/Deduction/Evidence Board Controller")]
    public sealed class EvidenceBoardController : MonoBehaviour
    {
        [SerializeField] private EvidenceBoardPanel _panel;

        private readonly HashSet<string> _selectedEvidenceIds = new HashSet<string>(StringComparer.Ordinal);
        private GameModeController _gameModes;
        private IGameplayInputSource _input;
        private EvidenceService _evidenceService;
        private DeductionService _deductionService;
        private IReadOnlyList<EvidenceData> _visibleEvidence = Array.Empty<EvidenceData>();
        private bool _isInputSubscribed;

        public bool IsOpen => _gameModes != null && _gameModes.CurrentMode == GameMode.EvidenceBoard;

        public void Configure(EvidenceBoardPanel panel)
        {
            _panel = panel;
        }

        public void Initialize(
            GameModeController gameModes,
            IGameplayInputSource input,
            EvidenceService evidenceService,
            DeductionService deductionService)
        {
            Unsubscribe();
            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _deductionService = deductionService ?? throw new ArgumentNullException(nameof(deductionService));
            _panel.Bind(this);
        }

        public void Open()
        {
            if (_gameModes == null)
            {
                return;
            }

            _selectedEvidenceIds.Clear();
            _gameModes.SetMode(GameMode.EvidenceBoard);
            _panel.Show();
            _panel.SetStatus("Kanıtları seçip bir çıkarım dene.");
            Render();
            Subscribe();
        }

        public void ToggleEvidence(int index)
        {
            if (!IsOpen || index < 0 || index >= _visibleEvidence.Count)
            {
                return;
            }

            string evidenceId = _visibleEvidence[index].StableId;
            if (!_selectedEvidenceIds.Add(evidenceId))
            {
                _selectedEvidenceIds.Remove(evidenceId);
            }

            RenderEvidence();
        }

        public void AttemptDeduction()
        {
            if (!IsOpen)
            {
                return;
            }

            DeductionData candidate = _deductionService.Catalog
                .Where(deduction => !_deductionService.IsCompleted(deduction.StableId))
                .OrderBy(deduction => deduction.StableId, StringComparer.Ordinal)
                .FirstOrDefault(deduction => HasExactSelection(deduction.RequiredEvidenceIds));

            if (candidate == null)
            {
                _panel.SetStatus("Bu seçim doğrulanabilir bir çıkarım oluşturmuyor.");
                return;
            }

            DeductionAttemptResult result = _deductionService.Attempt(candidate.StableId);
            _panel.SetStatus(result.Status == DeductionAttemptStatus.Completed
                ? $"Çıkarım tamamlandı: {candidate.ResultTitle}"
                : "Çıkarım için gerekli kanıtlar henüz tamamlanmadı.");
            _selectedEvidenceIds.Clear();
            Render();
        }

        public void ResetSelection()
        {
            _selectedEvidenceIds.Clear();
            _panel.SetStatus("Seçim temizlendi.");
            RenderEvidence();
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            Unsubscribe();
            _selectedEvidenceIds.Clear();
            _panel.Hide();
            _gameModes.SetMode(GameMode.Gameplay);
        }

        private bool HasExactSelection(IReadOnlyList<string> requiredIds)
        {
            string[] distinctRequired = requiredIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            return distinctRequired.Length == _selectedEvidenceIds.Count &&
                   distinctRequired.All(_selectedEvidenceIds.Contains);
        }

        private void Render()
        {
            _visibleEvidence = _evidenceService.GetCollectedEvidence();
            RenderEvidence();
            DeductionData[] solved = _deductionService.Catalog
                .Where(deduction => _deductionService.IsCompleted(deduction.StableId))
                .OrderBy(deduction => deduction.ResultTitle, StringComparer.CurrentCulture)
                .ToArray();
            _panel.SetSolved(solved);
        }

        private void RenderEvidence()
        {
            _panel.RenderEvidence(_visibleEvidence, _selectedEvidenceIds);
        }

        private void Subscribe()
        {
            if (_isInputSubscribed)
            {
                return;
            }

            _input.PausePressed += Close;
            _isInputSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isInputSubscribed || _input == null)
            {
                return;
            }

            _input.PausePressed -= Close;
            _isInputSubscribed = false;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }
    }
}
