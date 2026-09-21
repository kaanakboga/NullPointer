using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Journal
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Journal Entry", fileName = "JRN_NewEntry")]
    public sealed class JournalEntryData : AuthoredContentAsset
    {
        [SerializeField] private JournalSection _section;
        [SerializeField] private string _title = string.Empty;
        [SerializeField, TextArea] private string _body = string.Empty;
        [SerializeField] private string _requiredStoryFlag = string.Empty;
        [SerializeField] private string _requiredEvidenceId = string.Empty;
        [SerializeField] private string _requiredDeductionId = string.Empty;
        [SerializeField] private int _displayOrder;

        public JournalSection Section => _section;

        public string Title => _title;

        public string Body => _body;

        public string RequiredStoryFlag => _requiredStoryFlag;

        public string RequiredEvidenceId => _requiredEvidenceId;

        public string RequiredDeductionId => _requiredDeductionId;

        public int DisplayOrder => _displayOrder;
    }
}
