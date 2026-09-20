using System;

namespace NullPointer.Deduction
{
    public readonly struct DeductionCompleted
    {
        public DeductionCompleted(DeductionData deduction)
        {
            Deduction = deduction ?? throw new ArgumentNullException(nameof(deduction));
        }

        public DeductionData Deduction { get; }

        public string DeductionId => Deduction.StableId;
    }
}
