using System;
using System.Collections.Generic;

namespace NullPointer.Core
{
    [Serializable]
    public sealed class GameStateSnapshot
    {
        public int SchemaVersion = GameState.CurrentSchemaVersion;
        public string CurrentCaseId = string.Empty;
        public string LocationId = string.Empty;
        public string CheckpointId = string.Empty;
        public List<string> StoryFlags = new List<string>();
        public List<string> CollectedEvidenceIds = new List<string>();
        public List<string> UnlockedMemoryIds = new List<string>();
        public List<string> CompletedDeductionIds = new List<string>();
    }
}
