using System.Collections.Generic;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using NullPointer.Evidence;
using NullPointer.Memory;
using UnityEditor;
using UnityEngine;

namespace NullPointer.Tests.EditMode
{
    internal static class AuthoredAssetTestFactory
    {
        public static EvidenceData CreateEvidence(string stableId, string displayName = "Test Evidence")
        {
            EvidenceData evidence = ScriptableObject.CreateInstance<EvidenceData>();
            SetString(evidence, "_stableId", stableId);
            SetString(evidence, "_displayName", displayName);
            return evidence;
        }

        public static DeductionData CreateDeduction(
            string stableId,
            IReadOnlyList<string> requiredEvidenceIds,
            string storyFlag = "")
        {
            DeductionData deduction = ScriptableObject.CreateInstance<DeductionData>();
            var serialized = new SerializedObject(deduction);
            serialized.FindProperty("_stableId").stringValue = stableId;
            serialized.FindProperty("_resultTitle").stringValue = "Test Conclusion";
            SerializedProperty required = serialized.FindProperty("_requiredEvidenceIds");
            required.arraySize = requiredEvidenceIds.Count;
            for (int index = 0; index < requiredEvidenceIds.Count; index++)
            {
                required.GetArrayElementAtIndex(index).stringValue = requiredEvidenceIds[index];
            }

            SerializedProperty flags = serialized.FindProperty("_storyFlagsToSet");
            flags.arraySize = string.IsNullOrWhiteSpace(storyFlag) ? 0 : 1;
            if (flags.arraySize == 1)
            {
                flags.GetArrayElementAtIndex(0).stringValue = storyFlag;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return deduction;
        }

        public static DialogueData CreateLinearDialogue(string invalidNextNodeId = "")
        {
            DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
            var serialized = new SerializedObject(dialogue);
            serialized.FindProperty("_stableId").stringValue = "dialogue.test.linear";
            serialized.FindProperty("_startNodeId").stringValue = "start";
            SerializedProperty nodes = serialized.FindProperty("_nodes");
            nodes.arraySize = string.IsNullOrEmpty(invalidNextNodeId) ? 2 : 1;

            SerializedProperty first = nodes.GetArrayElementAtIndex(0);
            first.FindPropertyRelative("_nodeId").stringValue = "start";
            first.FindPropertyRelative("_text").stringValue = "First";
            first.FindPropertyRelative("_nextNodeId").stringValue =
                string.IsNullOrEmpty(invalidNextNodeId) ? "end" : invalidNextNodeId;

            if (nodes.arraySize == 2)
            {
                SerializedProperty second = nodes.GetArrayElementAtIndex(1);
                second.FindPropertyRelative("_nodeId").stringValue = "end";
                second.FindPropertyRelative("_text").stringValue = "Last";
                second.FindPropertyRelative("_nextNodeId").stringValue = string.Empty;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return dialogue;
        }

        public static DialogueData CreateConditionalChoiceDialogue()
        {
            DialogueData dialogue = ScriptableObject.CreateInstance<DialogueData>();
            var serialized = new SerializedObject(dialogue);
            serialized.FindProperty("_stableId").stringValue = "dialogue.test.conditional";
            serialized.FindProperty("_startNodeId").stringValue = "start";
            SerializedProperty nodes = serialized.FindProperty("_nodes");
            nodes.arraySize = 3;

            SerializedProperty start = nodes.GetArrayElementAtIndex(0);
            start.FindPropertyRelative("_nodeId").stringValue = "start";
            start.FindPropertyRelative("_text").stringValue = "Choose";
            SerializedProperty choices = start.FindPropertyRelative("_choices");
            choices.arraySize = 2;
            SerializedProperty locked = choices.GetArrayElementAtIndex(0);
            locked.FindPropertyRelative("_text").stringValue = "Evidence route";
            locked.FindPropertyRelative("_requiredEvidenceId").stringValue = "evidence.test.key";
            locked.FindPropertyRelative("_nextNodeId").stringValue = "evidence";
            SerializedProperty fallback = choices.GetArrayElementAtIndex(1);
            fallback.FindPropertyRelative("_text").stringValue = "Fallback route";
            fallback.FindPropertyRelative("_nextNodeId").stringValue = "fallback";

            SerializedProperty evidenceNode = nodes.GetArrayElementAtIndex(1);
            evidenceNode.FindPropertyRelative("_nodeId").stringValue = "evidence";
            evidenceNode.FindPropertyRelative("_text").stringValue = "Evidence branch";
            SerializedProperty fallbackNode = nodes.GetArrayElementAtIndex(2);
            fallbackNode.FindPropertyRelative("_nodeId").stringValue = "fallback";
            fallbackNode.FindPropertyRelative("_text").stringValue = "Fallback branch";

            serialized.ApplyModifiedPropertiesWithoutUndo();
            return dialogue;
        }

        public static MemoryData CreateMemory(string stableId)
        {
            MemoryData memory = ScriptableObject.CreateInstance<MemoryData>();
            SetString(memory, "_stableId", stableId);
            return memory;
        }

        private static void SetString(Object target, string propertyName, string value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(propertyName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
