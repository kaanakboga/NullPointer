using System;

namespace NullPointer.Core
{
    public interface IGameModeService
    {
        event Action<GameModeChanged> ModeChanged;

        GameMode CurrentMode { get; }

        bool SetMode(GameMode mode);
    }
}
