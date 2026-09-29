using System.Collections;
using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Cyber-Noir Panel Presentation")]
    public sealed class CyberNoirPanelPresentation : MonoBehaviour
    {
        [SerializeField] private UiTransition _panelTransition;
        [SerializeField] private CanvasGroup[] _stagedGroups = System.Array.Empty<CanvasGroup>();
        [SerializeField] private RectTransform _scanLine;
        [SerializeField] private CanvasGroup _feedbackPulse;
        [SerializeField, Min(0f)] private float _staggerSeconds = 0.045f;
        [SerializeField, Min(0.1f)] private float _scanPeriod = 3.8f;

        private Coroutine _sequence;
        private Coroutine _scan;
        private Coroutine _feedback;
        private Vector2 _scanStart;
        private Vector2 _feedbackStart;
        private bool _reducedMotion;
        private bool _reducedEffects;

        public bool IsPresenting => _sequence != null;
        public bool ReducedMotion => _reducedMotion;
        public bool ReducedEffects => _reducedEffects;

        public void Configure(
            UiTransition panelTransition,
            CanvasGroup[] stagedGroups,
            RectTransform scanLine,
            CanvasGroup feedbackPulse,
            float staggerSeconds = 0.045f,
            float scanPeriod = 3.8f)
        {
            _panelTransition = panelTransition;
            _stagedGroups = stagedGroups ?? System.Array.Empty<CanvasGroup>();
            _scanLine = scanLine;
            _feedbackPulse = feedbackPulse;
            _staggerSeconds = Mathf.Max(0f, staggerSeconds);
            _scanPeriod = Mathf.Max(0.1f, scanPeriod);
            CacheRestingState();
            ResetTransientState();
        }

        public void SetAccessibility(bool reducedMotion, bool reducedEffects)
        {
            _reducedMotion = reducedMotion;
            _reducedEffects = reducedEffects;
            _panelTransition?.SetReducedMotion(reducedMotion);
            if (reducedEffects && _feedbackPulse != null)
            {
                _feedbackPulse.alpha = 0f;
            }
        }

        public void Reveal()
        {
            if (!Application.isPlaying)
            {
                SnapVisible();
                return;
            }

            StopSequence();
            gameObject.SetActive(true);
            _panelTransition?.PlayReveal();
            _sequence = StartCoroutine(RevealStaged());
            if (!_reducedEffects && _scanLine != null)
            {
                _scan = StartCoroutine(ScanLoop());
            }
        }

        public void SnapVisible()
        {
            StopAllPresentationRoutines();
            gameObject.SetActive(true);
            _panelTransition?.SnapVisible();
            foreach (CanvasGroup group in _stagedGroups)
            {
                if (group != null)
                {
                    group.alpha = 1f;
                }
            }
        }

        public void HideImmediate()
        {
            StopAllPresentationRoutines();
            ResetTransientState();
            _panelTransition?.SnapHidden(false);
        }

        public void HideAnimated()
        {
            StopAllPresentationRoutines();
            ResetTransientState();
            if (!Application.isPlaying || _panelTransition == null || !isActiveAndEnabled || !gameObject.activeInHierarchy)
            {
                _panelTransition?.SnapHidden();
                if (_panelTransition == null)
                {
                    gameObject.SetActive(false);
                }

                return;
            }

            _panelTransition.PlayExit();
        }

        public void PlayInvalidFeedback()
        {
            PlayFeedback(false);
        }

        public void PlaySuccessFeedback()
        {
            PlayFeedback(true);
        }

        private IEnumerator RevealStaged()
        {
            foreach (CanvasGroup group in _stagedGroups)
            {
                if (group != null)
                {
                    group.alpha = 0f;
                }
            }

            float delay = _reducedMotion ? 0f : _staggerSeconds;
            for (int index = 0; index < _stagedGroups.Length; index++)
            {
                CanvasGroup group = _stagedGroups[index];
                if (group != null)
                {
                    group.alpha = 1f;
                }

                if (delay > 0f && index < _stagedGroups.Length - 1)
                {
                    yield return new WaitForSecondsRealtime(delay);
                }
            }

            _sequence = null;
        }

        private IEnumerator ScanLoop()
        {
            float height = (transform as RectTransform)?.rect.height ?? 900f;
            while (true)
            {
                float phase = Mathf.Repeat(Time.unscaledTime / _scanPeriod, 1f);
                _scanLine.anchoredPosition = _scanStart + Vector2.up * Mathf.Lerp(-height * 0.42f, height * 0.42f, phase);
                yield return null;
            }
        }

        private void PlayFeedback(bool success)
        {
            if (!Application.isPlaying)
            {
                if (_feedbackPulse != null)
                {
                    _feedbackPulse.alpha = _reducedEffects ? 0f : success ? 0.2f : 0.12f;
                }

                return;
            }

            if (_feedback != null)
            {
                StopCoroutine(_feedback);
            }

            _feedback = StartCoroutine(FeedbackPulse(success));
        }

        private IEnumerator FeedbackPulse(bool success)
        {
            float duration = success ? 0.48f : 0.22f;
            if (_reducedMotion)
            {
                duration = 0.08f;
            }

            float elapsed = 0f;
            RectTransform rect = _feedbackPulse != null ? _feedbackPulse.transform as RectTransform : null;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float envelope = Mathf.Sin(normalized * Mathf.PI);
                if (_feedbackPulse != null)
                {
                    _feedbackPulse.alpha = _reducedEffects ? 0f : envelope * (success ? 0.72f : 0.42f);
                }

                if (rect != null && !_reducedMotion && !success)
                {
                    rect.anchoredPosition = _feedbackStart + Vector2.right * Mathf.Sin(normalized * Mathf.PI * 4f) * 4f * envelope;
                }

                yield return null;
            }

            if (_feedbackPulse != null)
            {
                _feedbackPulse.alpha = 0f;
            }

            if (rect != null)
            {
                rect.anchoredPosition = _feedbackStart;
            }

            _feedback = null;
        }

        private void CacheRestingState()
        {
            _scanStart = _scanLine != null ? _scanLine.anchoredPosition : Vector2.zero;
            RectTransform feedbackRect = _feedbackPulse != null ? _feedbackPulse.transform as RectTransform : null;
            _feedbackStart = feedbackRect != null ? feedbackRect.anchoredPosition : Vector2.zero;
        }

        private void ResetTransientState()
        {
            if (_scanLine != null)
            {
                _scanLine.anchoredPosition = _scanStart;
            }

            if (_feedbackPulse != null)
            {
                _feedbackPulse.alpha = 0f;
                RectTransform rect = _feedbackPulse.transform as RectTransform;
                if (rect != null)
                {
                    rect.anchoredPosition = _feedbackStart;
                }
            }
        }

        private void StopSequence()
        {
            if (_sequence != null)
            {
                StopCoroutine(_sequence);
                _sequence = null;
            }
        }

        private void StopAllPresentationRoutines()
        {
            StopSequence();
            if (_scan != null)
            {
                StopCoroutine(_scan);
                _scan = null;
            }

            if (_feedback != null)
            {
                StopCoroutine(_feedback);
                _feedback = null;
            }
        }

        private void OnDisable()
        {
            StopAllPresentationRoutines();
            ResetTransientState();
        }
    }
}
