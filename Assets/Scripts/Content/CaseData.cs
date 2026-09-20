using UnityEngine;

namespace NullPointer.Content
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Case", fileName = "CASE_NewCase")]
    public sealed class CaseData : AuthoredContentAsset
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField, TextArea] private string _summary = string.Empty;

        public string DisplayName => _displayName;

        public string Summary => _summary;
    }
}
