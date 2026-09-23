using System;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Editor
{
    public enum ArtImportCategory
    {
        None,
        Character,
        Environment,
        Ui,
        Effect,
        Icon,
        NormalMap
    }

    public readonly struct ArtImportRules
    {
        public ArtImportRules(
            ArtImportCategory category,
            TextureImporterType textureType,
            SpriteImportMode spriteMode,
            float pixelsPerUnit,
            FilterMode filterMode,
            bool mipmaps,
            TextureImporterCompression compression,
            bool alphaIsTransparency)
        {
            Category = category;
            TextureType = textureType;
            SpriteMode = spriteMode;
            PixelsPerUnit = pixelsPerUnit;
            FilterMode = filterMode;
            Mipmaps = mipmaps;
            Compression = compression;
            AlphaIsTransparency = alphaIsTransparency;
        }

        public ArtImportCategory Category { get; }
        public TextureImporterType TextureType { get; }
        public SpriteImportMode SpriteMode { get; }
        public float PixelsPerUnit { get; }
        public FilterMode FilterMode { get; }
        public bool Mipmaps { get; }
        public TextureImporterCompression Compression { get; }
        public bool AlphaIsTransparency { get; }
        public bool Applies => Category != ArtImportCategory.None;
    }

    public static class ArtImportConvention
    {
        public const float WorldPixelsPerUnit = 16f;
        public const float UiPixelsPerUnit = 100f;

        public static ArtImportRules Resolve(string assetPath)
        {
            string path = (assetPath ?? string.Empty).Replace('\\', '/');
            if (!path.StartsWith("Assets/Art/", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/Source/", StringComparison.OrdinalIgnoreCase) ||
                path.Contains("/Reference/", StringComparison.OrdinalIgnoreCase))
            {
                return default;
            }

            ArtImportCategory category = ResolveCategory(path);
            if (category == ArtImportCategory.None)
            {
                return default;
            }

            if (path.EndsWith("_Normal.png", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith("_Normal.tga", StringComparison.OrdinalIgnoreCase))
            {
                return new ArtImportRules(
                    ArtImportCategory.NormalMap,
                    TextureImporterType.NormalMap,
                    SpriteImportMode.Single,
                    WorldPixelsPerUnit,
                    FilterMode.Point,
                    false,
                    TextureImporterCompression.Uncompressed,
                    false);
            }

            bool multiple = path.Contains("/SpriteSheets/", StringComparison.OrdinalIgnoreCase) ||
                            path.Contains("/Atlases/", StringComparison.OrdinalIgnoreCase);
            float pixelsPerUnit = category is ArtImportCategory.Ui or ArtImportCategory.Icon
                ? UiPixelsPerUnit
                : WorldPixelsPerUnit;
            return new ArtImportRules(
                category,
                TextureImporterType.Sprite,
                multiple ? SpriteImportMode.Multiple : SpriteImportMode.Single,
                pixelsPerUnit,
                FilterMode.Point,
                false,
                TextureImporterCompression.Uncompressed,
                true);
        }

        private static ArtImportCategory ResolveCategory(string path)
        {
            if (path.StartsWith("Assets/Art/Characters/", StringComparison.OrdinalIgnoreCase))
            {
                return ArtImportCategory.Character;
            }

            if (path.StartsWith("Assets/Art/Environments/", StringComparison.OrdinalIgnoreCase))
            {
                return ArtImportCategory.Environment;
            }

            if (path.StartsWith("Assets/Art/UI/", StringComparison.OrdinalIgnoreCase))
            {
                return ArtImportCategory.Ui;
            }

            if (path.StartsWith("Assets/Art/Effects/", StringComparison.OrdinalIgnoreCase))
            {
                return ArtImportCategory.Effect;
            }

            return path.StartsWith("Assets/Art/Icons/", StringComparison.OrdinalIgnoreCase)
                ? ArtImportCategory.Icon
                : ArtImportCategory.None;
        }
    }
}
