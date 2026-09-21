using System.Linq;
using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Evidence;
using NullPointer.Memory;
using NullPointer.Progression;
using NullPointer.Runtime;
using NullPointer.Save;
using UnityEditor;

namespace NullPointer.Tests.EditMode
{
    public sealed class ChapterOneHappyPathTests
    {
        [Test]
        public void CompleteOpeningHappyPath_PersistsChapterOneStateAtSafeCheckpoint()
        {
            ContentCatalog catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(
                "Assets/Data/CAT_OpeningContent.asset");
            MemoryData photographMemory = AssetDatabase.LoadAssetAtPath<MemoryData>(
                "Assets/Data/Memories/MEM_MertPhotoGlitch.asset");
            var storage = new TestSaveStorage();
            var save = new SaveManager(storage, "test");
            var state = new GameState();
            var evidence = new EvidenceService(state, catalog.Evidence);
            var deductions = new DeductionService(state, evidence, catalog.Deductions);
            var memories = new MemoryService(state);
            var objectives = new ObjectiveService(state, catalog.Objectives);
            var checkpoints = new CheckpointService(state, save, catalog.Checkpoints);
            var chapters = new ChapterProgressionService(state);

            state.SetCurrentCase("case.mert.suspicious_death");
            Assert.That(checkpoints.Activate("checkpoint.ch01.eren_start", false).IsSuccess, Is.True);
            objectives.Start("objective.ch01.inspect_dispatch");
            state.SetStoryFlag("flag.dispatch.mert_assignment_received");
            state.RecordDialogueProgress("terminal.read.terminal.eren.dispatch.dispatch_0317");
            objectives.Complete("objective.ch01.inspect_dispatch");
            objectives.Start("objective.ch01.go_to_mert");
            checkpoints.Activate("checkpoint.ch01.dispatch_complete", false);

            checkpoints.Activate("checkpoint.ch01.mert_entrance", false);
            objectives.Complete("objective.ch01.go_to_mert");
            objectives.Start("objective.ch01.investigate_scene");
            foreach (EvidenceData item in catalog.Evidence)
            {
                Assert.That(evidence.Collect(item.StableId), Is.Not.EqualTo(EvidenceCollectionStatus.UnknownEvidence));
            }

            memories.Unlock(photographMemory);
            state.RecordDialogueProgress("terminal.read.terminal.mert.personal.security_access_0251");
            Assert.That(
                deductions.Attempt("deduction.mert.suicide_timeline_inconsistent").Status,
                Is.EqualTo(DeductionAttemptStatus.MissingPrerequisite));
            Assert.That(
                deductions.Attempt("deduction.mert.postmortem_terminal").Status,
                Is.EqualTo(DeductionAttemptStatus.Completed));
            checkpoints.Activate("checkpoint.ch01.first_deduction", false);
            Assert.That(
                deductions.Attempt("deduction.mert.suicide_timeline_inconsistent").Status,
                Is.EqualTo(DeductionAttemptStatus.Completed));
            Assert.That(
                deductions.Attempt("deduction.mert.locked_room_unreliable").Status,
                Is.EqualTo(DeductionAttemptStatus.Completed));
            objectives.Complete("objective.ch01.compare_evidence");
            Assert.That(
                chapters.TryComplete(
                    "chapter.01",
                    new[] { "deduction.mert.locked_room_unreliable" },
                    "flag.ch01.completed"),
                Is.True);
            Assert.That(save.Save(state).IsSuccess, Is.True);

            SaveLoadResult restored = save.Load();
            Assert.That(restored.IsSuccess, Is.True);
            Assert.That(restored.GameState.LocationId, Is.EqualTo("location.mert.apartment"));
            Assert.That(restored.GameState.CheckpointId, Is.EqualTo("checkpoint.ch01.first_deduction"));
            Assert.That(restored.GameState.HasUnlockedMemory(photographMemory.StableId), Is.True);
            Assert.That(restored.GameState.CompletedDeductionIds.Count, Is.EqualTo(3));
            Assert.That(restored.GameState.HasCompletedChapter("chapter.01"), Is.True);
            Assert.That(restored.GameState.HasStoryFlag("flag.ch01.completed"), Is.True);
            Assert.That(restored.GameState.DialogueProgressIds.Any(), Is.True);
        }

        [Test]
        public void ChapterCompletion_WhenRequiredDeductionIsMissing_DoesNotCrossBoundary()
        {
            var state = new GameState();
            var chapters = new ChapterProgressionService(state);

            bool completed = chapters.TryComplete(
                "chapter.01",
                new[] { "deduction.mert.locked_room_unreliable" },
                "flag.ch01.completed");

            Assert.That(completed, Is.False);
            Assert.That(state.CompletedChapterIds, Is.Empty);
            Assert.That(state.StoryFlags, Is.Empty);
        }
    }
}
