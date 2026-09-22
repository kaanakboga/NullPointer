using System;
using System.IO;
using NullPointer.Runtime;
using NullPointer.Save;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Editor
{
    public sealed class DevelopmentSaveDiagnosticsWindow : EditorWindow
    {
        private Vector2 _scroll;
        private string _report = "Use this window in Play Mode to inspect the live GameState.";

        [MenuItem("Null Pointer/Development/Save Diagnostics")]
        public static void Open()
        {
            GetWindow<DevelopmentSaveDiagnosticsWindow>("Save Diagnostics");
        }

        private void OnGUI()
        {
            string path = GetSavePath();
            EditorGUILayout.LabelField("Development Save Diagnostics", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Editor-only controls. They operate on the local development slot and never appear in a player UI.",
                MessageType.Info);
            EditorGUILayout.LabelField("Primary path", path);
            EditorGUILayout.LabelField("Backup path", path + ".bak");

            GameApplication application = FindApplication();
            using (new EditorGUI.DisabledScope(application == null || application.GameState == null))
            {
                if (GUILayout.Button("Inspect Current GameState"))
                {
                    _report = JsonUtility.ToJson(application.GameState.CreateSnapshot(), true);
                }

                if (GUILayout.Button("Create Save From Current GameState"))
                {
                    ShowResult(application.SaveGame());
                }
            }

            if (GUILayout.Button("Reload and Inspect Save"))
            {
                SaveLoadResult result = CreateManager(path).Load();
                ShowResult(result);
                if (result.IsSuccess)
                {
                    _report += Environment.NewLine + JsonUtility.ToJson(result.GameState.CreateSnapshot(), true);
                }
            }

            if (GUILayout.Button("Restore Valid Backup"))
            {
                ShowResult(CreateManager(path).RestoreBackup());
            }

            if (GUILayout.Button("Simulate Corrupt Primary Save") &&
                EditorUtility.DisplayDialog(
                    "Corrupt development save?",
                    "This replaces only the primary local development save. The backup is preserved.",
                    "Corrupt Primary",
                    "Cancel"))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, "{ simulated-corrupt-primary");
                _report = "Primary save corrupted for recovery testing. Backup was not changed.";
            }

            if (GUILayout.Button("Delete Primary and Backup") &&
                EditorUtility.DisplayDialog(
                    "Delete development save?",
                    "This deletes the primary and backup files for the local development slot.",
                    "Delete",
                    "Cancel"))
            {
                CreateManager(path).Delete();
                _report = "Development save and backup deleted.";
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_report, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void ShowResult(SaveLoadResult result)
        {
            _report = result == null
                ? "No result."
                : $"Status: {result.Status}\nSource: {result.Source}\nMigrated: {result.WasMigrated}\n" +
                  $"Player message: {result.Message}\nDiagnostic: {result.DiagnosticMessage}";
        }

        private static SaveManager CreateManager(string path)
        {
            return new SaveManager(new FileSaveStorage(path), Application.version);
        }

        private static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, "save-v1.json");
        }

        private static GameApplication FindApplication()
        {
            return Application.isPlaying
                ? UnityEngine.Object.FindAnyObjectByType<GameApplication>(FindObjectsInactive.Include)
                : null;
        }
    }
}
