using System;
using System.Collections.Generic;
using System.Linq;
using NullPointer.Visual;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NullPointer.Editor
{
    public static class ArtSlotPreservation
    {
        public static IReadOnlyDictionary<string, Sprite> Capture(IEnumerable<FinalArtSlot> slots)
        {
            var result = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (FinalArtSlot slot in slots ?? Enumerable.Empty<FinalArtSlot>())
            {
                if (slot == null || !slot.HasFinalArt)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(slot.StableId))
                {
                    throw new InvalidOperationException("A final-art slot with an assigned sprite has no stable ID.");
                }

                if (!result.TryAdd(slot.StableId, slot.FinalArt))
                {
                    throw new InvalidOperationException($"Duplicate final-art slot ID '{slot.StableId}'.");
                }
            }

            return result;
        }

        public static IReadOnlyDictionary<string, Sprite> CaptureScene(string scenePath)
        {
            Scene scene = SceneManager.GetSceneByPath(scenePath);
            bool openedForCapture = false;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
                {
                    return new Dictionary<string, Sprite>(StringComparer.Ordinal);
                }

                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
                openedForCapture = true;
            }

            try
            {
                return Capture(FindSlots(scene));
            }
            finally
            {
                if (openedForCapture)
                {
                    EditorSceneManager.CloseScene(scene, true);
                }
            }
        }

        public static void Restore(Scene scene, IReadOnlyDictionary<string, Sprite> preserved)
        {
            if (preserved == null || preserved.Count == 0)
            {
                return;
            }

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (FinalArtSlot slot in FindSlots(scene))
            {
                if (slot == null || string.IsNullOrWhiteSpace(slot.StableId))
                {
                    continue;
                }

                if (!seen.Add(slot.StableId))
                {
                    throw new InvalidOperationException($"Duplicate final-art slot ID '{slot.StableId}' in rebuilt scene '{scene.path}'.");
                }

                if (preserved.TryGetValue(slot.StableId, out Sprite sprite))
                {
                    slot.SetFinalArt(sprite);
                    EditorUtility.SetDirty(slot);
                }
            }
        }

        private static IEnumerable<FinalArtSlot> FindSlots(Scene scene)
        {
            return scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<FinalArtSlot>(true));
        }
    }
}
