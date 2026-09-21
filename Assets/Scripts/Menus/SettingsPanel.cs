using System;
using System.Linq;
using NullPointer.Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        [SerializeField] private Button _applyButton;
        [SerializeField] private Button _backButton;

        private SettingsManager _manager;
        private Action _closed;
        private Resolution[] _resolutions = Array.Empty<Resolution>();

        public void Configure(
            Slider master,
            Slider music,
            Slider sfx,
            Slider textSpeed,
            Toggle fullscreen,
            Dropdown resolution,
            Button apply,
            Button back)
        {
            _masterVolume = master;
            _musicVolume = music;
            _sfxVolume = sfx;
            _textSpeed = textSpeed;
            _fullscreen = fullscreen;
            _resolution = resolution;
            _applyButton = apply;
            _backButton = back;
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
            _masterVolume.SetValueWithoutNotify(settings.MasterVolume);
            _musicVolume.SetValueWithoutNotify(settings.MusicVolume);
            _sfxVolume.SetValueWithoutNotify(settings.SfxVolume);
            _textSpeed.SetValueWithoutNotify(settings.TextSpeed);
            _fullscreen.SetIsOnWithoutNotify(settings.Fullscreen);
            PopulateResolutions(settings);
            EventSystem.current?.SetSelectedGameObject(_masterVolume.gameObject);
        }

        public void Hide()
        {
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
                ResolutionWidth = resolution.width,
                ResolutionHeight = resolution.height
            });
        }

        public void Close()
        {
            Hide();
            _closed?.Invoke();
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
        }
    }
}
