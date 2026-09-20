using UnityEngine;

namespace NullPointer.SceneFlow
{
    [AddComponentMenu("Null Pointer/Scene Flow/Spawn Point")]
    public sealed class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private string _spawnPointId = string.Empty;

        public string SpawnPointId => _spawnPointId;

        public void Configure(string spawnPointId)
        {
            _spawnPointId = spawnPointId ?? string.Empty;
        }
    }
}
