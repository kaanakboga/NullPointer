using System;
using NullPointer.Content;
using NullPointer.Deduction;
using NullPointer.Evidence;
using NullPointer.Interrogation;
using NullPointer.Journal;
using NullPointer.Progression;
using UnityEngine;

namespace NullPointer.Runtime
{
    [CreateAssetMenu(menuName = "Null Pointer/Runtime/Content Catalog", fileName = "CAT_Content")]
    public sealed class ContentCatalog : ScriptableObject
    {
        [SerializeField] private AuthoredContentAsset[] _allContent = Array.Empty<AuthoredContentAsset>();
        [SerializeField] private EvidenceData[] _evidence = Array.Empty<EvidenceData>();
        [SerializeField] private DeductionData[] _deductions = Array.Empty<DeductionData>();
        [SerializeField] private ObjectiveData[] _objectives = Array.Empty<ObjectiveData>();
        [SerializeField] private CheckpointData[] _checkpoints = Array.Empty<CheckpointData>();
        [SerializeField] private JournalEntryData[] _journalEntries = Array.Empty<JournalEntryData>();
        [SerializeField] private InterrogationClaimData[] _interrogationClaims =
            Array.Empty<InterrogationClaimData>();

        public System.Collections.Generic.IReadOnlyList<AuthoredContentAsset> AllContent => _allContent;

        public System.Collections.Generic.IReadOnlyList<EvidenceData> Evidence => _evidence;

        public System.Collections.Generic.IReadOnlyList<DeductionData> Deductions => _deductions;

        public System.Collections.Generic.IReadOnlyList<ObjectiveData> Objectives => _objectives;

        public System.Collections.Generic.IReadOnlyList<CheckpointData> Checkpoints => _checkpoints;

        public System.Collections.Generic.IReadOnlyList<JournalEntryData> JournalEntries => _journalEntries;

        public System.Collections.Generic.IReadOnlyList<InterrogationClaimData> InterrogationClaims =>
            _interrogationClaims;
    }
}
