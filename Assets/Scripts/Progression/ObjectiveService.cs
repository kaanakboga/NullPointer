using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Core;

namespace NullPointer.Progression
{
    public sealed class ObjectiveService
    {
        private readonly GameState _gameState;
        private readonly Dictionary<string, ObjectiveData> _catalog;

        public ObjectiveService(GameState gameState, IEnumerable<ObjectiveData> objectives)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
            _catalog = new Dictionary<string, ObjectiveData>(StringComparer.Ordinal);
            foreach (ObjectiveData objective in objectives ?? Array.Empty<ObjectiveData>())
            {
                if (objective != null && !_catalog.TryAdd(objective.StableId, objective))
                {
                    throw new ArgumentException($"Duplicate objective ID '{objective.StableId}'.", nameof(objectives));
                }
            }
        }

        public event Action<ObjectiveChanged> ObjectiveChanged;

        public bool Start(string objectiveId)
        {
            if (!TryGet(objectiveId, out ObjectiveData objective) || !_gameState.StartObjective(objective.StableId))
            {
                return false;
            }

            ObjectiveChanged?.Invoke(new ObjectiveChanged(objective, false));
            return true;
        }

        public bool Complete(string objectiveId)
        {
            if (!TryGet(objectiveId, out ObjectiveData objective) || !_gameState.CompleteObjective(objective.StableId))
            {
                return false;
            }

            ObjectiveChanged?.Invoke(new ObjectiveChanged(objective, true));
            return true;
        }

        public IReadOnlyList<ObjectiveData> GetActive()
        {
            return _catalog.Values
                .Where(objective => _gameState.IsObjectiveActive(objective.StableId))
                .OrderBy(objective => objective.DisplayOrder)
                .ThenBy(objective => objective.StableId, StringComparer.Ordinal)
                .ToArray();
        }

        public IReadOnlyList<ObjectiveData> GetCompleted()
        {
            return _catalog.Values
                .Where(objective => _gameState.HasCompletedObjective(objective.StableId))
                .OrderBy(objective => objective.DisplayOrder)
                .ThenBy(objective => objective.StableId, StringComparer.Ordinal)
                .ToArray();
        }

        private bool TryGet(string objectiveId, out ObjectiveData objective)
        {
            objective = null;
            return !string.IsNullOrWhiteSpace(objectiveId) &&
                   _catalog.TryGetValue(objectiveId.Trim(), out objective);
        }
    }
}
