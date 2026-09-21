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
            Assert.That(storage.Exists, Is.True);

            Object.DestroyImmediate(checkpoint);
            Object.DestroyImmediate(location);
        }
    }
}
