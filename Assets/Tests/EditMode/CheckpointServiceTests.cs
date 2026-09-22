using NUnit.Framework;
using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Progression;
using NullPointer.Save;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class CheckpointServiceTests
    {
        [Test]
        public void Activate_UpdatesSafeLocationAndPersistsCheckpoint()
        {
            LocationData location = ScriptableObject.CreateInstance<LocationData>();
            var locationSerialized = new SerializedObject(location);
            locationSerialized.FindProperty("_stableId").stringValue = "location.test.room";
            locationSerialized.FindProperty("_sceneName").stringValue = "SCN_Test";
            locationSerialized.ApplyModifiedPropertiesWithoutUndo();

            CheckpointData checkpoint = ScriptableObject.CreateInstance<CheckpointData>();
            var checkpointSerialized = new SerializedObject(checkpoint);
            checkpointSerialized.FindProperty("_stableId").stringValue = "checkpoint.test.entry";
            checkpointSerialized.FindProperty("_location").objectReferenceValue = location;
            checkpointSerialized.FindProperty("_spawnPointId").stringValue = "door";
            checkpointSerialized.ApplyModifiedPropertiesWithoutUndo();

            var storage = new TestSaveStorage();
            var state = new GameState();
            var service = new CheckpointService(
                state,
                new SaveManager(storage, "test"),
                new[] { checkpoint });

            SaveLoadResult result = service.Activate(checkpoint.StableId);

            Assert.That(result.Status, Is.EqualTo(SaveLoadStatus.Success));
            Assert.That(state.CheckpointId, Is.EqualTo(checkpoint.StableId));
            Assert.That(state.LocationId, Is.EqualTo(location.StableId));
            Assert.That(storage.PrimaryExists, Is.True);

            Object.DestroyImmediate(checkpoint);
            Object.DestroyImmediate(location);
        }

        [Test]
        public void RestoreLoadedCheckpoint_WithUnknownCheckpoint_UsesSafeFallback()
        {
            LocationData location = ScriptableObject.CreateInstance<LocationData>();
            var locationSerialized = new SerializedObject(location);
            locationSerialized.FindProperty("_stableId").stringValue = "location.test.safe";
            locationSerialized.FindProperty("_sceneName").stringValue = "SCN_Test";
            locationSerialized.ApplyModifiedPropertiesWithoutUndo();
            CheckpointData checkpoint = ScriptableObject.CreateInstance<CheckpointData>();
            var checkpointSerialized = new SerializedObject(checkpoint);
            checkpointSerialized.FindProperty("_stableId").stringValue = "checkpoint.test.safe";
            checkpointSerialized.FindProperty("_location").objectReferenceValue = location;
            checkpointSerialized.FindProperty("_spawnPointId").stringValue = "entry";
            checkpointSerialized.ApplyModifiedPropertiesWithoutUndo();
            var state = new GameState();
            state.SetCheckpoint("checkpoint.removed.invalid");
            state.SetLocation("location.removed.invalid");
            var service = new CheckpointService(
                state,
                new SaveManager(new TestSaveStorage(), "test"),
                new[] { checkpoint });

            CheckpointData restored = service.RestoreLoadedCheckpoint(checkpoint.StableId, out bool usedFallback);

            Assert.That(usedFallback, Is.True);
            Assert.That(restored, Is.SameAs(checkpoint));
            Assert.That(state.CheckpointId, Is.EqualTo(checkpoint.StableId));
            Assert.That(state.LocationId, Is.EqualTo(location.StableId));
            Object.DestroyImmediate(checkpoint);
            Object.DestroyImmediate(location);
        }

        [TestCase(GameMode.Inspect)]
        [TestCase(GameMode.Dialogue)]
        [TestCase(GameMode.Interrogation)]
        [TestCase(GameMode.Terminal)]
        [TestCase(GameMode.EvidenceBoard)]
        [TestCase(GameMode.Memory)]
        [TestCase(GameMode.Paused)]
        public void LoadedSessionRecovery_FromTransientMode_RestoresGameplay(GameMode persistedTransientMode)
        {
            LocationData location = ScriptableObject.CreateInstance<LocationData>();
            var locationSerialized = new SerializedObject(location);
            locationSerialized.FindProperty("_stableId").stringValue = "location.test.safe";
            locationSerialized.FindProperty("_sceneName").stringValue = "SCN_Test";
            locationSerialized.ApplyModifiedPropertiesWithoutUndo();
            CheckpointData checkpoint = ScriptableObject.CreateInstance<CheckpointData>();
            var checkpointSerialized = new SerializedObject(checkpoint);
            checkpointSerialized.FindProperty("_stableId").stringValue = "checkpoint.test.safe";
            checkpointSerialized.FindProperty("_location").objectReferenceValue = location;
            checkpointSerialized.FindProperty("_spawnPointId").stringValue = "entry";
            checkpointSerialized.ApplyModifiedPropertiesWithoutUndo();
            var state = new GameState();
            state.SetCheckpoint(checkpoint.StableId);
            var gameModes = new GameModeService(persistedTransientMode);
            var checkpoints = new CheckpointService(
                state,
                new SaveManager(new TestSaveStorage(), "test"),
                new[] { checkpoint });
            var recovery = new LoadedSessionRecovery(gameModes, checkpoints);

            CheckpointData restored = recovery.Restore(checkpoint.StableId, out bool usedFallback);

            Assert.That(gameModes.CurrentMode, Is.EqualTo(GameMode.Gameplay));
            Assert.That(restored, Is.SameAs(checkpoint));
            Assert.That(usedFallback, Is.False);
            Object.DestroyImmediate(checkpoint);
            Object.DestroyImmediate(location);
        }
    }
}
