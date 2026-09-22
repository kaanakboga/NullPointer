using System;
using System.IO;
using NUnit.Framework;
using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Progression;
using NullPointer.Save;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class SaveRelaunchTests
    {
        private string _directory;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "NullPointer.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_directory);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Test]
        public void NewManagerInstance_LoadsCheckpointAndProgressFromDisk_ThenRestoresGameplay()
        {
            string savePath = Path.Combine(_directory, "save.json");
            var initialState = new GameState();
            initialState.SetCurrentCase("case.chapter1");
            initialState.SetLocation("location.test.room");
            initialState.SetCheckpoint("checkpoint.test.room");
            initialState.SetStoryFlag("flag.test.called");
            initialState.CollectEvidence("evidence.test.receipt");
            initialState.UnlockMemory("memory.test.first");
            initialState.CompleteDeduction("deduction.test.route");
            initialState.StartObjective("objective.test.leave");

            var firstProcessManager = new SaveManager(new FileSaveStorage(savePath), "test");
            Assert.That(firstProcessManager.Save(initialState).IsSuccess, Is.True);

            // A fresh manager and storage instance model a new executable process.
            var relaunchedManager = new SaveManager(new FileSaveStorage(savePath), "test");
            SaveLoadResult loaded = relaunchedManager.Load();

            Assert.That(loaded.Status, Is.EqualTo(SaveLoadStatus.Success));
            Assert.That(loaded.Source, Is.EqualTo(SaveLoadSource.Primary));
            Assert.That(loaded.GameState.CurrentCaseId, Is.EqualTo("case.chapter1"));
            Assert.That(loaded.GameState.HasStoryFlag("flag.test.called"), Is.True);
            Assert.That(loaded.GameState.HasCollectedEvidence("evidence.test.receipt"), Is.True);
            Assert.That(loaded.GameState.HasUnlockedMemory("memory.test.first"), Is.True);
            Assert.That(loaded.GameState.HasCompletedDeduction("deduction.test.route"), Is.True);
            Assert.That(loaded.GameState.IsObjectiveActive("objective.test.leave"), Is.True);

            LocationData location = CreateLocation();
            CheckpointData checkpoint = CreateCheckpoint(location);
            var gameModes = new GameModeService(GameMode.Paused);
            var checkpoints = new CheckpointService(loaded.GameState, relaunchedManager, new[] { checkpoint });
            var recovery = new LoadedSessionRecovery(gameModes, checkpoints);

            CheckpointData restored = recovery.Restore(checkpoint.StableId, out bool usedFallback);

            Assert.That(usedFallback, Is.False);
            Assert.That(restored, Is.SameAs(checkpoint));
            Assert.That(gameModes.CurrentMode, Is.EqualTo(GameMode.Gameplay));
            Assert.That(loaded.GameState.LocationId, Is.EqualTo(location.StableId));

            UnityEngine.Object.DestroyImmediate(checkpoint);
            UnityEngine.Object.DestroyImmediate(location);
        }

        private static LocationData CreateLocation()
        {
            LocationData location = ScriptableObject.CreateInstance<LocationData>();
            var serialized = new SerializedObject(location);
            serialized.FindProperty("_stableId").stringValue = "location.test.room";
            serialized.FindProperty("_sceneName").stringValue = "SCN_Test";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return location;
        }

        private static CheckpointData CreateCheckpoint(LocationData location)
        {
            CheckpointData checkpoint = ScriptableObject.CreateInstance<CheckpointData>();
            var serialized = new SerializedObject(checkpoint);
            serialized.FindProperty("_stableId").stringValue = "checkpoint.test.room";
            serialized.FindProperty("_location").objectReferenceValue = location;
            serialized.FindProperty("_spawnPointId").stringValue = "entry";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return checkpoint;
        }
    }
}
