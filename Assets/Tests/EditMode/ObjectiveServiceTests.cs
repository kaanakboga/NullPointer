using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Progression;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class ObjectiveServiceTests
    {
        [Test]
        public void Complete_MovesObjectiveFromActiveToCompletedAndRejectsDuplicates()
        {
            ObjectiveData objective = ScriptableObject.CreateInstance<ObjectiveData>();
            var serialized = new SerializedObject(objective);
            serialized.FindProperty("_stableId").stringValue = "objective.test.inspect";
            serialized.FindProperty("_title").stringValue = "Inspect";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var state = new GameState();
            var service = new ObjectiveService(state, new[] { objective });

            Assert.That(service.Start(objective.StableId), Is.True);
            Assert.That(service.Start(objective.StableId), Is.False);
            Assert.That(service.Complete(objective.StableId), Is.True);
            Assert.That(service.Complete(objective.StableId), Is.False);
            Assert.That(service.GetActive(), Is.Empty);
            Assert.That(service.GetCompleted().Count, Is.EqualTo(1));

            Object.DestroyImmediate(objective);
        }
    }
}
