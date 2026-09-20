using System;
using System.Collections.Generic;
using NUnit.Framework;
using NullPointer.Core;

namespace NullPointer.Tests.EditMode
{
    public sealed class GameStateTests
    {
        [Test]
        public void Collections_RejectDuplicatesAndRetainFirstValue()
        {
            var state = new GameState();

            Assert.That(state.SetStoryFlag("flag.test.started"), Is.True);
            Assert.That(state.SetStoryFlag("flag.test.started"), Is.False);
            Assert.That(state.CollectEvidence("evidence.test.note"), Is.True);
            Assert.That(state.CollectEvidence("evidence.test.note"), Is.False);
            Assert.That(state.UnlockMemory("memory.test.fragment"), Is.True);
            Assert.That(state.UnlockMemory("memory.test.fragment"), Is.False);
            Assert.That(state.CompleteDeduction("deduction.test.result"), Is.True);
            Assert.That(state.CompleteDeduction("deduction.test.result"), Is.False);

            Assert.That(state.StoryFlags, Has.Count.EqualTo(1));
            Assert.That(state.CollectedEvidenceIds, Has.Count.EqualTo(1));
            Assert.That(state.UnlockedMemoryIds, Has.Count.EqualTo(1));
            Assert.That(state.CompletedDeductionIds, Has.Count.EqualTo(1));
        }

        [Test]
        public void Snapshot_RoundTripPreservesStateAndNormalizesDuplicates()
        {
            var snapshot = new GameStateSnapshot
            {
                SchemaVersion = GameState.CurrentSchemaVersion,
                CurrentCaseId = "case.test.current",
                LocationId = "location.test.room",
                CheckpointId = "checkpoint.test.entry",
                StoryFlags = new List<string> { "flag.test.b", "flag.test.a", "flag.test.a" },
                CollectedEvidenceIds = new List<string> { "evidence.test.note" },
                UnlockedMemoryIds = new List<string> { "memory.test.fragment" },
                CompletedDeductionIds = new List<string> { "deduction.test.result" }
            };

            GameState restored = GameState.FromSnapshot(snapshot);
            GameStateSnapshot roundTrip = restored.CreateSnapshot();

            Assert.That(roundTrip.SchemaVersion, Is.EqualTo(GameState.CurrentSchemaVersion));
            Assert.That(roundTrip.CurrentCaseId, Is.EqualTo(snapshot.CurrentCaseId));
            Assert.That(roundTrip.LocationId, Is.EqualTo(snapshot.LocationId));
            Assert.That(roundTrip.CheckpointId, Is.EqualTo(snapshot.CheckpointId));
            Assert.That(roundTrip.StoryFlags, Is.EqualTo(new[] { "flag.test.a", "flag.test.b" }));
            Assert.That(roundTrip.CollectedEvidenceIds, Is.EqualTo(snapshot.CollectedEvidenceIds));
            Assert.That(roundTrip.UnlockedMemoryIds, Is.EqualTo(snapshot.UnlockedMemoryIds));
            Assert.That(roundTrip.CompletedDeductionIds, Is.EqualTo(snapshot.CompletedDeductionIds));
        }

        [Test]
        public void ContentMutation_WhenIdIsBlank_Throws()
        {
            var state = new GameState();

            Assert.Throws<ArgumentException>(() => state.CollectEvidence("  "));
        }
    }
}
