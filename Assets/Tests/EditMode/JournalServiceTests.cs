using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Evidence;
using NullPointer.Journal;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class JournalServiceTests
    {
        [Test]
        public void GetEntries_OnlyReturnsEntriesWhoseStateConditionsAreMet()
        {
            JournalEntryData question = ScriptableObject.CreateInstance<JournalEntryData>();
            var serialized = new SerializedObject(question);
            serialized.FindProperty("_stableId").stringValue = "journal.question.test";
            serialized.FindProperty("_section").enumValueIndex = (int)JournalSection.Questions;
            serialized.FindProperty("_requiredStoryFlag").stringValue = "flag.test.unlocked";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var state = new GameState();
            var evidence = new EvidenceService(state, System.Array.Empty<EvidenceData>());
            var service = new JournalService(state, evidence, new[] { question });

            Assert.That(service.GetEntries(JournalSection.Questions), Is.Empty);
            state.SetStoryFlag("flag.test.unlocked");
            Assert.That(service.GetEntries(JournalSection.Questions).Count, Is.EqualTo(1));

            Object.DestroyImmediate(question);
        }
    }
}
