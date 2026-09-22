using System;
using NullPointer.Core;

namespace NullPointer.Progression
{
    public sealed class LoadedSessionRecovery
    {
        private readonly IGameModeService _gameModes;
        private readonly CheckpointService _checkpoints;

        public LoadedSessionRecovery(IGameModeService gameModes, CheckpointService checkpoints)
        {
            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _checkpoints = checkpoints ?? throw new ArgumentNullException(nameof(checkpoints));
        }

        public CheckpointData Restore(string fallbackCheckpointId, out bool usedFallback)
        {
            _gameModes.SetMode(GameMode.Gameplay);
            return _checkpoints.RestoreLoadedCheckpoint(fallbackCheckpointId, out usedFallback);
        }
    }
}
