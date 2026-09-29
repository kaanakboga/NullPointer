using System;
using System.IO;
using NullPointer.Menus;
using NullPointer.Settings;
using NullPointer.Visual;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.Editor
{
    public static class Phase5BVisualCapture
    {
        private const string MainMenuPath = "Assets/Scenes/MainMenu/SCN_MainMenu.unity";
        private const string GameplayPath = "Assets/Scenes/Gameplay/SCN_ErenApartment.unity";

        public static void CaptureAll()
        {
            string output = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty,
                "Logs", "VisualReview");
            Directory.CreateDirectory(output);
            foreach ((int width, int height) in new[] { (1920, 1080), (2560, 1440), (2560, 1600) })
            {
                Capture(MainMenuPath, "MainMenu", width, height,
                    roots => FocusFirstButton(FindInRoots<MainMenuPanel>(roots)), output);
                Capture(MainMenuPath, "MainMenu-Settings", width, height,
                    roots =>
                    {
                        SetPanelActive<SettingsPanel>(roots, "Main Menu Settings");
                        FocusFirstButton(FindInRoots<SettingsPanel>(roots));
                    }, output);
                Capture(GameplayPath, "Pause", width, height,
                    roots =>
                    {
                        SetPanelActive<PauseMenuPanel>(roots, "Pause Menu");
                        FocusFirstButton(FindInRoots<PauseMenuPanel>(roots));
                    }, output);
                Capture(GameplayPath, "Pause-Settings", width, height,
                    roots =>
                    {
                        SetPanelActive<SettingsPanel>(roots, "Pause Settings");
                        FocusFirstButton(FindInRoots<SettingsPanel>(roots));
                    }, output);
            }

            Debug.Log($"[Visual QA] Phase 5B captures written to {output}.");
        }

        private static void Capture(string scenePath, string label, int width, int height,
            Action<GameObject[]> prepare, string output)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            prepare?.Invoke(roots);
            Canvas canvas = FindInRoots<Canvas>(roots);
            Camera camera = FindInRoots<Camera>(roots);
            if (canvas == null || camera == null)
            {
                throw new InvalidOperationException($"Visual capture requires a Canvas and Camera in {scenePath}.");
            }

            RenderMode previousMode = canvas.renderMode;
            Camera previousWorldCamera = canvas.worldCamera;
            float previousPlaneDistance = canvas.planeDistance;
            RenderTexture previousTarget = camera.targetTexture;
            var renderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            try
            {
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.worldCamera = camera;
                canvas.planeDistance = 1f;
                camera.targetTexture = renderTexture;
                Canvas.ForceUpdateCanvases();
                camera.Render();
                RenderTexture.active = renderTexture;
                texture.ReadPixels(new Rect(0f, 0f, width, height), 0, 0);
                texture.Apply(false, false);
                File.WriteAllBytes(Path.Combine(output, $"{label}-{width}x{height}.png"), texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active = null;
                camera.targetTexture = previousTarget;
                canvas.renderMode = previousMode;
                canvas.worldCamera = previousWorldCamera;
                canvas.planeDistance = previousPlaneDistance;
                renderTexture.Release();
                UnityEngine.Object.DestroyImmediate(renderTexture);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void SetPanelActive<T>(GameObject[] roots, string objectName) where T : Component
        {
            foreach (GameObject root in roots)
            {
                foreach (T panel in root.GetComponentsInChildren<T>(true))
                {
                    panel.gameObject.SetActive(string.Equals(panel.gameObject.name, objectName,
                        StringComparison.Ordinal));
                    if (panel is SettingsPanel settings && panel.gameObject.activeSelf)
                    {
                        settings.PrepareValues(new GameSettings());
                    }
                }
            }
        }

        private static T FindInRoots<T>(GameObject[] roots) where T : Component
        {
            foreach (GameObject root in roots)
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static void FocusFirstButton(Component root)
        {
            if (root == null)
            {
                return;
            }

            CyberNoirButtonVisual visual = root.GetComponentInChildren<CyberNoirButtonVisual>(true);
            visual?.SetPreviewFocused(true);
        }
    }
}
