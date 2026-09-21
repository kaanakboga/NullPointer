using NullPointer.Save;

namespace NullPointer.Menus
{
    public interface IGameSessionCommands
    {
        bool HasContinue { get; }

        bool StartNewGame();

        bool ContinueGame();

        SaveLoadResult SaveGame();

        void ReturnToMainMenu();

        void QuitGame();
    }
}
