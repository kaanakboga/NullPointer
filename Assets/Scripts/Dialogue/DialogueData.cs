using System.Collections.Generic;
using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Dialogue
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Dialogue", fileName = "DLG_NewDialogue")]
    public sealed class DialogueData : AuthoredContentAsset
    {
        [SerializeField] private string _startNodeId = string.Empty;
        [SerializeField] private List<DialogueNode> _nodes = new List<DialogueNode>();

        public string StartNodeId => _startNodeId;

        public IReadOnlyList<DialogueNode> Nodes => _nodes;
    }
}
