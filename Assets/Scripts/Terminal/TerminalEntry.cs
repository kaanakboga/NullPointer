using System;
using UnityEngine;

namespace NullPointer.Terminal
{
    [Serializable]
    public sealed class TerminalEntry
    {
        [SerializeField] private string _entryId = string.Empty;
        [SerializeField] private TerminalEntryCategory _category;
        [SerializeField] private string _title = string.Empty;
        [SerializeField, TextArea] private string _body = string.Empty;
        [SerializeField] private string _evidenceId = string.Empty;
        [SerializeField] private string _storyFlagToSet = string.Empty;

        public string EntryId => _entryId;

        public TerminalEntryCategory Category => _category;

        public string Title => _title;

        public string Body => _body;

        public string EvidenceId => _evidenceId;

        public string StoryFlagToSet => _storyFlagToSet;
    }
}
