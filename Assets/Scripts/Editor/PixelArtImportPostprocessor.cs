using UnityEditor;

namespace NullPointer.Editor
{
    public sealed class PixelArtImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            ArtImportRules rules = ArtImportConvention.Resolve(assetPath);
            if (!rules.Applies)
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = rules.TextureType;
            importer.mipmapEnabled = rules.Mipmaps;
            importer.textureCompression = rules.Compression;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.filterMode = rules.FilterMode;
            importer.alphaIsTransparency = rules.AlphaIsTransparency;
            if (rules.TextureType == TextureImporterType.Sprite)
            {
                importer.spriteImportMode = rules.SpriteMode;
                importer.spritePixelsPerUnit = rules.PixelsPerUnit;
            }
        }
    }
}
