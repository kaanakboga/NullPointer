using NullPointer.Core;
using NullPointer.Dialogue;
using NUnit.Framework;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class DialogueRunnerTests
    {
        [Test]
        public void Advance_LinearDialogue_ReachesCompletion()
        {
            DialogueData dialogue = AuthoredAssetTestFactory.CreateLinearDialogue();
            try
            {
                var runner = new DialogueRunner(new GameState());
                int completedCount = 0;
                runner.Completed += () => completedCount++;

                bool started = runner.Start(dialogue);
                runner.Advance();
                runner.Advance();

                Assert.That(started, Is.True);
                Assert.That(runner.IsRunning, Is.False);
                Assert.That(completedCount, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(dialogue);
            }
        }

        [Test]
        public void Advance_WithBrokenNextNode_EndsSafelyWithDiagnostic()
        {
            DialogueData dialogue = AuthoredAssetTestFactory.CreateLinearDialogue("missing");
            try
            {
                var runner = new DialogueRunner(new GameState());
                runner.Start(dialogue);

                bool advanced = runner.Advance();

                Assert.That(advanced, Is.False);
                Assert.That(runner.IsRunning, Is.False);
                StringAssert.Contains("missing", runner.LastError);
            }
            finally
            {
                Object.DestroyImmediate(dialogue);
            }
        }

        [Test]
        public void Choices_FilterByCollectedEvidenceAndFollowSelectedBranch()
        {
            DialogueData dialogue = AuthoredAssetTestFactory.CreateConditionalChoiceDialogue();
            try
            {
                var state = new GameState();
                var runner = new DialogueRunner(state);

                runner.Start(dialogue);
                Assert.That(runner.AvailableChoices, Has.Count.EqualTo(1));
                Assert.That(runner.AvailableChoices[0].Text, Is.EqualTo("Fallback route"));

                state.CollectEvidence("evidence.test.key");
                runner.Start(dialogue);
                Assert.That(runner.AvailableChoices, Has.Count.EqualTo(2));

                runner.SelectChoice(0);
                Assert.That(runner.CurrentNode.Text, Is.EqualTo("Evidence branch"));
            }
            finally
            {
                Object.DestroyImmediate(dialogue);
            }
        }
    }
}
