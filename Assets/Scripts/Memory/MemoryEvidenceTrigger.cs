using NullPointer.Evidence;
using NullPointer.Inspect;
using UnityEngine;

namespace NullPointer.Memory
{
    [AddComponentMenu("Null Pointer/Memory/Memory Evidence Trigger")]
    public sealed class MemoryEvidenceTrigger : MonoBehaviour, IEvidenceCollectionFollowUp
    {
        [SerializeField] private MemoryController _controller;
        [SerializeField] private MemoryData _memory;

        public MemoryData Memory => _memory;

        public void Configure(MemoryController controller, MemoryData memory)
        {
            _controller = controller;
            _memory = memory;
        }

        public void OnEvidenceCollected(EvidenceData evidence)
        {
            if (_controller != null && _memory != null)
            {
                _controller.Play(_memory);
            }
        }
    }
}
