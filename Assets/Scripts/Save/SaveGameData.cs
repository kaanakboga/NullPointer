using System;
using NullPointer.Core;

namespace NullPointer.Save
{
    [Serializable]
    public sealed class SaveGameData
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public string GameVersion = string.Empty;
        public string SavedAtUtc = string.Empty;
        public GameStateSnapshot State = new GameStateSnapshot();
    }
}
