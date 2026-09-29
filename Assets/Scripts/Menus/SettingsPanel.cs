using System;
using System.Linq;
using System.Collections;
using NullPointer.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NullPointer.Visual;

namespace NullPointer.Menus
{
    [AddComponentMenu("Null Pointer/Menus/Settings Panel")]
    public sealed class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider _masterVolume;
        [SerializeField] private Slider _musicVolume;
        [SerializeField] private Slider _sfxVolume;
        [SerializeField] private Slider _textSpeed;
        [SerializeField] private Toggle _fullscreen;
        [SerializeField] private Dropdown _resolution;
        [SerializeField] private Toggle _reducedMotion;
        [SerializeField] private Toggle _reducedFx;
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private UiTransition _transition;
        [SerializeField] private UiTransition _contentTransition;

        private SettingsManager _manager;
        private Action _closed;
        private Resolution[] _resolutions = Array.Empty<Resolution>();
        private Coroutine _contentReveal;

        public void Configure(
            Slider master,
            Slider music,
            Slider sfx,
            Slider textSpeed,
            Toggle fullscreen,
            Dropdown resolution,
            Toggle reducedMotion,
            Toggle reducedFx,
            Button apply,
            Button back)
        {
            _masterVolume = master;
            _musicVolume = music;
            _sfxVolume = sfx;
            _textSpeed = textSpeed;
            _fullscreen = fullscreen;
            _resolution = resolution;
            _reducedMotion = reducedMotion;
            _reducedFx = reducedFx;
            _applyButton = apply;
            _backButton = back;
        }

        public void ConfigurePresentation(UiTransition transition, UiTransition contentTransition = null)
        {
            _transition = transition;
            _contentTransition = contentTransition;
        }

        public void Bind(SettingsManager manager, Action closed)
        {
            _manager = manager;
            _closed = closed;
            _applyButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
            _applyButton.onClick.AddListener(Apply);
            _backButton.onClick.AddListener(Close);
        }

        public void Show(GameSettings settings)
        {
            gameObject.SetActive(true);
            PrepareValues(settings);
            _transition?.SetReducedMotion(settings.ReducedUiMotion);
            _contentTransition?.SetReducedMotion(settings.ReducedUiMotion);
            _transition?.PlayReveal();
            if (_contentReveal != null)
            {
                StopCoroutine(_contentReveal);
            }
            _contentReveal = StartCoroutine(RevealContent(settings.ReducedUiMotion));
            EventSystem.current?.SetSelectedGameObject(_masterVolume.gameObject);
        }

        public void PrepareValues(GameSettings settings)
        {
            _masterVolume.SetValueWithoutNotify(settings.MasterVolume);
            _musicVolume.SetValueWithoutNotify(settings.MusicVolume);
            _sfxVolume.SetValueWithoutNotify(settings.SfxVolume);
            _textSpeed.SetValueWithoutNotify(settings.TextSpeed);
            _fullscreen.SetIsOnWithoutNotify(settings.Fullscreen);
            _reducedMotion.SetIsOnWithoutNotify(settings.ReducedUiMotion);
            _reducedFx.SetIsOnWithoutNotify(settings.ReducedVisualFx);
            PopulateResolutions(settings);
            RefreshPresenters();
        }

        public void Hide()
        {
            if (_contentReveal != null)
            {
                StopCoroutine(_contentReveal);
                _contentReveal = null;
            }
            gameObject.SetActive(false);
        }

        public void Apply()
        {
            Resolution resolution = _resolutions.Length == 0
                ? Screen.currentResolution
                : _resolutions[Mathf.Clamp(_resolution.value, 0, _resolutions.Length - 1)];
            _manager.Update(new GameSettings
            {
                MasterVolume = _masterVolume.value,
                MusicVolume = _musicVolume.value,
                SfxVolume = _sfxVolume.value,
                TextSpeed = _textSpeed.value,
                Fullscreen = _fullscreen.isOn,
                ReducedUiMotion = _reducedMotion.isOn,
                ReducedVisualFx = _reducedFx.isOn,
                ResolutionWidth = resolution.width,
                ResolutionHeight = resolution.height
            });
        }

        public void Close()
        {
            if (_transition != null && gameObject.activeSelf)
            {
                _transition.PlayExit(() => _closed?.Invoke());
            }
            else
            {
                Hide();
                _closed?.Invoke();
            }
        }

        private void PopulateResolutions(GameSettings settings)
        {
            _resolutions = Screen.resolutions
                .GroupBy(value => (value.width, value.height))
                .Select(group => group.Last())
                .ToArray();
            if (_resolutions.Length == 0)
            {
                _resolutions = new[] { Screen.currentResolution };
            }

            _resolution.ClearOptions();
            _resolution.AddOptions(_resolutions
                .Select(value => $"{value.width} × {value.height}")
                .ToList());
            int selected = Array.FindIndex(_resolutions, value =>
                value.width == settings.ResolutionWidth && value.height == settings.ResolutionHeight);
            _resolution.SetValueWithoutNotify(Mathf.Max(0, selected));
            _resolution.RefreshShownValue();
        }

        private void RefreshPresenters()
        {
            RefreshSlider(_masterVolume);
            RefreshSlider(_musicVolume);
            RefreshSlider(_sfxVolume);
            RefreshSlider(_textSpeed);
            RefreshToggle(_fullscreen);
            RefreshToggle(_reducedMotion);
            RefreshToggle(_reducedFx);
            _resolution.GetComponent<CyberNoirDropdownVisual>()?.RefreshImmediate();
        }

        private static void RefreshSlider(Slider slider)
        {
            slider.GetComponent<CyberNoirSliderVisual>()?.RefreshValue(slider.value);
        }

        private static void RefreshToggle(Toggle toggle)
        {
            toggle.GetComponent<CyberNoirToggleVisual>()?.RefreshStatus(toggle.isOn);
        }

        private IEnumerator RevealContent(bool reducedMotion)
        {
            _contentTransition?.SnapHidden(false);
            float delay = reducedMotion ? 0f : 0.08f;
            while (delay > 0f)
            {
                delay -= Time.unscaledDeltaTime;
                yield return null;
            }

            _contentTransition?.PlayReveal();
            _contentReveal = null;
        }
    }
}
