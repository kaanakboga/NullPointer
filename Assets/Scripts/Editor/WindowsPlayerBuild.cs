using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NullPointer.Editor
{
    public static class WindowsPlayerBuild
    {
        private const string ProductName = "Null Pointer: Anılar Silinmeden Önce";

        public static void BuildDevelopment()
        {
            Build(true);
        }

        public static void BuildRelease()
        {
            Build(false);
        }

        private static void Build(bool development)
        {
            ProductionContentValidator.ValidateOrThrow();
            if (!string.Equals(PlayerSettings.productName, ProductName, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"PlayerSettings.productName must be '{ProductName}' before a production build.");
            }

            string outputPath = GetArgumentValue("-buildOutputPath");
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                string configuration = development ? "Windows-Development" : "Windows-Release";
                outputPath = Path.Combine(
                    Directory.GetParent(Application.dataPath).FullName,
                    "Builds",
                    configuration,
                    "NullPointer.exe");
            }

            outputPath = Path.GetFullPath(outputPath);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Standalone,
                    BuildTarget.StandaloneWindows64))
            {
                throw new InvalidOperationException("Unity could not switch to the Windows x86-64 build target.");
            }

            BuildOptions options = development ? BuildOptions.Development : BuildOptions.None;
            if (HasArgument("-cleanBuild"))
            {
                options |= BuildOptions.CleanBuildCache;
            }

            var buildOptions = new BuildPlayerOptions
            {
                scenes = ProductionContentValidator.ProductionScenePaths,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                targetGroup = BuildTargetGroup.Standalone,
                options = options
            };
            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;
            if (summary.result != BuildResult.Succeeded || summary.totalErrors > 0)
            {
                throw new InvalidOperationException(
                    $"Windows player build failed: {summary.result}, {summary.totalErrors} error(s), " +
                    $"{summary.totalWarnings} warning(s).");
            }

            Debug.Log(
                $"[Build] Windows {(development ? "Development" : "Release")} player succeeded at " +
                $"'{outputPath}' ({summary.totalSize} bytes, {summary.totalWarnings} warning(s)).");
        }

        private static string GetArgumentValue(string key)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            int index = Array.FindIndex(arguments, value => string.Equals(value, key, StringComparison.OrdinalIgnoreCase));
            return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : string.Empty;
        }

        private static bool HasArgument(string key)
        {
            return Environment.GetCommandLineArgs()
                .Any(value => string.Equals(value, key, StringComparison.OrdinalIgnoreCase));
        }
    }
}
