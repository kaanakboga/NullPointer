using System.Collections.Generic;
using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Interrogation
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Interrogation Claim", fileName = "CLM_NewClaim")]
    public sealed class InterrogationClaimData : AuthoredContentAsset
    {
        [SerializeField, TextArea] private string _statement = string.Empty;
        [SerializeField] private List<string> _contradictingEvidenceIds = new List<string>();
        [SerializeField] private string _contradictionStoryFlag = string.Empty;
        [SerializeField, TextArea] private string _successResponse = string.Empty;
        [SerializeField, TextArea] private string _irrelevantResponse = string.Empty;

        public string Statement => _statement;

        public IReadOnlyList<string> ContradictingEvidenceIds => _contradictingEvidenceIds;

        public string ContradictionStoryFlag => _contradictionStoryFlag;

        public string SuccessResponse => _successResponse;

        public string IrrelevantResponse => _irrelevantResponse;
    }
}
