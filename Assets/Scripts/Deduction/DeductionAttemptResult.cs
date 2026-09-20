using System;
using System.Collections.Generic;

namespace NullPointer.Deduction
{
    public readonly struct DeductionAttemptResult
    {
        public DeductionAttemptResult(
            DeductionAttemptStatus status,
            DeductionData deduction,
            IReadOnlyList<string> missingEvidenceIds)
        {
            Status = status;
            Deduction = deduction;
            MissingEvidenceIds = missingEvidenceIds ?? Array.Empty<string>();
        }

        public DeductionAttemptStatus Status { get; }

        public DeductionData Deduction { get; }

        public IReadOnlyList<string> MissingEvidenceIds { get; }
    }
}
