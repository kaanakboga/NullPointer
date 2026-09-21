using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Interrogation;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class InterrogationServiceTests
    {
        [Test]
        public void PresentEvidence_IrrelevantIsNonFatalAndContradictionUnlocksBranchOnce()
        {
            InterrogationClaimData claim = ScriptableObject.CreateInstance<InterrogationClaimData>();
            var serialized = new SerializedObject(claim);
            serialized.FindProperty("_stableId").stringValue = "claim.test.timeline";
            serialized.FindProperty("_contradictionStoryFlag").stringValue = "flag.test.contradiction";
            SerializedProperty evidenceIds = serialized.FindProperty("_contradictingEvidenceIds");
            evidenceIds.arraySize = 1;
            evidenceIds.GetArrayElementAtIndex(0).stringValue = "evidence.test.log";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var state = new GameState();
            state.CollectEvidence("evidence.test.note");
            state.CollectEvidence("evidence.test.log");
            var service = new InterrogationService(state, new[] { claim });

            Assert.That(
                service.PresentEvidence(claim.StableId, "evidence.test.note").Status,
                Is.EqualTo(InterrogationAttemptStatus.IrrelevantEvidence));
            Assert.That(
                service.PresentEvidence(claim.StableId, "evidence.test.log").Status,
                Is.EqualTo(InterrogationAttemptStatus.ContradictionFound));
            Assert.That(state.HasStoryFlag("flag.test.contradiction"), Is.True);
            Assert.That(
                service.PresentEvidence(claim.StableId, "evidence.test.log").Status,
                Is.EqualTo(InterrogationAttemptStatus.AlreadyResolved));

            Object.DestroyImmediate(claim);
        }
    }
}
