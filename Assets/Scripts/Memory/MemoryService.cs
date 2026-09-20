using System;
using NullPointer.Core;

namespace NullPointer.Memory
{
    public sealed class MemoryService
    {
        private readonly GameState _gameState;

        public MemoryService(GameState gameState)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        }

        public event Action<MemoryUnlocked> MemoryUnlocked;

        public bool Unlock(MemoryData memory)
        {
            if (memory == null || string.IsNullOrWhiteSpace(memory.StableId))
            {
                return false;
            }

            if (!_gameState.UnlockMemory(memory.StableId))
            {
                return false;
            }

            MemoryUnlocked?.Invoke(new MemoryUnlocked(memory));
            return true;
        }

        public bool IsUnlocked(string memoryId)
        {
            return !string.IsNullOrWhiteSpace(memoryId) && _gameState.HasUnlockedMemory(memoryId);
        }
    }
}
