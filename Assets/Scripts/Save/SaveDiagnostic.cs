using System;

namespace NullPointer.Save
{
    public enum SaveDiagnosticSeverity
    {
        Information = 0,
        Warning = 1,
        Error = 2
    }

    public sealed class SaveDiagnostic
    {
        public SaveDiagnostic(SaveDiagnosticSeverity severity, string message, Exception exception = null)
        {
            Severity = severity;
            Message = message ?? string.Empty;
            Exception = exception;
        }

        public SaveDiagnosticSeverity Severity { get; }

        public string Message { get; }

        public Exception Exception { get; }
    }
}
