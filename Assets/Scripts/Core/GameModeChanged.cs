namespace NullPointer.Core
{
    public readonly struct GameModeChanged
    {
        public GameModeChanged(GameMode previousMode, GameMode currentMode)
        {
            PreviousMode = previousMode;
            CurrentMode = currentMode;
        }

        public GameMode PreviousMode { get; }

        public GameMode CurrentMode { get; }
    }
}
