using System;
using System.Collections.Generic;
using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Dialogue
{
    [Serializable]
    public sealed class DialogueNode
    {
        [SerializeField] private string _nodeId = string.Empty;
        [SerializeField] private CharacterData _speaker;
        [SerializeField, TextArea] private string _text = string.Empty;
        [SerializeField] private string _nextNodeId = string.Empty;
        [SerializeField] private List<DialogueChoice> _choices = new List<DialogueChoice>();
        [SerializeField] private List<DialogueAction> _actions = new List<DialogueAction>();

        public string NodeId => _nodeId;

        public CharacterData Speaker => _speaker;

        public string Text => _text;

        public string NextNodeId => _nextNodeId;

        public IReadOnlyList<DialogueChoice> Choices => _choices;

        public IReadOnlyList<DialogueAction> Actions => _actions;
    }
}
