using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NullPointer.Editor;
using NullPointer.Visual;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NullPointer.Tests.EditMode
{
    public sealed class VisualInfrastructureTests
    {
        private static readonly string[] ProductionVisualScenes =
        {
            ProductionContentValidator.MainMenuScenePath,
            ProductionContentValidator.ErenScenePath,
            ProductionContentValidator.MertScenePath
        };

        [Test]
        public void VisualTheme_AuthoredAssetHasValidSemanticTimingAndColors()
        {
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(VisualProductionBuilder.ThemePath);

            Assert.That(theme, Is.Not.Null);
            Assert.That(theme.IsValid(out string reason), Is.True, reason);
            Assert.That(theme.NearBlack.grayscale, Is.LessThan(theme.PrimaryText.grayscale));
            Assert.That(theme.PrimaryAccent, Is.Not.EqualTo(theme.DangerAccent));
        }

        [Test]
        public void ProductionScenes_HaveUniqueVisualRootsAndFinalArtSlots()
        {
            var globalSlotIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (string path in ProductionVisualScenes)
            {
                WithScene(path, roots =>
                {
                    VisualRootAnchor[] anchors = FindAll<VisualRootAnchor>(roots);
                    Assert.That(anchors, Has.Length.EqualTo(1), path);
                    Assert.That(anchors[0].StableId, Is.Not.Empty, path);
                    Assert.That(anchors[0].Theme, Is.Not.Null, path);

                    FinalArtSlot[] slots = FindAll<FinalArtSlot>(roots);
                    Assert.That(slots, Is.Not.Empty, path);
                    Assert.That(slots.Select(slot => slot.StableId), Is.Unique, path);
                    foreach (FinalArtSlot slot in slots)
                    {
                        Assert.That(slot.StableId, Is.Not.Empty, path);
                        Assert.That(slot.ManifestAssetId, Is.Not.Empty, slot.StableId);
                        Assert.That(globalSlotIds.Add(slot.StableId), Is.True, slot.StableId);
                    }
                });
            }
        }

        [Test]
        public void ArtManifest_HasUniqueContractIdsAndCoversEveryProductionSlot()
        {
            string[] manifestIds = File.ReadAllLines("Docs/ART_ASSET_MANIFEST.md")
                .Where(line => line.StartsWith("| NP-", StringComparison.Ordinal))
                .Select(line => line.Split('|')[1].Trim())
                .ToArray();
            Assert.That(manifestIds, Has.Length.EqualTo(94));
            Assert.That(manifestIds, Is.Unique);
            var contract = new HashSet<string>(manifestIds, StringComparer.Ordinal);

            foreach (string path in ProductionVisualScenes)
            {
                WithScene(path, roots =>
                {
                    foreach (FinalArtSlot slot in FindAll<FinalArtSlot>(roots))
                    {
                        Assert.That(contract, Does.Contain(slot.ManifestAssetId),
                            $"{path} slot '{slot.StableId}' is not covered by the manifest.");
                    }
                });
            }
        }

        [Test]
        public void ArtSlotPreservation_ReappliesManualSpriteAfterStructuralReplacement()
        {
            Scene scene = SceneManager.GetActiveScene();
            Texture2D texture = new(2, 2);
            Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), Vector2.one * 0.5f, 16f);
            GameObject rebuilt = null;
            try
            {
                var original = new GameObject("Original");
                SceneManager.MoveGameObjectToScene(original, scene);
                SpriteRenderer originalRenderer = original.AddComponent<SpriteRenderer>();
                FinalArtSlot originalSlot = original.AddComponent<FinalArtSlot>();
                originalSlot.ConfigureStructural("slot.test.manual", "NP-TEST-001", originalRenderer);
                originalSlot.SetFinalArt(sprite);
                IReadOnlyDictionary<string, Sprite> snapshot = ArtSlotPreservation.Capture(new[] { originalSlot });
                UnityEngine.Object.DestroyImmediate(original);

                rebuilt = new GameObject("Rebuilt");
                SceneManager.MoveGameObjectToScene(rebuilt, scene);
                SpriteRenderer rebuiltRenderer = rebuilt.AddComponent<SpriteRenderer>();
                FinalArtSlot rebuiltSlot = rebuilt.AddComponent<FinalArtSlot>();
                rebuiltSlot.ConfigureStructural("slot.test.manual", "NP-TEST-001", rebuiltRenderer);

                ArtSlotPreservation.Restore(scene, snapshot);

                Assert.That(rebuiltSlot.FinalArt, Is.SameAs(sprite));
                Assert.That(rebuiltRenderer.sprite, Is.SameAs(sprite));
            }
            finally
            {
                if (rebuilt != null)
                {
                    UnityEngine.Object.DestroyImmediate(rebuilt);
                }

                UnityEngine.Object.DestroyImmediate(sprite);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [Test]
        public void FinalArtSlot_MissingOptionalArtKeepsStructuralPlaceholder()
        {
            Texture2D texture = new(2, 2);
            Sprite placeholder = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), Vector2.one * 0.5f, 16f);
            var target = new GameObject("Slot", typeof(SpriteRenderer));
            try
            {
                SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
                renderer.sprite = placeholder;
                renderer.color = Color.magenta;
                FinalArtSlot slot = target.AddComponent<FinalArtSlot>();
                slot.ConfigureStructural("slot.test.optional", "NP-TEST-OPTIONAL", renderer);
                slot.SetFinalArt(placeholder);
                slot.SetFinalArt(null);

                Assert.That(slot.HasFinalArt, Is.False);
                Assert.That(renderer.sprite, Is.SameAs(placeholder));
                Assert.That(renderer.color, Is.EqualTo(Color.magenta));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(placeholder);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        [TestCase("Assets/Art/Characters/Eren/CHR_Eren_Idle.png", ArtImportCategory.Character, 16f, UnityEditor.SpriteImportMode.Single)]
        [TestCase("Assets/Art/Characters/SpriteSheets/CHR_Eren_Walk.png", ArtImportCategory.Character, 16f, UnityEditor.SpriteImportMode.Multiple)]
        [TestCase("Assets/Art/Environments/Eren/ENV_Eren_Room.png", ArtImportCategory.Environment, 16f, UnityEditor.SpriteImportMode.Single)]
        [TestCase("Assets/Art/UI/UI_Panel.png", ArtImportCategory.Ui, 100f, UnityEditor.SpriteImportMode.Single)]
        [TestCase("Assets/Art/UI/Atlases/UI_FrameAtlas.png", ArtImportCategory.Ui, 100f, UnityEditor.SpriteImportMode.Multiple)]
        [TestCase("Assets/Art/Icons/ICO_Evidence.png", ArtImportCategory.Icon, 100f, UnityEditor.SpriteImportMode.Single)]
        public void ArtImportConvention_UsesCategorySpecificRules(
            string path,
            ArtImportCategory expectedCategory,
            float expectedPpu,
            UnityEditor.SpriteImportMode expectedMode)
        {
            ArtImportRules rules = ArtImportConvention.Resolve(path);

            Assert.That(rules.Applies, Is.True);
            Assert.That(rules.Category, Is.EqualTo(expectedCategory));
            Assert.That(rules.PixelsPerUnit, Is.EqualTo(expectedPpu));
            Assert.That(rules.SpriteMode, Is.EqualTo(expectedMode));
            Assert.That(rules.Mipmaps, Is.False);
            Assert.That(rules.Compression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(rules.FilterMode, Is.EqualTo(FilterMode.Point));
        }

        [TestCase("Assets/Settings/Logo.png")]
        [TestCase("Assets/Art/Reference/ErenConcept.png")]
        [TestCase("Assets/Art/Source/LayeredPaintover.png")]
        public void ArtImportConvention_DoesNotAffectUnownedOrReferenceTextures(string path)
        {
            Assert.That(ArtImportConvention.Resolve(path).Applies, Is.False);
        }

        [Test]
        public void VisualPreview_HasOneEventSystemAndMemoryFxDefaultsOff()
        {
            WithScene(VisualProductionBuilder.PreviewScenePath, roots =>
            {
                Assert.That(FindAll<EventSystem>(roots), Has.Length.EqualTo(1));
                MemoryDistortionController controller = FindAll<MemoryDistortionController>(roots).Single();
                GameObject fxRoot = roots
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .First(item => item.name == "Memory Distortion FX")
                    .gameObject;
                Assert.That(controller.IsActive, Is.False);
                Assert.That(fxRoot.activeSelf, Is.False);
            });
        }

        [TestCase(1920, 1080)]
        [TestCase(2560, 1440)]
        [TestCase(2560, 1600)]
        public void VisualPreview_LayoutBudgetFitsSupportedViewport(int width, int height)
        {
            Assert.That(VisualProductionBuilder.PreviewLayoutFitsViewport(width, height), Is.True,
                $"Preview safe-area content does not fit {width}x{height}.");
        }

        [Test]
        public void VisualPreview_UsesReferenceScalerSafeMarginsAndEdgePivots()
        {
            WithScene(VisualProductionBuilder.PreviewScenePath, roots =>
            {
                CanvasScaler scaler = FindAll<CanvasScaler>(roots).Single();
                Assert.That(scaler.uiScaleMode, Is.EqualTo(CanvasScaler.ScaleMode.ScaleWithScreenSize));
                Assert.That(scaler.screenMatchMode, Is.EqualTo(CanvasScaler.ScreenMatchMode.MatchWidthOrHeight));
                AssertVector(scaler.referenceResolution, VisualProductionBuilder.PreviewReferenceResolution);
                Assert.That(scaler.matchWidthOrHeight, Is.EqualTo(VisualProductionBuilder.PreviewMatchWidthOrHeight).Within(0.001f));

                RectTransform safeArea = FindRect(roots, "Preview Safe Area");
                AssertVector(safeArea.anchorMin, Vector2.zero);
                AssertVector(safeArea.anchorMax, Vector2.one);
                AssertVector(safeArea.offsetMin, VisualProductionBuilder.PreviewSafeMargins);
                AssertVector(safeArea.offsetMax, -VisualProductionBuilder.PreviewSafeMargins);

                RectTransform identity = FindRect(roots, "Identity Column");
                AssertVector(identity.anchorMin, Vector2.zero);
                AssertVector(identity.anchorMax, new Vector2(0.58f, 1f));
                AssertVector(identity.pivot, new Vector2(0f, 0.5f));

                RectTransform title = identity.Find("Title").GetComponent<RectTransform>();
                AssertVector(title.pivot, new Vector2(0f, 1f));
                Assert.That(title.anchoredPosition.x, Is.GreaterThanOrEqualTo(0f));

                RectTransform panel = FindRect(roots, "Investigation Panel");
                AssertVector(panel.anchorMin, new Vector2(1f, 0.5f));
                AssertVector(panel.anchorMax, new Vector2(1f, 0.5f));
                AssertVector(panel.pivot, new Vector2(1f, 0.5f));
                AssertVector(panel.anchoredPosition, Vector2.zero);
            });
        }

        private static void WithScene(string path, Action<GameObject[]> assertion)
        {
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(path), Is.Not.Null, path);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                assertion(scene.GetRootGameObjects());
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static T[] FindAll<T>(IEnumerable<GameObject> roots) where T : Component
        {
            return roots.SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        }

        private static RectTransform FindRect(IEnumerable<GameObject> roots, string name)
        {
            return roots
                .SelectMany(root => root.GetComponentsInChildren<RectTransform>(true))
                .Single(item => item.name == name);
        }

        private static void AssertVector(Vector2 actual, Vector2 expected)
        {
            Assert.That(Vector2.Distance(actual, expected), Is.LessThan(0.01f),
                $"Expected {expected}, received {actual}.");
        }
    }
}
