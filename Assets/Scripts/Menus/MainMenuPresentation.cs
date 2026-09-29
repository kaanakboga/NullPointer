using System;
using System.Collections;
using NullPointer.Settings;
using NullPointer.Visual;
using UnityEngine;

namespace NullPointer.Menus
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Menus/Main Menu Presentation")]
    public sealed class MainMenuPresentation : MonoBehaviour
    {
        [SerializeField] private UiTransition _titleTransition;
        [SerializeField] private UiTransition _optionsTransition;
        [SerializeField] private UiTransition[] _optionRows = Array.Empty<UiTransition>();
        [SerializeField] private CanvasGroup _departureCurtain;
        [SerializeField] private MenuAtmosphereController _atmosphere;
        private SettingsManager _settings;
        private Coroutine _revealRoutine;
        private Coroutine _departureRoutine;
        private bool _revealComplete;
        private bool _reducedMotion;
        private Action _pendingDeparture;

        public bool RevealComplete => _revealComplete;
        public bool IsDeparting => _departureRoutine != null;

        public void Configure(UiTransition titleTransition, UiTransition optionsTransition,
            CanvasGroup departureCurtain, MenuAtmosphereController atmosphere,
            UiTransition[] optionRows = null)
        {
            _titleTransition = titleTransition;
            _optionsTransition = optionsTransition;
            _departureCurtain = departureCurtain;
            _atmosphere = atmosphere;
            _optionRows = optionRows ?? Array.Empty<UiTransition>();
        }

        public void Initialize(SettingsManager settings)
        {
            if (_settings != null)
            {
                _settings.Changed -= OnSettingsChanged;
            }

            _settings = settings;
            _settings.Changed += OnSettingsChanged;
            ApplySettings(_settings.Current);
            if (_departureCurtain != null)
            {
                _departureCurtain.alpha = 0f;
                _departureCurtain.interactable = false;
                _departureCurtain.blocksRaycasts = false;
            }

            StartReveal();
        }

        public void AccelerateReveal()
        {
            if (_revealComplete)
            {
                return;
            }

            if (_revealRoutine != null)
            {
                StopCoroutine(_revealRoutine);
                _revealRoutine = null;
            }

            _titleTransition?.SnapVisible();
            _optionsTransition?.SnapVisible();
            foreach (UiTransition row in _optionRows)
            {
                row?.SnapVisible();
            }
            _revealComplete = true;
        }

        public bool BeginDeparture(bool continuation, Action completed)
        {
            if (_departureRoutine != null || completed == null)
            {
                return false;
            }

            AccelerateReveal();
            _pendingDeparture = completed;
            _departureRoutine = StartCoroutine(Departure(continuation));
            return true;
        }

        private void StartReveal()
        {
            if (_revealRoutine != null)
            {
                StopCoroutine(_revealRoutine);
            }

            _revealComplete = false;
            _titleTransition?.SnapHidden(false);
            _optionsTransition?.SnapHidden(false);
            foreach (UiTransition row in _optionRows)
            {
                row?.SnapHidden(false);
            }
            _revealRoutine = StartCoroutine(Reveal());
        }

        private IEnumerator Reveal()
        {
            float titleDelay = _reducedMotion ? 0f : 0.32f;
            float optionsDelay = _reducedMotion ? 0.04f : 0.72f;
            yield return WaitUnscaled(titleDelay);
            _titleTransition?.PlayReveal();
            yield return WaitUnscaled(optionsDelay);
            _optionsTransition?.PlayReveal();
            foreach (UiTransition row in _optionRows)
            {
                row?.PlayReveal();
                yield return WaitUnscaled(_reducedMotion ? 0f : 0.06f);
            }
            yield return WaitUnscaled(_reducedMotion ? 0.08f : 0.24f);
            _revealComplete = true;
            _revealRoutine = null;
        }

        private IEnumerator Departure(bool continuation)
        {
            if (_departureCurtain != null)
            {
                _departureCurtain.blocksRaycasts = true;
                float duration = _reducedMotion ? 0.06f : continuation ? 0.18f : 0.3f;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Mathf.Max(0f, Time.unscaledDeltaTime);
                    float t = Mathf.Clamp01(elapsed / duration);
                    _departureCurtain.alpha = t * t;
                    yield return null;
                }

                _departureCurtain.alpha = 1f;
            }

            Action completed = _pendingDeparture;
            _pendingDeparture = null;
            _departureRoutine = null;
            completed?.Invoke();
        }

        private static IEnumerator WaitUnscaled(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Mathf.Max(0f, Time.unscaledDeltaTime);
                yield return null;
            }
        }

        private void OnSettingsChanged(GameSettings settings) => ApplySettings(settings);

        private void ApplySettings(GameSettings settings)
        {
            _reducedMotion = settings.ReducedUiMotion;
            _atmosphere?.SetAccessibility(settings.ReducedUiMotion, settings.ReducedVisualFx);
            foreach (UiTransition row in _optionRows)
            {
                row?.SetReducedMotion(settings.ReducedUiMotion);
            }
        }

        private void OnDisable()
        {
            if (_pendingDeparture != null)
            {
                Action completed = _pendingDeparture;
                _pendingDeparture = null;
                completed.Invoke();
            }
        }

        private void OnDestroy()
        {
            if (_settings != null)
            {
                _settings.Changed -= OnSettingsChanged;
            }
        }
    }
}
