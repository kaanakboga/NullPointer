using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Progression
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Checkpoint", fileName = "CHK_NewCheckpoint")]
    public sealed class CheckpointData : AuthoredContentAsset
    {
        [SerializeField] private string _displayName = string.Empty;
        [SerializeField] private LocationData _location;
        [SerializeField] private string _spawnPointId = "entry";

        public string DisplayName => _displayName;

        public LocationData Location => _location;

        public string SpawnPointId => _spawnPointId;
    }
}
