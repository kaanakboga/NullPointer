using System.IO;
using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Save;
using UnityEngine;

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

        [Test]
        public void Load_WithValidPrimary_UsesPrimaryWithoutRecovery()
        {
            var storage = new TestSaveStorage();
            var manager = new SaveManager(storage, "test");
            var state = new GameState();
            state.SetCheckpoint("checkpoint.test.primary");
            manager.Save(state);

            SaveLoadResult result = manager.Load();

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.Success));
            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.Primary));
            Assert.That(result.GameState.CheckpointId, Is.EqualTo("checkpoint.test.primary"));
        }

        [Test]
        public void Load_WithCorruptPrimaryAndValidBackup_RecoversAndRestoresPrimary()
        {
            TestSaveStorage storage = CreateStorageWithValidBackup("checkpoint.test.backup");
            storage.PrimaryContents = "{ corrupt primary";
            var manager = new SaveManager(storage, "test");

            SaveLoadResult result = manager.Load();

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.RecoveredFromBackup));
            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.Backup));
            Assert.That(result.GameState.CheckpointId, Is.EqualTo("checkpoint.test.backup"));
            Assert.That(storage.PrimaryContents, Is.EqualTo(storage.BackupContents));
        }

        [Test]
        public void Load_WithMissingPrimaryAndValidBackup_RecoversBackup()
        {
            TestSaveStorage storage = CreateStorageWithValidBackup("checkpoint.test.backup");
            storage.PrimaryContents = null;
            var manager = new SaveManager(storage, "test");

            SaveLoadResult result = manager.Load();

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.RecoveredFromBackup));
            Assert.That(result.GameState.CheckpointId, Is.EqualTo("checkpoint.test.backup"));
            Assert.That(storage.PrimaryExists, Is.True);
        }

        [Test]
        public void Load_WithCorruptPrimaryAndCorruptBackup_ReturnsCorruptedWithoutState()
        {
            var storage = new TestSaveStorage
            {
                PrimaryContents = "{ corrupt primary",
                BackupContents = "{ corrupt backup"
            };
            var manager = new SaveManager(storage, "test");

            SaveLoadResult result = manager.Load();

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.Corrupted));
            Assert.That(result.GameState, Is.Null);
        }

        [Test]
        public void Save_WithValidPrimary_CreatesBackupOfPreviousState()
        {
            var storage = new TestSaveStorage();
            var manager = new SaveManager(storage, "test");
            var first = new GameState();
            first.SetCheckpoint("checkpoint.test.first");
            manager.Save(first);
            var second = new GameState();
            second.SetCheckpoint("checkpoint.test.second");

            manager.Save(second);
            string current = storage.PrimaryContents;
            storage.PrimaryContents = null;
            SaveLoadResult backup = manager.Load();

            Assert.That(current, Is.Not.EqualTo(storage.BackupContents));
            Assert.That(backup.Status, Is.EqualTo(SaveLoadStatus.RecoveredFromBackup));
            Assert.That(backup.GameState.CheckpointId, Is.EqualTo("checkpoint.test.first"));
        }

        [Test]
        public void Save_WhenAtomicWriteFails_PreservesPrimaryAndBackup()
        {
            TestSaveStorage storage = CreateStorageWithValidBackup("checkpoint.test.backup");
            string primaryBefore = storage.PrimaryContents;
            string backupBefore = storage.BackupContents;
            storage.FailNextWrite = true;
            var manager = new SaveManager(storage, "test");
            var replacement = new GameState();
            replacement.SetCheckpoint("checkpoint.test.replacement");

            SaveLoadResult result = manager.Save(replacement);

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.StorageFailure));
            Assert.That(storage.PrimaryContents, Is.EqualTo(primaryBefore));
            Assert.That(storage.BackupContents, Is.EqualTo(backupBefore));
        }

        [Test]
        public void Save_WithCorruptPrimary_PreservesValidBackupDuringReplacement()
        {
            TestSaveStorage storage = CreateStorageWithValidBackup("checkpoint.test.backup");
            string backupBefore = storage.BackupContents;
            storage.PrimaryContents = "{ corrupt primary";
            var manager = new SaveManager(storage, "test");
            var replacement = new GameState();
            replacement.SetCheckpoint("checkpoint.test.replacement");

            SaveLoadResult result = manager.Save(replacement);

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.Success));
            Assert.That(storage.BackupContents, Is.EqualTo(backupBefore));
            Assert.That(manager.Load().GameState.CheckpointId, Is.EqualTo("checkpoint.test.replacement"));
        }

        [Test]
        public void Load_LegacyVersionOneFixture_MigratesStableIdsDeterministically()
        {
            string fixturePath = Path.Combine(
                Application.dataPath,
                "Tests/EditMode/Fixtures/Save/save-schema-1-state-schema-1.json");
            var storage = new TestSaveStorage { PrimaryContents = File.ReadAllText(fixturePath) };
            var manager = new SaveManager(storage, "test");

            SaveLoadResult result = manager.Load();

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.Success));
            Assert.That(result.WasMigrated, Is.True);
            Assert.That(result.GameState.CheckpointId, Is.EqualTo("checkpoint.ch01.mert_entrance"));
            Assert.That(result.GameState.CollectedEvidenceIds, Does.Contain("evidence.mert.photo"));
            Assert.That(result.GameState.ActiveObjectiveIds, Is.Empty);
        }

        [Test]
        public void Load_WithFuturePrimary_DoesNotFallBackToOlderBackup()
        {
            TestSaveStorage storage = CreateStorageWithValidBackup("checkpoint.test.backup");
            storage.PrimaryContents = "{\"SchemaVersion\":99,\"State\":{\"SchemaVersion\":2}}";
            var manager = new SaveManager(storage, "test");

            SaveLoadResult result = manager.Load();

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.UnsupportedSchema));
            Assert.That(result.GameState, Is.Null);
            Assert.That(result.Source, Is.EqualTo(SaveLoadSource.Primary));
        }

        private static TestSaveStorage CreateStorageWithValidBackup(string checkpointId)
        {
            var storage = new TestSaveStorage();
            var manager = new SaveManager(storage, "test");
            var state = new GameState();
            state.SetCheckpoint(checkpointId);
            manager.Save(state);
            storage.BackupContents = storage.PrimaryContents;
            return storage;
        }
    }
}
