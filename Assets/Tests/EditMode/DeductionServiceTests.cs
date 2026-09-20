using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Evidence;
using NUnit.Framework;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class DeductionServiceTests
    {
        private EvidenceData _firstEvidence;
        private EvidenceData _secondEvidence;
        private DeductionData _deduction;

        [SetUp]
        public void SetUp()
        {
            _firstEvidence = AuthoredAssetTestFactory.CreateEvidence("evidence.test.first");
            _secondEvidence = AuthoredAssetTestFactory.CreateEvidence("evidence.test.second");
            _deduction = AuthoredAssetTestFactory.CreateDeduction(
                "deduction.test.combination",
                new[] { _firstEvidence.StableId, _secondEvidence.StableId },
                "flag.test.deduced");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_firstEvidence);
            Object.DestroyImmediate(_secondEvidence);
            Object.DestroyImmediate(_deduction);
        }

        [Test]
        public void Attempt_WithMissingEvidence_ReportsRequirementsWithoutCompleting()
        {
            var state = new GameState();
            var evidenceService = new EvidenceService(state, new[] { _firstEvidence, _secondEvidence });
            var service = new DeductionService(state, evidenceService, new[] { _deduction });
            evidenceService.Collect(_firstEvidence);

            DeductionAttemptResult result = service.Attempt(_deduction.StableId);

            Assert.That(result.Status, Is.EqualTo(DeductionAttemptStatus.MissingEvidence));
            Assert.That(result.MissingEvidenceIds, Is.EquivalentTo(new[] { _secondEvidence.StableId }));
            Assert.That(state.HasCompletedDeduction(_deduction.StableId), Is.False);
        }

        [Test]
        public void Attempt_WithAllEvidence_CompletesOnceAndPersistsInSnapshot()
        {
            var state = new GameState();
            var evidenceService = new EvidenceService(state, new[] { _firstEvidence, _secondEvidence });
            var service = new DeductionService(state, evidenceService, new[] { _deduction });
            evidenceService.Collect(_firstEvidence);
            evidenceService.Collect(_secondEvidence);

            DeductionAttemptResult first = service.Attempt(_deduction.StableId);
            DeductionAttemptResult second = service.Attempt(_deduction.StableId);
            GameState restored = GameState.FromSnapshot(state.CreateSnapshot());

            Assert.That(first.Status, Is.EqualTo(DeductionAttemptStatus.Completed));
            Assert.That(second.Status, Is.EqualTo(DeductionAttemptStatus.AlreadyCompleted));
            Assert.That(restored.HasCompletedDeduction(_deduction.StableId), Is.True);
            Assert.That(restored.HasStoryFlag("flag.test.deduced"), Is.True);
        }
    }
}
