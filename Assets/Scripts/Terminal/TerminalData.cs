using System.Collections.Generic;
using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Terminal
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Terminal", fileName = "TERM_NewTerminal")]
    public sealed class TerminalData : AuthoredContentAsset
    {
        [SerializeField] private string _menuTitle = string.Empty;
        [SerializeField] private List<TerminalEntry> _entries = new List<TerminalEntry>();

        public string MenuTitle => _menuTitle;

        public IReadOnlyList<TerminalEntry> Entries => _entries;
    }
}
