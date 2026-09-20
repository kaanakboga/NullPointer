using NullPointer.Evidence;

namespace NullPointer.Inspect
{
    public interface IEvidenceCollectionFollowUp
    {
        void OnEvidenceCollected(EvidenceData evidence);
    }
}
