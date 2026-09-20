using NullPointer.Content;
using NullPointer.Core;
using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.SceneFlow
{
    [AddComponentMenu("Null Pointer/Scene Flow/Scene Transition Interactable")]
    public sealed class SceneTransitionInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private LocationData _targetLocation;
        [SerializeField] private string _targetSpawnPointId = string.Empty;
        [SerializeField] private string _requiredStoryFlag = string.Empty;
        [SerializeField] private int _interactionPriority;
        [SerializeField] private string _interactionPrompt = "İlerle";

        private SceneLoader _sceneLoader;
        private GameState _gameState;

        public bool CanInteract =>
            isActiveAndEnabled &&
            _sceneLoader != null &&
            !_sceneLoader.IsLoading &&
            _targetLocation != null &&
            (string.IsNullOrWhiteSpace(_requiredStoryFlag) || _gameState.HasStoryFlag(_requiredStoryFlag));

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt => _interactionPrompt;

        public void Configure(
            LocationData targetLocation,
            string targetSpawnPointId,
            string requiredStoryFlag = "")
        {
            _targetLocation = targetLocation;
            _targetSpawnPointId = targetSpawnPointId ?? string.Empty;
            _requiredStoryFlag = requiredStoryFlag ?? string.Empty;
        }

        public void Initialize(SceneLoader sceneLoader, GameState gameState)
        {
            _sceneLoader = sceneLoader;
            _gameState = gameState;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _sceneLoader.LoadLocation(_targetLocation, _targetSpawnPointId);
            }
        }
    }
}
