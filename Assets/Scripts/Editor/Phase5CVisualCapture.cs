using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Evidence;
using NullPointer.Inspect;
using NullPointer.Interrogation;
using NullPointer.Journal;
using NullPointer.Memory;
using NullPointer.Menus;
using NullPointer.Progression;
using NullPointer.Runtime;
using NullPointer.Terminal;
using NullPointer.UI;
using NullPointer.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.Editor
{
    public static class Phase5CVisualCapture
    {
        private const string GameplayPath = "Assets/Scenes/Gameplay/SCN_ErenApartment.unity";
        private static readonly (int Width, int Height)[] Resolutions =
        {
            (1920, 1080),
            (2560, 1440),
            (2560, 1600)
        };

        [MenuItem("Null Pointer/Visual/Capture Phase 5C Surfaces")]
        public static void CaptureAll()
        {
            string output = Path.Combine(Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty,
                "Logs", "VisualReview");
            Directory.CreateDirectory(output);

            foreach ((int width, int height) in Resolutions)
            {
                Capture("Dialogue", width, height, PrepareDialogue, output);
                Capture("Inspect", width, height, PrepareInspect, output);
                Capture("Evidence-Acquired", width, height, PrepareEvidenceAcquired, output);
                Capture("Evidence-Board", width, height, PrepareEvidenceBoard, output);
                Capture("Deduction-Solved", width, height, PrepareDeductionSolved, output);
                Capture("Terminal", width, height, PrepareTerminal, output);
                Capture("Journal", width, height, PrepareJournal, output);
                Capture("Objective-Update", width, height, PrepareObjective, output);
                Capture("Interrogation", width, height, PrepareInterrogation, output);
                Capture("Memory-Event", width, height, PrepareMemory, output);
            }

            Debug.Log($"[Visual QA] Phase 5C wrote 30 production-surface captures to {output}.");
        }

        public static void CaptureFromCommandLine()
        {
            CaptureAll();
        }

        private static void Capture(string label, int width, int height, Action<GameObject[]> prepare, string output)
        {
            Scene scene = EditorSceneManager.OpenScene(GameplayPath, OpenSceneMode.Single);
            GameObject[] roots = scene.GetRootGameObjects();
            HideAllProductionSurfaces(roots);
            prepare(roots);

            Canvas canvas = FindInRoots<Canvas>(roots);
            Camera camera = FindInRoots<Camera>(roots);
            if (canvas == null || camera == null)
            {
                throw new InvalidOperationException("Phase 5C visual capture requires the production Canvas and Camera.");
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
                File.WriteAllBytes(Path.Combine(output, $"Phase5C-{label}-{width}x{height}.png"), texture.EncodeToPNG());
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

        private static void PrepareDialogue(GameObject[] roots)
        {
            DialoguePanel panel = FindInRoots<DialoguePanel>(roots);
            DialogueData data = Load<DialogueData>("Assets/Data/Dialogue/DLG_MertDamagedRecorder.asset");
            DialogueNode node = data.Nodes.First();
            panel.Show(node, node.Choices, new[] { "Kayıt bütünlüğü doğrulanıyor." }, 0f);
            FocusFirstButton(panel);
        }

        private static void PrepareInspect(GameObject[] roots)
        {
            InspectPanel panel = FindInRoots<InspectPanel>(roots);
            panel.Show(Load<InspectData>("Assets/Data/Inspections/INSP_MertDamagedImplant.asset"), true, false);
            FocusFirstButton(panel);
        }

        private static void PrepareEvidenceAcquired(GameObject[] roots)
        {
            ContentCatalog catalog = Load<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset");
            EvidenceData evidence = catalog.Evidence.First(item => item.IsCritical);
            var service = new EvidenceService(new GameState(), catalog.Evidence);
            EvidenceNotificationController notification = FindInRoots<EvidenceNotificationController>(roots);
            notification.Initialize(service);
            service.Collect(evidence);
        }

        private static void PrepareEvidenceBoard(GameObject[] roots)
        {
            ContentCatalog catalog = Load<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset");
            EvidenceData[] evidence = catalog.Evidence.Take(6).ToArray();
            var selected = new HashSet<string>(evidence.Take(3).Select(item => item.StableId), StringComparer.Ordinal);
            EvidenceBoardPanel panel = FindInRoots<EvidenceBoardPanel>(roots);
            panel.Show();
            panel.RenderEvidence(evidence, selected);
            panel.SetSolved(Array.Empty<DeductionData>());
            panel.SetStatus("3 KANIT SEÇİLDİ / BAĞLANTI ÖRÜNTÜSÜ İNCELENİYOR");
            FocusFirstButton(panel);
        }

        private static void PrepareDeductionSolved(GameObject[] roots)
        {
            ContentCatalog catalog = Load<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset");
            EvidenceData[] evidence = catalog.Evidence.Take(6).ToArray();
            var selected = new HashSet<string>(evidence.Take(3).Select(item => item.StableId), StringComparer.Ordinal);
            DeductionData deduction = catalog.Deductions.First();
            EvidenceBoardPanel panel = FindInRoots<EvidenceBoardPanel>(roots);
            panel.Show();
            panel.RenderEvidence(evidence, selected);
            panel.SetSolved(new[] { deduction });
            panel.SetStatus("ÇIKARIM DOĞRULANDI / DOSYA DURUMU GÜNCELLENDİ");
            panel.PlayDeductionFeedback(true);
            FocusFirstButton(panel);
        }

        private static void PrepareTerminal(GameObject[] roots)
        {
            TerminalData data = Load<TerminalData>("Assets/Data/Terminals/TERM_MertPersonal.asset");
            TerminalPanel panel = FindInRoots<TerminalPanel>(roots);
            panel.Show(data, data.Entries);
            panel.ShowEntry(data.Entries.First(), true);
            FocusFirstButton(panel);
        }

        private static void PrepareJournal(GameObject[] roots)
        {
            ContentCatalog catalog = Load<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset");
            var state = new GameState();
            var evidenceService = new EvidenceService(state, catalog.Evidence);
            foreach (EvidenceData evidence in catalog.Evidence.Take(4))
            {
                evidenceService.Collect(evidence);
            }

            JournalPanel panel = FindInRoots<JournalPanel>(roots);
            panel.Bind(new JournalService(state, evidenceService, catalog.JournalEntries), () => { });
            panel.Show();
            FocusFirstButton(panel);
        }

        private static void PrepareObjective(GameObject[] roots)
        {
            ContentCatalog catalog = Load<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset");
            FindInRoots<ObjectivePresenter>(roots).ShowPreview(catalog.Objectives.First());
        }

        private static void PrepareInterrogation(GameObject[] roots)
        {
            ContentCatalog catalog = Load<ContentCatalog>("Assets/Data/CAT_OpeningContent.asset");
            InterrogationPanel panel = FindInRoots<InterrogationPanel>(roots);
            panel.Show(catalog.InterrogationClaims.First(), catalog.Evidence.Take(4).ToArray());
            FocusFirstButton(panel);
        }

        private static void PrepareMemory(GameObject[] roots)
        {
            MemoryData data = Load<MemoryData>("Assets/Data/Memories/MEM_MertPhotoGlitch.asset");
            MemoryPanel panel = FindInRoots<MemoryPanel>(roots);
            panel.Show(data);
            panel.ShowBeat(data.Beats.First());
            MemoryDistortionController distortion = FindInRoots<MemoryDistortionController>(roots);
            distortion.ShowPreview(Load<MemoryDistortionProfile>(VisualProductionBuilder.MemoryProfilePath));
        }

        private static void HideAllProductionSurfaces(GameObject[] roots)
        {
            Type[] panelTypes =
            {
                typeof(DialoguePanel), typeof(InspectPanel), typeof(EvidenceBoardPanel), typeof(TerminalPanel),
                typeof(JournalPanel), typeof(MemoryPanel), typeof(InterrogationPanel)
            };
            foreach (Type type in panelTypes)
            {
                Component component = FindInRoots(roots, type);
                component?.gameObject.SetActive(false);
            }

            foreach (string name in new[]
                     {
                         "Evidence Notification", "Objective HUD", "Interaction Prompt", "Pause Menu",
                         "Pause Settings", "Settings Panel"
                     })
            {
                GameObject found = FindNamed(roots, name);
                found?.SetActive(false);
            }
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                throw new InvalidOperationException($"Required production asset is missing: {path}");
            }

            return asset;
        }

        private static T FindInRoots<T>(GameObject[] roots) where T : Component
        {
            return FindInRoots(roots, typeof(T)) as T;
        }

        private static Component FindInRoots(GameObject[] roots, Type type)
        {
            foreach (GameObject root in roots)
            {
                Component component = root.GetComponentInChildren(type, true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }

        private static GameObject FindNamed(GameObject[] roots, string name)
        {
            foreach (GameObject root in roots)
            {
                Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
                Transform found = transforms.FirstOrDefault(item => string.Equals(item.name, name, StringComparison.Ordinal));
                if (found != null)
                {
                    return found.gameObject;
                }
            }

            return null;
        }

        private static void FocusFirstButton(Component root)
        {
            CyberNoirButtonVisual visual = root?.GetComponentInChildren<CyberNoirButtonVisual>(true);
            visual?.SetPreviewFocused(true);
        }
    }
}
