using System;

namespace NullPointer.Memory
{
    public readonly struct MemoryUnlocked
    {
        public MemoryUnlocked(MemoryData memory)
        {
            Memory = memory ?? throw new ArgumentNullException(nameof(memory));
        }

        public MemoryData Memory { get; }

        public string MemoryId => Memory.StableId;
    }
}
