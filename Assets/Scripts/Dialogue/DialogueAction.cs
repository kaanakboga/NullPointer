using System;
using UnityEngine;

namespace NullPointer.Dialogue
{
    [Serializable]
    public sealed class DialogueAction
    {
        [SerializeField] private DialogueActionType _actionType;
        [SerializeField] private string _targetId = string.Empty;
        [SerializeField] private string _parameter = string.Empty;

        public DialogueActionType ActionType => _actionType;

        public string TargetId => _targetId;

        public string Parameter => _parameter;
    }
}
