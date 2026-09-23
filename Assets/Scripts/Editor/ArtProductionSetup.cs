using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Editor
{
    public static class ArtProductionSetup
    {
        private static readonly string[] SortingLayerNames =
        {
            "Default",
            "Background",
            "Environment",
            "PropsBack",
            "Characters",
            "PropsFront",
            "Effects",
            "Foreground",
            "WorldUI"
        };

        [MenuItem("Null Pointer/Art/Apply Production Import and Sorting Standards")]
        public static void Configure()
        {
            UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (assets.Length == 0)
            {
                throw new InvalidOperationException("Unity TagManager project settings could not be loaded.");
            }

            var serialized = new SerializedObject(assets[0]);
            SerializedProperty layers = serialized.FindProperty("m_SortingLayers");
            var existing = new HashSet<string>(StringComparer.Ordinal);
            for (int index = 0; index < layers.arraySize; index++)
            {
                SerializedProperty layer = layers.GetArrayElementAtIndex(index);
                existing.Add(layer.FindPropertyRelative("name").stringValue);
            }

            foreach (string name in SortingLayerNames)
            {
                if (existing.Contains(name))
                {
                    continue;
                }

                int index = layers.arraySize;
                layers.arraySize = index + 1;
                SerializedProperty layer = layers.GetArrayElementAtIndex(index);
                layer.FindPropertyRelative("name").stringValue = name;
                layer.FindPropertyRelative("uniqueID").longValue = string.Equals(name, "Default", StringComparison.Ordinal)
                    ? 0
                    : unchecked((uint)Animator.StringToHash("NullPointer.SortingLayer." + name));
                layer.FindPropertyRelative("locked").boolValue = false;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[Art] Production sorting layers and automatic sprite import rules are configured.");
        }

        public static void ConfigureFromCommandLine()
        {
            Configure();
        }
    }
}
