using System.Linq;
using NullPointer.Content;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Inspect;
using NullPointer.Memory;
using NullPointer.Player;
using NullPointer.Runtime;
using NullPointer.SceneFlow;
using NullPointer.Terminal;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.Tests.EditMode
{
    public sealed class OpeningContentAndSceneTests
    {
        private const string CatalogPath = "Assets/Data/CAT_OpeningContent.asset";
        private const string BootstrapPath = "Assets/Scenes/Bootstrap/SCN_Bootstrap.unity";
        private const string ErenPath = "Assets/Scenes/Gameplay/SCN_ErenApartment.unity";
        private const string MertPath = "Assets/Scenes/Gameplay/SCN_MertApartment.unity";

        [Test]
        public void OpeningCatalog_ContainsValidUniqueIdsAndRequiredClueChain()
        {
            ContentCatalog catalog = AssetDatabase.LoadAssetAtPath<ContentCatalog>(CatalogPath);

            Assert.That(catalog, Is.Not.Null);
            Assert.That(ContentIdValidator.Validate(catalog.AllContent), Is.Empty);
            Assert.That(catalog.Evidence.Select(item => item.StableId), Is.EquivalentTo(new[]
            {
                "evidence.mert.photo",
                "evidence.mert.terminal_log_0251",
                "evidence.mert.death_time_0236"
            }));

            DeductionData deduction = catalog.Deductions.Single();
            Assert.That(deduction.StableId, Is.EqualTo("deduction.mert.postmortem_terminal"));
            Assert.That(deduction.RequiredEvidenceIds, Is.EquivalentTo(new[]
            {
                "evidence.mert.terminal_log_0251",
                "evidence.mert.death_time_0236"
            }));

            DialogueData recorder = AssetDatabase.LoadAssetAtPath<DialogueData>(
                "Assets/Data/Dialogue/DLG_MertDamagedRecorder.asset");
            Assert.That(recorder.Nodes.First().Speaker.StableId, Is.EqualTo("character.mert.ersoy"));
        }

        [Test]
        public void ProductionBuildSettings_StartAtBootstrapAndExcludeEngineeringScene()
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            Assert.That(enabledScenes, Is.EqualTo(new[] { BootstrapPath, ErenPath, MertPath }));
            Assert.That(enabledScenes.Any(path => path.Contains("/Test/")), Is.False);
        }

        [Test]
        public void BootstrapScene_ContainsCompositionRoot()
        {
            WithScene(BootstrapPath, roots =>
            {
                Assert.That(FindInRoots<GameApplication>(roots), Is.Not.Null);
                Assert.That(FindInRoots<SceneLoader>(roots), Is.Not.Null);
            });
        }

        [Test]
        public void ErenApartment_ContainsPlayerInspectDispatchAndGatedExit()
        {
            WithScene(ErenPath, roots =>
            {
                Assert.That(FindInRoots<PlayerController>(roots), Is.Not.Null);
                Assert.That(FindAllInRoots<InspectInteractable>(roots).Length, Is.GreaterThanOrEqualTo(2));
                Assert.That(FindInRoots<TerminalInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<SceneTransitionInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<OpeningSceneInstaller>(roots), Is.Not.Null);
            });
        }

        [Test]
        public void MertApartment_ContainsEvidenceBoardMemoryTerminalAndDialogue()
        {
            WithScene(MertPath, roots =>
            {
                Assert.That(FindAllInRoots<EvidenceInteractable>(roots).Length, Is.GreaterThanOrEqualTo(2));
                Assert.That(FindInRoots<EvidenceBoardInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<MemoryEvidenceTrigger>(roots), Is.Not.Null);
                Assert.That(FindInRoots<TerminalInteractable>(roots), Is.Not.Null);
                Assert.That(FindInRoots<DialogueInteractable>(roots), Is.Not.Null);
            });
        }

        private static void WithScene(string path, System.Action<GameObject[]> assertion)
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            Assert.That(sceneAsset, Is.Not.Null, $"Production scene is missing at {path}.");
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

        private static T FindInRoots<T>(GameObject[] roots) where T : Component
        {
            return roots.Select(root => root.GetComponentInChildren<T>(true)).FirstOrDefault(item => item != null);
        }

        private static T[] FindAllInRoots<T>(GameObject[] roots) where T : Component
        {
            return roots.SelectMany(root => root.GetComponentsInChildren<T>(true)).ToArray();
        }
    }
}
