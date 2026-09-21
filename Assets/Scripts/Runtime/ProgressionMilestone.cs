using System;
using NullPointer.Progression;
using UnityEngine;

namespace NullPointer.Runtime
{
    [Serializable]
    public sealed class ProgressionMilestone
    {
        [SerializeField] private ProgressionTriggerKind _triggerKind;
        [SerializeField] private string _triggerId = string.Empty;
        [SerializeField] private ObjectiveData _objectiveToComplete;
        [SerializeField] private ObjectiveData _objectiveToStart;
        [SerializeField] private CheckpointData _checkpoint;
        [SerializeField] private string _storyFlagToSet = string.Empty;
        [SerializeField] private string _chapterIdToComplete = string.Empty;

        public ProgressionMilestone(
            ProgressionTriggerKind triggerKind,
            string triggerId,
            ObjectiveData objectiveToComplete = null,
            ObjectiveData objectiveToStart = null,
            CheckpointData checkpoint = null,
            string storyFlagToSet = "",
            string chapterIdToComplete = "")
        {
            _triggerKind = triggerKind;
            _triggerId = triggerId ?? string.Empty;
            _objectiveToComplete = objectiveToComplete;
            _objectiveToStart = objectiveToStart;
            _checkpoint = checkpoint;
            _storyFlagToSet = storyFlagToSet ?? string.Empty;
            _chapterIdToComplete = chapterIdToComplete ?? string.Empty;
        }

        public ProgressionTriggerKind TriggerKind => _triggerKind;
        public string TriggerId => _triggerId;
        public ObjectiveData ObjectiveToComplete => _objectiveToComplete;
        public ObjectiveData ObjectiveToStart => _objectiveToStart;
        public CheckpointData Checkpoint => _checkpoint;
        public string StoryFlagToSet => _storyFlagToSet;
        public string ChapterIdToComplete => _chapterIdToComplete;
    }
}
