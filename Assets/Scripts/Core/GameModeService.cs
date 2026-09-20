using System;

namespace NullPointer.Core
{
    public sealed class GameModeService : IGameModeService
    {
        private GameMode _currentMode;

        public GameModeService(GameMode initialMode = GameMode.Gameplay)
        {
            EnsureDefined(initialMode);
            _currentMode = initialMode;
        }

        public event Action<GameModeChanged> ModeChanged;

        public GameMode CurrentMode => _currentMode;

        public bool SetMode(GameMode mode)
        {
            EnsureDefined(mode);

            if (_currentMode == mode)
            {
                return false;
            }

            GameMode previousMode = _currentMode;
            _currentMode = mode;
            ModeChanged?.Invoke(new GameModeChanged(previousMode, mode));
            return true;
        }

        private static void EnsureDefined(GameMode mode)
        {
            if (!Enum.IsDefined(typeof(GameMode), mode))
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown game mode.");
            }
        }
    }
}
