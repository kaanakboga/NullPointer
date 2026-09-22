using NullPointer.Core;

namespace NullPointer.Save
{
    public sealed class SaveLoadResult
    {
        public SaveLoadResult(SaveLoadStatus status, GameState gameState, string message)
            : this(status, gameState, message, string.Empty, SaveLoadSource.None, false)
        {
        }

        public SaveLoadResult(
            SaveLoadStatus status,
            GameState gameState,
            string message,
            string diagnosticMessage,
            SaveLoadSource source,
            bool wasMigrated)
        {
            Status = status;
            GameState = gameState;
            Message = message ?? string.Empty;
            DiagnosticMessage = diagnosticMessage ?? string.Empty;
            Source = source;
            WasMigrated = wasMigrated;
        }

        public SaveLoadStatus Status { get; }

        public GameState GameState { get; }

        public string Message { get; }

        public string DiagnosticMessage { get; }

        public SaveLoadSource Source { get; }

        public bool WasMigrated { get; }

        public bool IsSuccess => Status == SaveLoadStatus.Success ||
                                 Status == SaveLoadStatus.RecoveredFromBackup;
    }
}
