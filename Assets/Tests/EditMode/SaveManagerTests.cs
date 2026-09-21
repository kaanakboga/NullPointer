using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Save;

namespace NullPointer.Tests.EditMode
{
    public sealed class SaveManagerTests
    {
        [Test]
        public void SaveAndLoad_RoundTripPreservesProgressAndNormalizesDuplicates()
        {
            var storage = new TestSaveStorage();
            var manager = new SaveManager(storage, "test");
            var state = new GameState();
            state.SetLocation("location.test.room");
            state.SetCheckpoint("checkpoint.test.entry");
            state.CollectEvidence("evidence.test.note");
            state.CollectEvidence("evidence.test.note");
            state.StartObjective("objective.test.inspect");
            state.RecordDialogueProgress("dialogue.test.spoken");
            state.CompleteChapter("chapter.test.one");

            SaveLoadResult saveResult = manager.Save(state);
            SaveLoadResult loadResult = manager.Load();

            Assert.That(saveResult.Status, Is.EqualTo(SaveLoadStatus.Success));
            Assert.That(loadResult.Status, Is.EqualTo(SaveLoadStatus.Success));
            Assert.That(loadResult.GameState.LocationId, Is.EqualTo("location.test.room"));
            Assert.That(loadResult.GameState.CollectedEvidenceIds, Has.Count.EqualTo(1));
            Assert.That(loadResult.GameState.IsObjectiveActive("objective.test.inspect"), Is.True);
            Assert.That(loadResult.GameState.HasDialogueProgress("dialogue.test.spoken"), Is.True);
            Assert.That(loadResult.GameState.HasCompletedChapter("chapter.test.one"), Is.True);
        }

        [Test]
        public void Load_WhenFileIsMissing_ReturnsMissingFile()
        {
            var manager = new SaveManager(new TestSaveStorage(), "test");

            Assert.That(manager.Load().Status, Is.EqualTo(SaveLoadStatus.MissingFile));
            Assert.That(manager.HasValidSave, Is.False);
        }

        [Test]
        public void Load_WhenJsonIsCorrupted_ReturnsCorruptedWithoutState()
        {
            var storage = new TestSaveStorage { Contents = "{ definitely not json" };
            var manager = new SaveManager(storage, "test");

            SaveLoadResult result = manager.Load();

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.Corrupted));
            Assert.That(result.GameState, Is.Null);
        }

        [Test]
        public void Load_WhenSaveSchemaIsUnsupported_ReturnsUnsupportedSchema()
        {
            var storage = new TestSaveStorage
            {
                Contents = "{\"SchemaVersion\":99,\"State\":{\"SchemaVersion\":2}}"
            };
            var manager = new SaveManager(storage, "test");

            Assert.That(manager.Load().Status, Is.EqualTo(SaveLoadStatus.UnsupportedSchema));
        }
    }
}
