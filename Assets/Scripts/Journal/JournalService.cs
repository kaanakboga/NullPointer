using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;
using NullPointer.Evidence;

namespace NullPointer.Journal
{
    public sealed class JournalService
    {
        private readonly GameState _gameState;
        private readonly EvidenceService _evidenceService;
        private readonly IReadOnlyList<JournalEntryData> _entries;

        public JournalService(
            GameState gameState,
            EvidenceService evidenceService,
            IEnumerable<JournalEntryData> entries)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _entries = (entries ?? Array.Empty<JournalEntryData>())
                .Where(entry => entry != null)
                .OrderBy(entry => entry.DisplayOrder)
                .ThenBy(entry => entry.StableId, StringComparer.Ordinal)
                .ToArray();
        }

        public IReadOnlyList<JournalEntryData> GetEntries(JournalSection section)
        {
            return _entries.Where(entry => entry.Section == section && IsUnlocked(entry)).ToArray();
        }

        public IReadOnlyList<EvidenceData> GetCollectedEvidence()
        {
            return _evidenceService.GetCollectedEvidence();
        }

        public bool IsUnlocked(JournalEntryData entry)
        {
            if (entry == null)
            {
                return false;
            }

            return (string.IsNullOrWhiteSpace(entry.RequiredStoryFlag) ||
                    _gameState.HasStoryFlag(entry.RequiredStoryFlag)) &&
                   (string.IsNullOrWhiteSpace(entry.RequiredEvidenceId) ||
                    _gameState.HasCollectedEvidence(entry.RequiredEvidenceId)) &&
                   (string.IsNullOrWhiteSpace(entry.RequiredDeductionId) ||
                    _gameState.HasCompletedDeduction(entry.RequiredDeductionId));
        }
    }
}
