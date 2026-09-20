using System;

namespace NullPointer.Evidence
{
    public readonly struct EvidenceCollected
    {
        public EvidenceCollected(EvidenceData evidence)
        {
            Evidence = evidence ?? throw new ArgumentNullException(nameof(evidence));
        }

        public EvidenceData Evidence { get; }

        public string EvidenceId => Evidence.StableId;
    }
}
