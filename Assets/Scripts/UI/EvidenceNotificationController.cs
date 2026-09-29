using System.Collections;
using NullPointer.Evidence;
using NullPointer.Visual;
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
        [SerializeField] private Text _categoryLabel;
        [SerializeField] private Text _metadataLabel;
        [SerializeField] private Image _thumbnail;
        [SerializeField] private GameObject _thumbnailFallback;
        [SerializeField] private CyberNoirPanelPresentation _presentation;

        private EvidenceService _evidenceService;
        private Coroutine _hideRoutine;

        public void Configure(GameObject notificationRoot, Text notificationLabel)
        {
            _notificationRoot = notificationRoot;
            _notificationLabel = notificationLabel;
        }

        public void ConfigurePresentation(
            CyberNoirPanelPresentation presentation,
            Text categoryLabel,
            Text metadataLabel,
            Image thumbnail,
            GameObject thumbnailFallback = null,
            float displaySeconds = 1.15f)
        {
            _presentation = presentation;
            _categoryLabel = categoryLabel;
            _metadataLabel = metadataLabel;
            _thumbnail = thumbnail;
            _thumbnailFallback = thumbnailFallback;
            _displaySeconds = Mathf.Clamp(displaySeconds, 0.7f, 1.4f);
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
            _notificationLabel.text = change.Evidence.DisplayName;
            if (_categoryLabel != null)
            {
                _categoryLabel.text = "KANIT KAYDEDİLDİ / " + change.Evidence.EvidenceType.ToString().ToUpperInvariant();
            }

            if (_metadataLabel != null)
            {
                _metadataLabel.text = change.Evidence.IsCritical
                    ? "KRİTİK BAĞLANTI / DOSYA İNDEKSİ GÜNCELLENDİ"
                    : "DOSYA İNDEKSİ GÜNCELLENDİ";
            }

            if (_thumbnail != null)
            {
                _thumbnail.sprite = change.Evidence.Icon;
                _thumbnail.enabled = change.Evidence.Icon != null;
                _thumbnailFallback?.SetActive(change.Evidence.Icon == null);
            }
            _notificationRoot.SetActive(true);
            _presentation?.Reveal();

            if (!Application.isPlaying)
            {
                return;
            }

            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
            }

            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_displaySeconds);
            if (_presentation != null)
            {
                _presentation.HideAnimated();
            }
            else
            {
                _notificationRoot.SetActive(false);
            }

            _hideRoutine = null;
        }

        private void OnDisable()
        {
            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
                _hideRoutine = null;
            }

            _presentation?.HideImmediate();
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
