using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Evidence
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Evidence", fileName = "EV_NewEvidence")]
    public sealed class EvidenceData : AuthoredContentAsset
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField, TextArea] private string _description = string.Empty;
        [SerializeField] private EvidenceType _evidenceType;
        [SerializeField] private string _caseId = string.Empty;
        [SerializeField] private bool _isCritical;
        [SerializeField] private Sprite _icon;

        public string DisplayName => _displayName;

        public string Description => _description;

        public EvidenceType EvidenceType => _evidenceType;

        public string CaseId => _caseId;

        public bool IsCritical => _isCritical;

        public Sprite Icon => _icon;
    }
}
