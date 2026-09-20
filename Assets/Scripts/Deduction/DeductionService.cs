using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;
using NullPointer.Evidence;

namespace NullPointer.Deduction
{
    public sealed class DeductionService
    {
        private readonly GameState _gameState;
        private readonly EvidenceService _evidenceService;
        private readonly Dictionary<string, DeductionData> _deductionsById;

        public DeductionService(
            GameState gameState,
            EvidenceService evidenceService,
            IEnumerable<DeductionData> deductions)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _deductionsById = new Dictionary<string, DeductionData>(StringComparer.Ordinal);

            if (deductions == null)
            {
                return;
            }

            foreach (DeductionData deduction in deductions)
            {
                if (deduction == null || string.IsNullOrWhiteSpace(deduction.StableId))
                {
                    continue;
                }

                if (!_deductionsById.TryAdd(deduction.StableId, deduction))
                {
                    throw new ArgumentException(
                        $"Deduction catalog contains duplicate stable ID '{deduction.StableId}'.",
                        nameof(deductions));
                }
            }
        }

        public event Action<DeductionCompleted> DeductionCompleted;

        public IReadOnlyCollection<DeductionData> Catalog => _deductionsById.Values;

        public DeductionAttemptResult Attempt(string deductionId)
        {
            if (string.IsNullOrWhiteSpace(deductionId) ||
                !_deductionsById.TryGetValue(deductionId.Trim(), out DeductionData deduction))
            {
                return new DeductionAttemptResult(
                    DeductionAttemptStatus.UnknownDeduction,
                    null,
                    Array.Empty<string>());
            }

            if (_gameState.HasCompletedDeduction(deduction.StableId))
            {
                return new DeductionAttemptResult(
                    DeductionAttemptStatus.AlreadyCompleted,
                    deduction,
                    Array.Empty<string>());
            }

            string[] requiredIds = deduction.RequiredEvidenceIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (requiredIds.Length == 0)
            {
                return new DeductionAttemptResult(
                    DeductionAttemptStatus.InvalidDefinition,
                    deduction,
                    Array.Empty<string>());
            }

            string[] missingIds = requiredIds
                .Where(id => !_gameState.HasCollectedEvidence(id))
                .ToArray();

            if (missingIds.Length > 0)
            {
                return new DeductionAttemptResult(
                    DeductionAttemptStatus.MissingEvidence,
                    deduction,
                    missingIds);
            }

            _gameState.CompleteDeduction(deduction.StableId);

            if (!string.IsNullOrWhiteSpace(deduction.ResultingEvidenceId))
            {
                _evidenceService.Collect(deduction.ResultingEvidenceId);
            }

            foreach (string flagId in deduction.StoryFlagsToSet)
            {
                if (!string.IsNullOrWhiteSpace(flagId))
                {
                    _gameState.SetStoryFlag(flagId);
                }
            }

            DeductionCompleted?.Invoke(new DeductionCompleted(deduction));
            return new DeductionAttemptResult(
                DeductionAttemptStatus.Completed,
                deduction,
                Array.Empty<string>());
        }

        public bool IsCompleted(string deductionId)
        {
            return !string.IsNullOrWhiteSpace(deductionId) &&
                   _gameState.HasCompletedDeduction(deductionId);
        }
    }
}
