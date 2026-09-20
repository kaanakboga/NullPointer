using System;
using NullPointer.Content;
using NullPointer.Deduction;
using NullPointer.Evidence;
using UnityEngine;

namespace NullPointer.Runtime
{
    [CreateAssetMenu(menuName = "Null Pointer/Runtime/Content Catalog", fileName = "CAT_Content")]
    public sealed class ContentCatalog : ScriptableObject
    {
        [SerializeField] private AuthoredContentAsset[] _allContent = Array.Empty<AuthoredContentAsset>();
        [SerializeField] private EvidenceData[] _evidence = Array.Empty<EvidenceData>();
        [SerializeField] private DeductionData[] _deductions = Array.Empty<DeductionData>();

        public System.Collections.Generic.IReadOnlyList<AuthoredContentAsset> AllContent => _allContent;

        public System.Collections.Generic.IReadOnlyList<EvidenceData> Evidence => _evidence;

        public System.Collections.Generic.IReadOnlyList<DeductionData> Deductions => _deductions;
    }
}
