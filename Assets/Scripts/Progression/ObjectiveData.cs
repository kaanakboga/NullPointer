using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Progression
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Objective", fileName = "OBJ_NewObjective")]
    public sealed class ObjectiveData : AuthoredContentAsset
    {
        [SerializeField] private string _title = string.Empty;
        [SerializeField, TextArea] private string _description = string.Empty;
        [SerializeField] private int _displayOrder;

        public string Title => _title;

        public string Description => _description;

        public int DisplayOrder => _displayOrder;
    }
}
