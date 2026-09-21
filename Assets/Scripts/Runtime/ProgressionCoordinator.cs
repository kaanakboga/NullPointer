using System;
using NullPointer.Deduction;
using NullPointer.Evidence;
using NullPointer.Memory;
using NullPointer.Terminal;
using UnityEngine;

namespace NullPointer.Runtime
{
    [AddComponentMenu("Null Pointer/Runtime/Progression Coordinator")]
    public sealed class ProgressionCoordinator : MonoBehaviour
    {
        [SerializeField] private TerminalController _terminalController;
        [SerializeField] private ProgressionMilestone[] _milestones = Array.Empty<ProgressionMilestone>();
        private GameApplication _application;

        public void Configure(TerminalController terminalController, ProgressionMilestone[] milestones)
        {
            _terminalController = terminalController;
            _milestones = milestones ?? Array.Empty<ProgressionMilestone>();
        }

        public void Initialize(GameApplication application)
        {
            Unsubscribe();
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _application.EvidenceService.EvidenceCollected += OnEvidenceCollected;
            _application.MemoryService.MemoryUnlocked += OnMemoryUnlocked;
            _application.DeductionService.DeductionCompleted += OnDeductionCompleted;
            if (_terminalController != null)
            {
                _terminalController.EntryOpened += OnTerminalEntryOpened;
            }

            Apply(ProgressionTriggerKind.SceneEntered, _application.GameState.LocationId);
        }

        private void OnEvidenceCollected(EvidenceCollected change) =>
            Apply(ProgressionTriggerKind.EvidenceCollected, change.EvidenceId);

        private void OnMemoryUnlocked(MemoryUnlocked change) =>
            Apply(ProgressionTriggerKind.MemoryUnlocked, change.MemoryId);

        private void OnDeductionCompleted(DeductionCompleted change) =>
            Apply(ProgressionTriggerKind.DeductionCompleted, change.DeductionId);

        private void OnTerminalEntryOpened(TerminalEntryOpened change) =>
            Apply(ProgressionTriggerKind.TerminalEntryOpened, change.Entry.EntryId);

        private void Apply(ProgressionTriggerKind kind, string triggerId)
        {
            foreach (ProgressionMilestone milestone in _milestones)
            {
                if (milestone == null || milestone.TriggerKind != kind ||
                    !string.Equals(milestone.TriggerId, triggerId, StringComparison.Ordinal))
                {
                    continue;
                }

                if (milestone.ObjectiveToComplete != null)
                {
                    _application.ObjectiveService.Complete(milestone.ObjectiveToComplete.StableId);
                }

                if (milestone.ObjectiveToStart != null)
                {
                    _application.ObjectiveService.Start(milestone.ObjectiveToStart.StableId);
                }

                if (!string.IsNullOrWhiteSpace(milestone.StoryFlagToSet))
                {
                    _application.GameState.SetStoryFlag(milestone.StoryFlagToSet);
                }

                if (!string.IsNullOrWhiteSpace(milestone.ChapterIdToComplete))
                {
                    _application.ChapterProgressionService.TryComplete(
                        milestone.ChapterIdToComplete,
                        new[] { triggerId },
                        milestone.StoryFlagToSet);
                    _application.SaveGame();
                }

                if (milestone.Checkpoint != null &&
                    !string.Equals(
                        _application.GameState.CheckpointId,
                        milestone.Checkpoint.StableId,
                        StringComparison.Ordinal))
                {
                    _application.CheckpointService.Activate(milestone.Checkpoint.StableId);
                }
            }
        }

        private void Unsubscribe()
        {
            if (_application != null)
            {
                _application.EvidenceService.EvidenceCollected -= OnEvidenceCollected;
                _application.MemoryService.MemoryUnlocked -= OnMemoryUnlocked;
                _application.DeductionService.DeductionCompleted -= OnDeductionCompleted;
            }

            if (_terminalController != null)
            {
                _terminalController.EntryOpened -= OnTerminalEntryOpened;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
