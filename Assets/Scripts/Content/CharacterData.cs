using UnityEngine;

namespace NullPointer.Content
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Character", fileName = "CHAR_NewCharacter")]
    public sealed class CharacterData : AuthoredContentAsset
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private string _roleLabel = string.Empty;
        [SerializeField] private Sprite _portrait;

        public string DisplayName => _displayName;

        public string RoleLabel => _roleLabel;

        public Sprite Portrait => _portrait;
    }
}
