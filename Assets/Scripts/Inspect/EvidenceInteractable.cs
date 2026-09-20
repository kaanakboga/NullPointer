using NullPointer.Evidence;
using NullPointer.Interaction;
using UnityEngine;

namespace NullPointer.Inspect
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Inspect/Evidence Interactable")]
    public sealed class EvidenceInteractable : MonoBehaviour, IInteractable, IInteractionPromptSource
    {
        [SerializeField] private InspectController _controller;
        [SerializeField] private InspectData _inspection;
        [SerializeField] private EvidenceData _evidence;
        [SerializeField] private MonoBehaviour _collectionFollowUp;
        [SerializeField] private int _interactionPriority = 10;
        [SerializeField] private string _interactionPrompt = "Kanıtı incele";

        private IEvidenceCollectionFollowUp _followUp;

        public bool CanInteract =>
            isActiveAndEnabled && _controller != null && _inspection != null && _evidence != null;

        public int InteractionPriority => _interactionPriority;

        public Transform InteractionTransform => transform;

        public string InteractionPrompt => _interactionPrompt;

        public void Configure(
            InspectController controller,
            InspectData inspection,
            EvidenceData evidence,
            MonoBehaviour collectionFollowUp = null)
        {
            _controller = controller;
            _inspection = inspection;
            _evidence = evidence;
            _collectionFollowUp = collectionFollowUp;
            _followUp = collectionFollowUp as IEvidenceCollectionFollowUp;
        }

        public void Interact()
        {
            if (CanInteract)
            {
                _controller.OpenEvidence(_inspection, _evidence, OnEvidenceCollected);
            }
        }

        private void Awake()
        {
            _followUp = _collectionFollowUp as IEvidenceCollectionFollowUp;
        }

        private void OnEvidenceCollected(EvidenceData evidence)
        {
            _followUp?.OnEvidenceCollected(evidence);
        }
    }
}
