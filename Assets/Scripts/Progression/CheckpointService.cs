using System;
using System.Collections.Generic;
using NullPointer.Core;
using NullPointer.Save;

namespace NullPointer.Progression
{
    public sealed class CheckpointService
    {
        private readonly GameState _gameState;
        private readonly SaveManager _saveManager;
        private readonly Dictionary<string, CheckpointData> _catalog;

        public CheckpointService(
            GameState gameState,
            SaveManager saveManager,
            IEnumerable<CheckpointData> checkpoints)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _saveManager = saveManager ?? throw new ArgumentNullException(nameof(saveManager));
            _catalog = new Dictionary<string, CheckpointData>(StringComparer.Ordinal);
            foreach (CheckpointData checkpoint in checkpoints ?? Array.Empty<CheckpointData>())
            {
                if (checkpoint != null && !_catalog.TryAdd(checkpoint.StableId, checkpoint))
                {
                    throw new ArgumentException($"Duplicate checkpoint ID '{checkpoint.StableId}'.", nameof(checkpoints));
                }
            }
        }

        public event Action<CheckpointData> CheckpointActivated;

        public CheckpointData Current => Resolve(_gameState.CheckpointId);

        public CheckpointData Resolve(string checkpointId)
        {
            return !string.IsNullOrWhiteSpace(checkpointId) &&
                   _catalog.TryGetValue(checkpointId.Trim(), out CheckpointData checkpoint)
                ? checkpoint
                : null;
        }

        public SaveLoadResult Activate(string checkpointId, bool saveImmediately = true)
        {
            CheckpointData checkpoint = Resolve(checkpointId);
            if (checkpoint == null || checkpoint.Location == null || string.IsNullOrWhiteSpace(checkpoint.SpawnPointId))
            {
                return new SaveLoadResult(SaveLoadStatus.Corrupted, null, $"Checkpoint '{checkpointId}' is invalid.");
            }

            _gameState.SetCheckpoint(checkpoint.StableId);
            _gameState.SetLocation(checkpoint.Location.StableId);
            CheckpointActivated?.Invoke(checkpoint);
            return saveImmediately
                ? _saveManager.Save(_gameState)
                : new SaveLoadResult(SaveLoadStatus.Success, _gameState, "Checkpoint activated.");
        }
    }
}
