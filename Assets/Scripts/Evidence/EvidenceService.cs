using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;

namespace NullPointer.Evidence
{
    public sealed class EvidenceService
    {
        private readonly GameState _gameState;
        private readonly Dictionary<string, EvidenceData> _evidenceById;

        public EvidenceService(GameState gameState, IEnumerable<EvidenceData> evidence)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _evidenceById = new Dictionary<string, EvidenceData>(StringComparer.Ordinal);

            if (evidence == null)
            {
                return;
            }

            foreach (EvidenceData item in evidence)
            {
                if (item == null || string.IsNullOrWhiteSpace(item.StableId))
                {
                    continue;
                }

                if (!_evidenceById.TryAdd(item.StableId, item))
                {
                    throw new ArgumentException(
                        $"Evidence catalog contains duplicate stable ID '{item.StableId}'.",
                        nameof(evidence));
                }
            }
        }

        public event Action<EvidenceCollected> EvidenceCollected;

        public IReadOnlyCollection<EvidenceData> Catalog => _evidenceById.Values;

        public EvidenceCollectionStatus Collect(string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                return EvidenceCollectionStatus.InvalidId;
            }

            string normalizedId = evidenceId.Trim();
            if (!_evidenceById.TryGetValue(normalizedId, out EvidenceData evidence))
            {
                return EvidenceCollectionStatus.UnknownEvidence;
            }

            if (!_gameState.CollectEvidence(normalizedId))
            {
                return EvidenceCollectionStatus.AlreadyCollected;
            }

            EvidenceCollected?.Invoke(new EvidenceCollected(evidence));
            return EvidenceCollectionStatus.Collected;
        }

        public EvidenceCollectionStatus Collect(EvidenceData evidence)
        {
            return evidence == null ? EvidenceCollectionStatus.UnknownEvidence : Collect(evidence.StableId);
        }

        public bool IsCollected(string evidenceId)
        {
            return !string.IsNullOrWhiteSpace(evidenceId) && _gameState.HasCollectedEvidence(evidenceId);
        }

        public bool TryGet(string evidenceId, out EvidenceData evidence)
        {
            if (string.IsNullOrWhiteSpace(evidenceId))
            {
                evidence = null;
                return false;
            }

            return _evidenceById.TryGetValue(evidenceId.Trim(), out evidence);
        }

        public IReadOnlyList<EvidenceData> GetCollectedEvidence()
        {
            return _gameState.CollectedEvidenceIds
                .Select(id => _evidenceById.TryGetValue(id, out EvidenceData evidence) ? evidence : null)
                .Where(evidence => evidence != null)
                .OrderBy(evidence => evidence.DisplayName, StringComparer.CurrentCulture)
                .ThenBy(evidence => evidence.StableId, StringComparer.Ordinal)
                .ToArray();
        }
    }
}
