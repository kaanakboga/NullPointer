using NullPointer.Core;
using NullPointer.Evidence;
using NUnit.Framework;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    public sealed class EvidenceServiceTests
    {
        private EvidenceData _evidence;

        [SetUp]
        public void SetUp()
        {
            _evidence = AuthoredAssetTestFactory.CreateEvidence("evidence.test.item");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_evidence);
        }

        [Test]
        public void Collect_KnownEvidence_UpdatesGameStateAndRaisesEvent()
        {
            var state = new GameState();
            var service = new EvidenceService(state, new[] { _evidence });
            int eventCount = 0;
            service.EvidenceCollected += _ => eventCount++;

            EvidenceCollectionStatus result = service.Collect(_evidence.StableId);

            Assert.That(result, Is.EqualTo(EvidenceCollectionStatus.Collected));
            Assert.That(state.HasCollectedEvidence(_evidence.StableId), Is.True);
            Assert.That(eventCount, Is.EqualTo(1));
        }

        [Test]
        public void Collect_SameEvidenceTwice_IsDuplicateSafeAndRaisesOneEvent()
        {
            var state = new GameState();
            var service = new EvidenceService(state, new[] { _evidence });
            int eventCount = 0;
            service.EvidenceCollected += _ => eventCount++;

            service.Collect(_evidence.StableId);
            EvidenceCollectionStatus secondResult = service.Collect(_evidence.StableId);

            Assert.That(secondResult, Is.EqualTo(EvidenceCollectionStatus.AlreadyCollected));
            Assert.That(state.CollectedEvidenceIds, Has.Count.EqualTo(1));
            Assert.That(eventCount, Is.EqualTo(1));
        }
    }
}
