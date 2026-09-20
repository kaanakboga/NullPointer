using NullPointer.Core;
using NullPointer.Memory;
using NUnit.Framework;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class MemoryServiceTests
    {
        [Test]
        public void Unlock_SameMemoryTwice_IsRecordedAndEmittedOnce()
        {
            MemoryData memory = AuthoredAssetTestFactory.CreateMemory("memory.test.glitch");
            try
            {
                var state = new GameState();
                var service = new MemoryService(state);
                int eventCount = 0;
                service.MemoryUnlocked += _ => eventCount++;

                bool first = service.Unlock(memory);
                bool second = service.Unlock(memory);

                Assert.That(first, Is.True);
                Assert.That(second, Is.False);
                Assert.That(state.HasUnlockedMemory(memory.StableId), Is.True);
                Assert.That(eventCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(memory);
            }
        }
    }
}
