using System;
using NullPointer.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NullPointer.Editor
{
    public static class VisualProductionBuilder
    {
        public const string ThemePath = "Assets/Art/UI/Themes/VT_CyberNoir.asset";
        public const string MemoryProfilePath = "Assets/Art/Effects/Profiles/VFX_MemoryDistortion_Default.asset";
        public const string PreviewScenePath = "Assets/Scenes/Test/SCN_VisualStylePreview.unity";
        public static readonly Vector2 PreviewReferenceResolution = new(1920f, 1080f);
        public static readonly Vector2 PreviewSafeMargins = new(96f, 72f);
        public const float PreviewMatchWidthOrHeight = 0.5f;
        public const float PreviewIdentityWidth = 900f;
        public const float PreviewPanelWidth = 650f;
        public const float PreviewPanelHeight = 720f;
        public const float PreviewMinimumColumnGap = 32f;

        [MenuItem("Null Pointer/Art/Rebuild Phase 5A Visual Preview")]
        public static void RebuildPreview()
        {
            EnsureVisualAssets();
            ArtProductionSetup.Configure();
            VisualTheme theme = AssetDatabase.LoadAssetAtPath<VisualTheme>(ThemePath);
            MemoryDistortionProfile memoryProfile = AssetDatabase.LoadAssetAtPath<MemoryDistortionProfile>(MemoryProfilePath);
            BuildPreviewScene(theme, memoryProfile);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Visual] Phase 5A theme assets and non-production preview scene rebuilt successfully.");
        }

        public static void RebuildFromCommandLine()
        {
            RebuildPreview();
        }

        public static void EnsureVisualAssets()
        {
            EnsureFolder("Assets/Art/UI/Themes");
            EnsureFolder("Assets/Art/UI/Atlases");
            EnsureFolder("Assets/Art/Characters/SpriteSheets");
            EnsureFolder("Assets/Art/Effects/Profiles");
            EnsureFolder("Assets/Art/Effects/Screen");
            EnsureFolder("Assets/Art/Effects/World");
            EnsureFolder("Assets/Art/Reference");
            EnsureFolder("Assets/Art/Source");

            VisualTheme theme = GetOrCreate<VisualTheme>(ThemePath);
            var themeObject = new SerializedObject(theme);
            SetColor(themeObject, "_nearBlack", "#070A0F");
            SetColor(themeObject, "_deepNavy", "#0C1523");
            SetColor(themeObject, "_charcoal", "#161A20");
            SetColor(themeObject, "_panel", "#0E1825F5");
            SetColor(themeObject, "_primaryAccent", "#4DA3A6");
            SetColor(themeObject, "_secondaryAccent", "#5D4D82");
            SetColor(themeObject, "_warningAccent", "#D28A3D");
            SetColor(themeObject, "_dangerAccent", "#B84A56");
            SetColor(themeObject, "_focusAccent", "#9AC8C5");
            SetColor(themeObject, "_primaryText", "#E0E4E5");
            SetColor(themeObject, "_secondaryText", "#8796A0");
            SetColor(themeObject, "_disabled", "#3F4951B8");
            themeObject.FindProperty("_microDuration").floatValue = 0.16f;
            themeObject.FindProperty("_panelDuration").floatValue = 0.24f;
            themeObject.FindProperty("_revealDuration").floatValue = 0.46f;
            themeObject.FindProperty("_focusPulsePeriod").floatValue = 1.2f;
            themeObject.FindProperty("_panelOpacity").floatValue = 0.96f;
            themeObject.FindProperty("_backdropOpacity").floatValue = 0.82f;
            themeObject.FindProperty("_disabledOpacity").floatValue = 0.46f;
            themeObject.FindProperty("_subtleFxOpacity").floatValue = 0.08f;
            themeObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(theme);

            MemoryDistortionProfile profile = GetOrCreate<MemoryDistortionProfile>(MemoryProfilePath);
            var profileObject = new SerializedObject(profile);
            profileObject.FindProperty("_duration").floatValue = 1.8f;
            profileObject.FindProperty("_intensity").floatValue = 0.55f;
            profileObject.FindProperty("_chromaticOffset").floatValue = 5f;
            profileObject.FindProperty("_horizontalTearing").floatValue = 28f;
            profileObject.FindProperty("_staticOpacity").floatValue = 0.16f;
            profileObject.FindProperty("_vignetteShift").floatValue = 0.18f;
            profileObject.FindProperty("_flashOpacity").floatValue = 0.12f;
            profileObject.FindProperty("_overlayOpacity").floatValue = 0.22f;
            profileObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(profile);
        }

        private static void BuildPreviewScene(VisualTheme theme, MemoryDistortionProfile memoryProfile)
        {
            Scene previous = SceneManager.GetActiveScene();
            bool preservePrevious = previous.IsValid() && previous.isLoaded && !string.IsNullOrEmpty(previous.path);
            if (previous.IsValid() && previous.isLoaded && string.IsNullOrEmpty(previous.path) && previous.isDirty && !Application.isBatchMode)
            {
                throw new InvalidOperationException("Save or close the current untitled scene before rebuilding the visual preview.");
            }

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                preservePrevious ? NewSceneMode.Additive : NewSceneMode.Single);
            SceneManager.SetActiveScene(scene);
            try
            {
                CreateCamera(theme);
                var eventSystemObject = new GameObject("EventSystem");
                eventSystemObject.AddComponent<EventSystem>();
                eventSystemObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();

                var canvasObject = new GameObject("Visual Preview Canvas", typeof(RectTransform));
                Canvas canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = PreviewReferenceResolution;
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = PreviewMatchWidthOrHeight;
                canvasObject.AddComponent<GraphicRaycaster>();
                canvasObject.AddComponent<VisualRootAnchor>().Configure(
                    "visual.preview.phase5a",
                    VisualRootKind.DevelopmentPreview,
                    theme);

                Image backdrop = CreateImage(canvasObject.transform, "Near Black Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, theme.NearBlack);
                CreateImage(backdrop.transform, "Deep Navy Field", new Vector2(0f, 0f), new Vector2(0.58f, 1f), Vector2.zero, Vector2.zero, theme.DeepNavy);
                CreateImage(backdrop.transform, "Violet Memory Band", new Vector2(0.58f, 0f), Vector2.one, Vector2.zero, Vector2.zero, new Color(theme.SecondaryAccent.r, theme.SecondaryAccent.g, theme.SecondaryAccent.b, 0.16f));
                CreateImage(backdrop.transform, "Amber Practical", new Vector2(0.08f, 0.12f), new Vector2(0.085f, 0.88f), Vector2.zero, Vector2.zero, theme.WarningAccent);

                GameObject safeArea = CreateRect(backdrop.transform, "Preview Safe Area", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                RectTransform safeRect = safeArea.GetComponent<RectTransform>();
                safeRect.offsetMin = PreviewSafeMargins;
                safeRect.offsetMax = -PreviewSafeMargins;

                GameObject identityColumn = CreateRect(safeArea.transform, "Identity Column", Vector2.zero, new Vector2(0.58f, 1f), Vector2.zero, Vector2.zero);
                RectTransform identityRect = identityColumn.GetComponent<RectTransform>();
                identityRect.pivot = new Vector2(0f, 0.5f);

                Text eyebrow = CreateText(identityColumn.transform, "Eyebrow", "MNEMOSYNE // VISUAL SYSTEM 5A", new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(PreviewIdentityWidth, 36f), 18, TextAnchor.MiddleLeft, theme.SecondaryText);
                eyebrow.rectTransform.pivot = new Vector2(0f, 1f);
                Text title = CreateText(identityColumn.transform, "Title", "NULL POINTER", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -56f), new Vector2(PreviewIdentityWidth, 100f), 66, TextAnchor.MiddleLeft, theme.PrimaryText);
                title.rectTransform.pivot = new Vector2(0f, 1f);
                title.fontStyle = FontStyle.Bold;
                Text subtitle = CreateText(identityColumn.transform, "Subtitle", "ANILAR SİLİNMEDEN ÖNCE", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(4f, -154f), new Vector2(720f, 42f), 24, TextAnchor.MiddleLeft, theme.PrimaryAccent);
                subtitle.rectTransform.pivot = new Vector2(0f, 1f);
                Text metadata = CreateText(identityColumn.transform, "Metadata", "03:17  //  SIGNAL INTEGRITY 72%  //  SECTOR 7", Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(850f, 38f), 17, TextAnchor.MiddleLeft, theme.SecondaryText);
                metadata.rectTransform.pivot = Vector2.zero;

                Image panel = CreateImage(safeArea.transform, "Investigation Panel", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(PreviewPanelWidth, PreviewPanelHeight), theme.Panel);
                panel.rectTransform.pivot = new Vector2(1f, 0.5f);
                CanvasGroup panelGroup = panel.gameObject.AddComponent<CanvasGroup>();
                UiTransition transition = panel.gameObject.AddComponent<UiTransition>();
                transition.Configure(panel.rectTransform, panelGroup, true, true, true, new Vector2(36f, 0f), new Vector3(0.985f, 0.985f, 1f), theme.PanelDuration);
                CreateImage(panel.transform, "Panel Accent", new Vector2(0f, 0f), new Vector2(0.008f, 1f), Vector2.zero, Vector2.zero, theme.PrimaryAccent);
                CreateText(panel.transform, "Panel Heading", "ACTIVE CASE", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(46f, -64f), new Vector2(-92f, 45f), 19, TextAnchor.MiddleLeft, theme.PrimaryAccent);
                Text caseTitle = CreateText(panel.transform, "Case Title", "MERT ERSOY\nTIMELINE ANOMALY", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(46f, -145f), new Vector2(-92f, 104f), 31, TextAnchor.UpperLeft, theme.PrimaryText);
                caseTitle.fontStyle = FontStyle.Bold;
                CreateText(panel.transform, "Body", "Clean records are not proof of a clean memory.\n\nSelect a controlled response. Focus remains readable by shape, motion, and value—not color alone.", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(46f, 48f), new Vector2(-92f, 190f), 22, TextAnchor.UpperLeft, theme.SecondaryText);

                Button replay = CreateCyberButton(panel.transform, "Replay Motion", "REPLAY PANEL MOTION", new Vector2(0f, -150f), theme, true);
                Button memory = CreateCyberButton(panel.transform, "Memory Distortion", "PREVIEW MEMORY DISTORTION", new Vector2(0f, -238f), theme, false);
                Button disabled = CreateCyberButton(panel.transform, "Disabled", "LOCKED // NEEDS EVIDENCE", new Vector2(0f, -326f), theme, false);
                disabled.interactable = false;

                ScreenOverlayLayer scanlines = CreateScanlines(backdrop.transform, theme);
                MemoryDistortionController memoryFx = CreateMemoryFx(canvasObject.transform, theme);
                var controller = canvasObject.AddComponent<VisualStylePreviewController>();
                controller.Configure(transition, memoryFx, memoryProfile, replay, memory);
                _ = eyebrow;
                _ = scanlines;

                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene, PreviewScenePath))
                {
                    throw new InvalidOperationException($"Failed to save visual preview scene at {PreviewScenePath}.");
                }
            }
            finally
            {
                if (preservePrevious)
                {
                    EditorSceneManager.CloseScene(scene, true);
                    SceneManager.SetActiveScene(previous);
                }
            }
        }

        private static Button CreateCyberButton(
            Transform parent,
            string name,
            string text,
            Vector2 position,
            VisualTheme theme,
            bool primary)
        {
            Image surface = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(530f, 66f), theme.DeepNavy);
            Button button = surface.gameObject.AddComponent<Button>();
            button.targetGraphic = surface;
            button.transition = Selectable.Transition.None;
            Image glow = CreateImage(surface.transform, "Focus Glow", Vector2.zero, Vector2.one, new Vector2(-8f, 0f), new Vector2(16f, 16f), new Color(theme.FocusAccent.r, theme.FocusAccent.g, theme.FocusAccent.b, 0.18f));
            CanvasGroup glowGroup = glow.gameObject.AddComponent<CanvasGroup>();
            glowGroup.alpha = 0f;
            Image edge = CreateImage(surface.transform, "Animated Edge", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 2f), new Vector2(-2f, 3f), primary ? theme.WarningAccent : theme.PrimaryAccent);
            Text label = CreateText(surface.transform, "Label", text, Vector2.zero, Vector2.one, new Vector2(28f, 0f), new Vector2(-58f, -6f), 19, TextAnchor.MiddleLeft, theme.PrimaryText);
            label.fontStyle = FontStyle.Bold;
            surface.gameObject.AddComponent<CyberNoirButtonVisual>().Configure(theme, button, surface, label, edge, glowGroup, surface.rectTransform);
            return button;
        }

        private static ScreenOverlayLayer CreateScanlines(Transform parent, VisualTheme theme)
        {
            GameObject root = CreateRect(parent, "Subtle Scanlines", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            for (int index = 0; index < 36; index++)
            {
                float y = index / 36f;
                CreateImage(root.transform, $"Scanline {index:00}", new Vector2(0f, y), new Vector2(1f, y), Vector2.zero, new Vector2(0f, 1f), new Color(theme.PrimaryText.r, theme.PrimaryText.g, theme.PrimaryText.b, 1f));
            }

            ScreenOverlayLayer layer = root.AddComponent<ScreenOverlayLayer>();
            layer.Configure(group, theme.SubtleFxOpacity, true);
            return layer;
        }

        private static MemoryDistortionController CreateMemoryFx(Transform parent, VisualTheme theme)
        {
            var controllerObject = new GameObject("Memory Distortion Controller");
            controllerObject.transform.SetParent(parent, false);
            var controller = controllerObject.AddComponent<MemoryDistortionController>();
            GameObject root = CreateRect(parent, "Memory Distortion FX", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            Image overlay = CreateImage(root.transform, "Overlay", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, theme.SecondaryAccent);
            Image staticLayer = CreateImage(root.transform, "Static", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, theme.PrimaryText);
            Image vignette = CreateImage(root.transform, "Vignette Hook", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, theme.NearBlack);
            Image flash = CreateImage(root.transform, "Flash", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, theme.PrimaryText);
            Image tear = CreateImage(root.transform, "Horizontal Tear", new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), Vector2.zero, new Vector2(0f, 42f), theme.PrimaryAccent);
            Image red = CreateImage(root.transform, "Chromatic Left", new Vector2(0f, 0f), new Vector2(0.012f, 1f), Vector2.zero, Vector2.zero, theme.DangerAccent);
            Image cyan = CreateImage(root.transform, "Chromatic Right", new Vector2(0.988f, 0f), Vector2.one, Vector2.zero, Vector2.zero, theme.PrimaryAccent);
            controller.Configure(root, group, overlay, staticLayer, vignette, flash, tear.rectTransform, red.rectTransform, cyan.rectTransform);
            return controller;
        }

        private static void CreateCamera(VisualTheme theme)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = theme.NearBlack;
            cameraObject.AddComponent<AudioListener>();
        }

        public static bool PreviewLayoutFitsViewport(int pixelWidth, int pixelHeight)
        {
            if (pixelWidth <= 0 || pixelHeight <= 0)
            {
                return false;
            }

            float widthScale = pixelWidth / PreviewReferenceResolution.x;
            float heightScale = pixelHeight / PreviewReferenceResolution.y;
            float logWidth = Mathf.Log(widthScale, 2f);
            float logHeight = Mathf.Log(heightScale, 2f);
            float scale = Mathf.Pow(2f, Mathf.Lerp(logWidth, logHeight, PreviewMatchWidthOrHeight));
            float safeWidth = pixelWidth / scale - PreviewSafeMargins.x * 2f;
            float safeHeight = pixelHeight / scale - PreviewSafeMargins.y * 2f;
            return safeWidth >= PreviewIdentityWidth + PreviewPanelWidth + PreviewMinimumColumnGap
                && safeHeight >= PreviewPanelHeight;
        }

        private static Image CreateImage(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            Color color)
        {
            GameObject item = CreateRect(parent, name, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            Image image = item.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(
            Transform parent,
            string name,
            string text,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta,
            int fontSize,
            TextAnchor alignment,
            Color color)
        {
            GameObject item = CreateRect(parent, name, anchorMin, anchorMax, anchoredPosition, sizeDelta);
            Text label = item.AddComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = color;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.raycastTarget = false;
            return label;
        }

        private static GameObject CreateRect(
            Transform parent,
            string name,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 anchoredPosition,
            Vector2 sizeDelta)
        {
            var item = new GameObject(name, typeof(RectTransform));
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;
            return item;
        }

        private static void SetColor(SerializedObject target, string propertyName, string html)
        {
            if (!ColorUtility.TryParseHtmlString(html, out Color color))
            {
                throw new InvalidOperationException($"Invalid visual-theme color '{html}'.");
            }

            target.FindProperty(propertyName).colorValue = color;
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            string[] segments = path.Split('/');
            string current = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }
    }
}
