using System.Collections.Generic;
using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Deduction
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Deduction", fileName = "DED_NewDeduction")]
    public sealed class DeductionData : AuthoredContentAsset
    {
        [SerializeField] private List<string> _requiredEvidenceIds = new List<string>();
        [SerializeField] private string _resultTitle = string.Empty;
        [SerializeField, TextArea] private string _resultText = string.Empty;
        [SerializeField] private string _resultingEvidenceId = string.Empty;
        [SerializeField] private List<string> _storyFlagsToSet = new List<string>();

        public IReadOnlyList<string> RequiredEvidenceIds => _requiredEvidenceIds;

        public string ResultTitle => _resultTitle;

        public string ResultText => _resultText;

        public string ResultingEvidenceId => _resultingEvidenceId;

        public IReadOnlyList<string> StoryFlagsToSet => _storyFlagsToSet;
    }
}
