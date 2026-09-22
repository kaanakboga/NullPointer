using System;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Editor
{
    public sealed class PixelArtImportPostprocessor : AssetPostprocessor
    {
        private const float WorldPixelsPerUnit = 16f;
        private const float UiPixelsPerUnit = 100f;

        private void OnPreprocessTexture()
        {
            string normalizedPath = assetPath.Replace('\\', '/');
            if (!normalizedPath.StartsWith("Assets/Art/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = FilterMode.Point;
            importer.alphaIsTransparency = true;

            if (normalizedPath.EndsWith("_Normal.png", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.EndsWith("_Normal.tga", StringComparison.OrdinalIgnoreCase))
            {
                importer.textureType = TextureImporterType.NormalMap;
                return;
            }

            if (!IsProductionSpriteFolder(normalizedPath))
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = normalizedPath.StartsWith(
                "Assets/Art/UI/",
                StringComparison.OrdinalIgnoreCase)
                ? UiPixelsPerUnit
                : WorldPixelsPerUnit;
        }

        private static bool IsProductionSpriteFolder(string path)
        {
            return path.StartsWith("Assets/Art/Characters/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("Assets/Art/Environments/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("Assets/Art/Effects/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("Assets/Art/Icons/", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("Assets/Art/UI/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
