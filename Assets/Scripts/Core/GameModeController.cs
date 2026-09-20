using System;
using UnityEngine;

namespace NullPointer.Core
{
    [AddComponentMenu("Null Pointer/Core/Game Mode Controller")]
    public sealed class GameModeController : MonoBehaviour, IGameModeService
    {
        [SerializeField] private GameMode _initialMode = GameMode.Gameplay;

        private GameModeService _service;

        public event Action<GameModeChanged> ModeChanged
        {
            add => Service.ModeChanged += value;
            remove => Service.ModeChanged -= value;
        }

        public GameMode CurrentMode => Service.CurrentMode;

        public bool SetMode(GameMode mode)
        {
            return Service.SetMode(mode);
        }

        private GameModeService Service => _service ??= new GameModeService(_initialMode);
    }
}
