namespace NullPointer.Interrogation
{
    public readonly struct InterrogationAttemptResult
    {
        public InterrogationAttemptResult(
            InterrogationAttemptStatus status,
            InterrogationClaimData claim,
            string response)
        {
            Status = status;
            Claim = claim;
            Response = response ?? string.Empty;
        }

        public InterrogationAttemptStatus Status { get; }

        public InterrogationClaimData Claim { get; }

        public string Response { get; }
    }
}
