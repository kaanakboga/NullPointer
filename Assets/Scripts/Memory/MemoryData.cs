using System.Collections.Generic;
using NullPointer.Content;
using UnityEngine;

namespace NullPointer.Memory
{
    [CreateAssetMenu(menuName = "Null Pointer/Content/Memory", fileName = "MEM_NewMemory")]
    public sealed class MemoryData : AuthoredContentAsset
    {
        [SerializeField] private string _title = string.Empty;
        [SerializeField] private List<MemoryBeat> _beats = new List<MemoryBeat>();

        public string Title => _title;

        public IReadOnlyList<MemoryBeat> Beats => _beats;
    }
}
