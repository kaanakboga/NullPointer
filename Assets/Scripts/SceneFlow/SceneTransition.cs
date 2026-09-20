using System;
using NullPointer.Content;

namespace NullPointer.SceneFlow
{
    public readonly struct SceneTransition
    {
        public SceneTransition(LocationData location, string spawnPointId)
        {
            Location = location ?? throw new ArgumentNullException(nameof(location));
            SpawnPointId = spawnPointId ?? string.Empty;
        }

        public LocationData Location { get; }

        public string SpawnPointId { get; }
    }
}
