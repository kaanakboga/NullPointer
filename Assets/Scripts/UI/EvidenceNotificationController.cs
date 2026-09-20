using System.Collections;
using NullPointer.Evidence;
using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.UI
{
    [AddComponentMenu("Null Pointer/UI/Evidence Notification Controller")]
    public sealed class EvidenceNotificationController : MonoBehaviour
    {
        [SerializeField] private GameObject _notificationRoot;
        [SerializeField] private Text _notificationLabel;
        [SerializeField, Min(0.1f)] private float _displaySeconds = 2.5f;

        private EvidenceService _evidenceService;
        private Coroutine _hideRoutine;

        public void Configure(GameObject notificationRoot, Text notificationLabel)
        {
            _notificationRoot = notificationRoot;
            _notificationLabel = notificationLabel;
        }

        public void Initialize(EvidenceService evidenceService)
        {
            Unsubscribe();
            _evidenceService = evidenceService;
            if (_evidenceService != null)
            {
                _evidenceService.EvidenceCollected += OnEvidenceCollected;
            }

            _notificationRoot.SetActive(false);
        }

        private void OnEvidenceCollected(EvidenceCollected change)
        {
            _notificationLabel.text = $"KANIT KAYDEDİLDİ\n{change.Evidence.DisplayName}";
            _notificationRoot.SetActive(true);

            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
            }

            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_displaySeconds);
            _notificationRoot.SetActive(false);
            _hideRoutine = null;
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_evidenceService != null)
            {
                _evidenceService.EvidenceCollected -= OnEvidenceCollected;
            }
        }
    }
}
