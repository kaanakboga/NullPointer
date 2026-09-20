using System;
using System.Collections.Generic;

namespace NullPointer.Content
{
    public sealed class ContentIdDiagnostic
    {
        public ContentIdDiagnostic(
            ContentIdDiagnosticCode code,
            string message,
            IReadOnlyList<string> assetNames)
        {
            Code = code;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            AssetNames = assetNames ?? throw new ArgumentNullException(nameof(assetNames));
        }

        public ContentIdDiagnosticCode Code { get; }

        public string Message { get; }

        public IReadOnlyList<string> AssetNames { get; }
    }
}
