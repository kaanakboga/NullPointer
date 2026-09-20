using System.Collections.Generic;
using System.Linq;
using NullPointer.Content;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Editor
{
    public static class ContentValidationMenu
    {
        [MenuItem("Null Pointer/Content/Validate Stable Content IDs")]
        public static void ValidateAllMenu()
        {
            IReadOnlyList<ContentIdDiagnostic> diagnostics = ValidateAll();
            if (diagnostics.Count == 0)
            {
                Debug.Log("[Content] Stable content ID validation passed.");
                return;
            }

            foreach (ContentIdDiagnostic diagnostic in diagnostics)
            {
                Debug.LogError($"[Content] {diagnostic.Code}: {diagnostic.Message}");
            }

            throw new System.InvalidOperationException(
                $"Stable content ID validation failed with {diagnostics.Count} diagnostic(s).");
        }

        public static IReadOnlyList<ContentIdDiagnostic> ValidateAll()
        {
            IEnumerable<PathAwareContent> assets = AssetDatabase.FindAssets("t:ScriptableObject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(path => new
                {
                    Path = path,
                    Asset = AssetDatabase.LoadAssetAtPath<AuthoredContentAsset>(path)
                })
                .Where(item => item.Asset != null)
                .Select(item => new PathAwareContent(item.Asset, item.Path));

            return ContentIdValidator.Validate(assets);
        }

        private sealed class PathAwareContent : IStableContent
        {
            private readonly AuthoredContentAsset _asset;
            private readonly string _path;

            public PathAwareContent(AuthoredContentAsset asset, string path)
            {
                _asset = asset;
                _path = path;
            }

            public string StableId => _asset.StableId;

            public string DiagnosticName => $"{_asset.name} ({_path})";
        }
    }
}
