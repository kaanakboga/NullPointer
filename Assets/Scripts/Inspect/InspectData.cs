using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Inspect
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Inspection", fileName = "INSP_NewInspection")]
    public sealed class InspectData : AuthoredContentAsset
    {
        [SerializeField] private string _title = string.Empty;
        [SerializeField, TextArea] private string _description = string.Empty;

        public string Title => _title;

        public string Description => _description;
    }
}
