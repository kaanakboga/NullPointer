using System;
using System.Collections.Generic;
using System.Linq;

namespace NullPointer.Content
{
    public static class ContentIdValidator
    {
        public static bool IsValid(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId) || stableId.Length < 3)
            {
                return false;
            }

            bool hasDot = false;
            bool atSegmentStart = true;

            for (int index = 0; index < stableId.Length; index++)
            {
                char character = stableId[index];
                if (character == '.')
                {
                    if (atSegmentStart || index == stableId.Length - 1)
                    {
                        return false;
                    }

                    hasDot = true;
                    atSegmentStart = true;
                    continue;
                }

                bool isLowercaseLetter = character >= 'a' && character <= 'z';
                bool isDigit = character >= '0' && character <= '9';
                bool isUnderscore = character == '_';
                if (!isLowercaseLetter && !isDigit && !isUnderscore)
                {
                    return false;
                }

                if (atSegmentStart && !isLowercaseLetter)
                {
                    return false;
                }

                atSegmentStart = false;
            }

            return hasDot;
        }

        public static IReadOnlyList<ContentIdDiagnostic> Validate(IEnumerable<IStableContent> assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException(nameof(assets));
            }

            var diagnostics = new List<ContentIdDiagnostic>();
            var assetsById = new Dictionary<string, List<IStableContent>>(StringComparer.Ordinal);

            foreach (IStableContent asset in assets)
            {
                if (asset == null)
                {
                    diagnostics.Add(new ContentIdDiagnostic(
                        ContentIdDiagnosticCode.NullAsset,
                        "The authored-content collection contains a null asset reference.",
                        Array.Empty<string>()));
                    continue;
                }

                if (string.IsNullOrWhiteSpace(asset.StableId))
                {
                    diagnostics.Add(new ContentIdDiagnostic(
                        ContentIdDiagnosticCode.MissingId,
                        $"Authored asset '{asset.DiagnosticName}' has no stable content ID.",
                        new[] { asset.DiagnosticName }));
                    continue;
                }

                if (!IsValid(asset.StableId))
                {
                    diagnostics.Add(new ContentIdDiagnostic(
                        ContentIdDiagnosticCode.InvalidFormat,
                        $"Authored asset '{asset.DiagnosticName}' has invalid stable ID '{asset.StableId}'. " +
                        "Use lowercase dot-separated ASCII segments.",
                        new[] { asset.DiagnosticName }));
                    continue;
                }

                if (!assetsById.TryGetValue(asset.StableId, out List<IStableContent> matches))
                {
                    matches = new List<IStableContent>();
                    assetsById.Add(asset.StableId, matches);
                }

                matches.Add(asset);
            }

            foreach (KeyValuePair<string, List<IStableContent>> pair in assetsById.OrderBy(pair => pair.Key))
            {
                if (pair.Value.Count < 2)
                {
                    continue;
                }

                string[] names = pair.Value.Select(asset => asset.DiagnosticName).OrderBy(name => name).ToArray();
                diagnostics.Add(new ContentIdDiagnostic(
                    ContentIdDiagnosticCode.DuplicateId,
                    $"Stable content ID '{pair.Key}' is used by conflicting assets: {string.Join(", ", names)}.",
                    names));
            }

            return diagnostics;
        }
    }
}
