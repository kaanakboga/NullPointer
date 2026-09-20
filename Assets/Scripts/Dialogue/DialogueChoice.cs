using System;
using System.Collections.Generic;
using UnityEngine;

namespace NullPointer.Dialogue
{
    [Serializable]
    public sealed class DialogueChoice
    {
        [SerializeField] private string _text = string.Empty;
        [SerializeField] private string _requiredStoryFlag = string.Empty;
        [SerializeField] private string _requiredEvidenceId = string.Empty;
        [SerializeField] private string _nextNodeId = string.Empty;
        [SerializeField] private List<DialogueAction> _actions = new List<DialogueAction>();

        public string Text => _text;

        public string RequiredStoryFlag => _requiredStoryFlag;

        public string RequiredEvidenceId => _requiredEvidenceId;

        public string NextNodeId => _nextNodeId;

        public IReadOnlyList<DialogueAction> Actions => _actions;
    }
}
