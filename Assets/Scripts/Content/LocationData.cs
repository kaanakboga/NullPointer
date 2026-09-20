using UnityEngine;

namespace NullPointer.Content
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Location", fileName = "LOC_NewLocation")]
    public sealed class LocationData : AuthoredContentAsset
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private string _sceneName = string.Empty;

        public string DisplayName => _displayName;

        public string SceneName => _sceneName;
    }
}
