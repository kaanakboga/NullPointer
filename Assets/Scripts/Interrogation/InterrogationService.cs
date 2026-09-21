using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;

namespace NullPointer.Interrogation
{
    public sealed class InterrogationService
    {
        private const string ResolvedProgressPrefix = "dialogue.interrogation.resolved.";
        private readonly GameState _gameState;
        private readonly Dictionary<string, InterrogationClaimData> _claims;

        public InterrogationService(GameState gameState, IEnumerable<InterrogationClaimData> claims)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _claims = new Dictionary<string, InterrogationClaimData>(StringComparer.Ordinal);
            foreach (InterrogationClaimData claim in claims ?? Array.Empty<InterrogationClaimData>())
            {
                if (claim != null && !_claims.TryAdd(claim.StableId, claim))
                {
                    throw new ArgumentException($"Duplicate interrogation claim ID '{claim.StableId}'.", nameof(claims));
                }
            }
        }

        public InterrogationAttemptResult PresentEvidence(string claimId, string evidenceId)
        {
            if (string.IsNullOrWhiteSpace(claimId) || !_claims.TryGetValue(claimId.Trim(), out InterrogationClaimData claim))
            {
                return new InterrogationAttemptResult(InterrogationAttemptStatus.UnknownClaim, null, string.Empty);
            }

            string progressId = ResolvedProgressPrefix + claim.StableId;
            if (_gameState.HasDialogueProgress(progressId))
            {
                return new InterrogationAttemptResult(
                    InterrogationAttemptStatus.AlreadyResolved,
                    claim,
                    claim.SuccessResponse);
            }

            if (string.IsNullOrWhiteSpace(evidenceId) || !_gameState.HasCollectedEvidence(evidenceId.Trim()))
            {
                return new InterrogationAttemptResult(
                    InterrogationAttemptStatus.EvidenceNotCollected,
                    claim,
                    "Bu kanıt henüz soruşturma kaydında değil.");
            }

            bool contradicts = claim.ContradictingEvidenceIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Contains(evidenceId.Trim(), StringComparer.Ordinal);
            if (!contradicts)
            {
                return new InterrogationAttemptResult(
                    InterrogationAttemptStatus.IrrelevantEvidence,
                    claim,
                    claim.IrrelevantResponse);
            }

            _gameState.RecordDialogueProgress(progressId);
            if (!string.IsNullOrWhiteSpace(claim.ContradictionStoryFlag))
            {
                _gameState.SetStoryFlag(claim.ContradictionStoryFlag);
            }

            return new InterrogationAttemptResult(
                InterrogationAttemptStatus.ContradictionFound,
                claim,
                claim.SuccessResponse);
        }
    }
}
