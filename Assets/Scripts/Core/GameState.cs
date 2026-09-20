using System;
using System.Collections.Generic;
using System.Linq;

namespace NullPointer.Core
{
    public sealed class GameState
    {
        public const int CurrentSchemaVersion = 1;

        private readonly HashSet<string> _storyFlags = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _collectedEvidenceIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _unlockedMemoryIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _completedDeductionIds = new HashSet<string>(StringComparer.Ordinal);

        public string CurrentCaseId { get; private set; } = string.Empty;

        public string LocationId { get; private set; } = string.Empty;

        public string CheckpointId { get; private set; } = string.Empty;

        public IReadOnlyCollection<string> StoryFlags => _storyFlags;

        public IReadOnlyCollection<string> CollectedEvidenceIds => _collectedEvidenceIds;

        public IReadOnlyCollection<string> UnlockedMemoryIds => _unlockedMemoryIds;

        public IReadOnlyCollection<string> CompletedDeductionIds => _completedDeductionIds;

        public void SetCurrentCase(string caseId)
        {
            CurrentCaseId = RequireId(caseId, nameof(caseId));
        }

        public void ClearCurrentCase()
        {
            CurrentCaseId = string.Empty;
        }

        public void SetLocation(string locationId)
        {
            LocationId = RequireId(locationId, nameof(locationId));
        }

        public void ClearLocation()
        {
            LocationId = string.Empty;
        }

        public void SetCheckpoint(string checkpointId)
        {
            CheckpointId = RequireId(checkpointId, nameof(checkpointId));
        }

        public void ClearCheckpoint()
        {
            CheckpointId = string.Empty;
        }

        public bool SetStoryFlag(string flagId)
        {
            return _storyFlags.Add(RequireId(flagId, nameof(flagId)));
        }

        public bool ClearStoryFlag(string flagId)
        {
            return _storyFlags.Remove(RequireId(flagId, nameof(flagId)));
        }

        public bool HasStoryFlag(string flagId)
        {
            return _storyFlags.Contains(RequireId(flagId, nameof(flagId)));
        }

        public bool CollectEvidence(string evidenceId)
        {
            return _collectedEvidenceIds.Add(RequireId(evidenceId, nameof(evidenceId)));
        }

        public bool HasCollectedEvidence(string evidenceId)
        {
            return _collectedEvidenceIds.Contains(RequireId(evidenceId, nameof(evidenceId)));
        }

        public bool UnlockMemory(string memoryId)
        {
            return _unlockedMemoryIds.Add(RequireId(memoryId, nameof(memoryId)));
        }

        public bool HasUnlockedMemory(string memoryId)
        {
            return _unlockedMemoryIds.Contains(RequireId(memoryId, nameof(memoryId)));
        }

        public bool CompleteDeduction(string deductionId)
        {
            return _completedDeductionIds.Add(RequireId(deductionId, nameof(deductionId)));
        }

        public bool HasCompletedDeduction(string deductionId)
        {
            return _completedDeductionIds.Contains(RequireId(deductionId, nameof(deductionId)));
        }

        public GameStateSnapshot CreateSnapshot()
        {
            return new GameStateSnapshot
            {
                SchemaVersion = CurrentSchemaVersion,
                CurrentCaseId = CurrentCaseId,
                LocationId = LocationId,
                CheckpointId = CheckpointId,
                StoryFlags = SortedCopy(_storyFlags),
                CollectedEvidenceIds = SortedCopy(_collectedEvidenceIds),
                UnlockedMemoryIds = SortedCopy(_unlockedMemoryIds),
                CompletedDeductionIds = SortedCopy(_completedDeductionIds)
            };
        }

        public static GameState FromSnapshot(GameStateSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (snapshot.SchemaVersion != CurrentSchemaVersion)
            {
                throw new NotSupportedException(
                    $"Game state schema {snapshot.SchemaVersion} is not supported; expected {CurrentSchemaVersion}.");
            }

            var state = new GameState();
            RestoreOptionalId(snapshot.CurrentCaseId, state.SetCurrentCase);
            RestoreOptionalId(snapshot.LocationId, state.SetLocation);
            RestoreOptionalId(snapshot.CheckpointId, state.SetCheckpoint);
            RestoreIds(snapshot.StoryFlags, state.SetStoryFlag);
            RestoreIds(snapshot.CollectedEvidenceIds, state.CollectEvidence);
            RestoreIds(snapshot.UnlockedMemoryIds, state.UnlockMemory);
            RestoreIds(snapshot.CompletedDeductionIds, state.CompleteDeduction);
            return state;
        }

        private static string RequireId(string id, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("A stable content ID is required.", parameterName);
            }

            return id.Trim();
        }

        private static List<string> SortedCopy(IEnumerable<string> values)
        {
            return values.OrderBy(value => value, StringComparer.Ordinal).ToList();
        }

        private static void RestoreOptionalId(string id, Action<string> setter)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                setter(id);
            }
        }

        private static void RestoreIds(IEnumerable<string> ids, Func<string, bool> add)
        {
            if (ids == null)
            {
                return;
            }

            foreach (string id in ids)
            {
                add(id);
            }
        }
    }
}
