using System;
using System.IO;
using System.Linq;
using NullPointer.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.Editor
{
    public static class Phase5DVisualCapture
    {
        private const string ErenPath = "Assets/Scenes/Gameplay/SCN_ErenApartment.unity";
        private const string MertPath = "Assets/Scenes/Gameplay/SCN_MertApartment.unity";
        private static readonly (int Width, int Height)[] Resolutions =
        {
            (1920, 1080),
            (2560, 1440),
            (2560, 1600)
        };

        [MenuItem("Null Pointer/Visual/Capture Phase 5D Environments")]
        public static void CaptureAll()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            string output = Path.Combine(projectRoot, "Logs", "VisualReview", "Phase5D");
            Directory.CreateDirectory(output);

            foreach ((int width, int height) in Resolutions)
            {
                Capture(ErenPath, "Eren-Normal-Gameplay", width, height, roots => Prepare(roots, -4.7f, 0.4f), output);
                Capture(ErenPath, "Eren-Terminal-Focal", width, height, roots => Prepare(roots, 1.15f, 2.1f), output);
                Capture(ErenPath, "Eren-Window-City-Focal", width, height, roots => Prepare(roots, -0.5f, 4.6f), output);
                Capture(ErenPath, "Eren-Rain-Lighting", width, height, roots => Prepare(roots, 3.2f, 8.8f), output);
                Capture(ErenPath, "Eren-Pause-Overlay", width, height, roots => PreparePause(roots, -1.8f), output);

                Capture(MertPath, "Mert-Entrance", width, height, roots => Prepare(roots, -5.2f, 0.8f), output);
                Capture(MertPath, "Mert-Workstation", width, height, roots => Prepare(roots, -2.8f, 2.7f), output);
                Capture(MertPath, "Mert-Evidence-Cluster", width, height, roots => Prepare(roots, 0.5f, 4.3f), output);
                Capture(MertPath, "Mert-Memory-Implant", width, height, roots => Prepare(roots, -0.9f, 6.1f), output);
                Capture(MertPath, "Mert-Terminal", width, height, roots => Prepare(roots, -2.35f, 7.4f), output);
                Capture(MertPath, "Mert-Pause-Overlay", width, height, roots => PreparePause(roots, 1.1f), output);
            }

            Debug.Log($"[Visual QA] Phase 5D wrote 33 production-environment captures to {output}.");
        }

        public static void CaptureFromCommandLine()
        {
            CaptureAll();
        }

        private static void Capture(
            string scenePath,
            string label,
            int width,
            int height,
            Action<GameObject[]> prepare,
            string output)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            HideUi(roots);
            prepare(roots);

            Canvas canvas = FindInRoots<Canvas>(roots);
            Camera camera = FindInRoots<Camera>(roots);
            if (canvas == null || camera == null)
            {
                throw new InvalidOperationException($"Phase 5D capture requires a production Canvas and Camera in {scenePath}.");
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
                File.WriteAllBytes(Path.Combine(output, $"Phase5D-{label}-{width}x{height}.png"), texture.EncodeToPNG());
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

        private static void Prepare(GameObject[] roots, float playerX, float previewTime)
        {
            Transform player = FindNamed(roots, "Player")?.transform;
            if (player != null)
            {
                player.position = new Vector3(playerX, -1.15f, 0f);
            }

            FindInRoots<EnvironmentPresentationController>(roots)?.SetPreviewTime(previewTime);
        }

        private static void PreparePause(GameObject[] roots, float playerX)
        {
            Prepare(roots, playerX, 3.5f);
            GameObject pause = FindNamed(roots, "Pause Menu");
            if (pause != null)
            {
                pause.SetActive(true);
                CanvasGroup group = pause.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    group.alpha = 1f;
                }
            }
        }

        private static void HideUi(GameObject[] roots)
        {
            string[] keepHidden =
            {
                "Interaction Prompt", "Evidence Notification", "Objective HUD", "Pause Menu", "Pause Settings",
                "Inspect Panel", "Dialogue Panel", "Terminal Panel", "Memory Panel", "Evidence Board Panel",
                "Journal Panel", "Interrogation Panel"
            };
            foreach (string name in keepHidden)
            {
                FindNamed(roots, name)?.SetActive(false);
            }
        }

        private static T FindInRoots<T>(GameObject[] roots) where T : Component
        {
            return roots.Select(root => root.GetComponentInChildren<T>(true)).FirstOrDefault(item => item != null);
        }

        private static GameObject FindNamed(GameObject[] roots, string name)
        {
            foreach (GameObject root in roots)
            {
                Transform found = root.GetComponentsInChildren<Transform>(true)
                    .FirstOrDefault(item => string.Equals(item.name, name, StringComparison.Ordinal));
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }
    }
}
