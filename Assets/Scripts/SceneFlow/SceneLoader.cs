using System;
using System.Collections;
using NullPointer.Content;
using NullPointer.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.SceneFlow
{
    [AddComponentMenu("Null Pointer/Scene Flow/Scene Loader")]
    public sealed class SceneLoader : MonoBehaviour
    {
        private GameState _gameState;

        public event Action<SceneTransition> TransitionStarted;

        public event Action<SceneTransition> TransitionCompleted;

        public bool IsLoading { get; private set; }

        public string PendingSpawnPointId { get; private set; } = string.Empty;

        public void Initialize(GameState gameState)
        {
            _gameState = gameState ?? throw new ArgumentNullException(nameof(gameState));
        }

        public bool LoadLocation(LocationData location, string spawnPointId)
        {
            if (IsLoading || location == null || string.IsNullOrWhiteSpace(location.SceneName))
            {
                return false;
            }

            StartCoroutine(LoadLocationAsync(location, spawnPointId));
            return true;
        }

        private IEnumerator LoadLocationAsync(LocationData location, string spawnPointId)
        {
            IsLoading = true;
            PendingSpawnPointId = spawnPointId ?? string.Empty;
            var transition = new SceneTransition(location, PendingSpawnPointId);
            TransitionStarted?.Invoke(transition);

            AsyncOperation operation = SceneManager.LoadSceneAsync(location.SceneName, LoadSceneMode.Single);
            if (operation == null)
            {
                Debug.LogError($"[SceneFlow] Unity could not begin loading scene '{location.SceneName}'.", this);
                IsLoading = false;
                yield break;
            }

            while (!operation.isDone)
            {
                yield return null;
            }

            _gameState.SetLocation(location.StableId);
            IsLoading = false;
            TransitionCompleted?.Invoke(transition);
        }
    }
}
