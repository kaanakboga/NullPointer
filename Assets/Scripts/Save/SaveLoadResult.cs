using NullPointer.Core;

namespace NullPointer.Save
{
    public sealed class SaveLoadResult
    {
        public SaveLoadResult(SaveLoadStatus status, GameState gameState, string message)
        {
            Status = status;
            GameState = gameState;
            Message = message ?? string.Empty;
        }

        public SaveLoadStatus Status { get; }

        public GameState GameState { get; }

        public string Message { get; }

        public bool IsSuccess => Status == SaveLoadStatus.Success;
    }
}
