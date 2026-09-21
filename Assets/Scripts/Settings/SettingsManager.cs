using System;
using UnityEngine;

namespace NullPointer.Settings
{
    public sealed class SettingsManager
    {
        private readonly ISettingsStorage _storage;

        public SettingsManager(ISettingsStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            Current = LoadOrDefault();
        }

        public event Action<GameSettings> Changed;

        public GameSettings Current { get; private set; }

        public void Update(GameSettings settings, bool applyDisplay = true)
        {
            Current = Sanitize(settings ?? new GameSettings());
            _storage.Write(JsonUtility.ToJson(Current, true));
            Apply(applyDisplay);
            Changed?.Invoke(Current);
        }

        public void Apply(bool applyDisplay = true)
        {
            AudioListener.volume = Current.MasterVolume;
            if (applyDisplay && Current.ResolutionWidth > 0 && Current.ResolutionHeight > 0)
            {
                Screen.SetResolution(
                    Current.ResolutionWidth,
                    Current.ResolutionHeight,
                    Current.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
            }
        }

        private GameSettings LoadOrDefault()
        {
            if (!_storage.Exists)
            {
                return new GameSettings();
            }

            try
            {
                GameSettings settings = JsonUtility.FromJson<GameSettings>(_storage.Read());
                return settings == null || settings.SchemaVersion != GameSettings.CurrentSchemaVersion
                    ? new GameSettings()
                    : Sanitize(settings);
            }
            catch
            {
                return new GameSettings();
            }
        }

        private static GameSettings Sanitize(GameSettings settings)
        {
            settings.SchemaVersion = GameSettings.CurrentSchemaVersion;
            settings.MasterVolume = Mathf.Clamp01(settings.MasterVolume);
            settings.MusicVolume = Mathf.Clamp01(settings.MusicVolume);
            settings.SfxVolume = Mathf.Clamp01(settings.SfxVolume);
            settings.ResolutionWidth = Mathf.Max(640, settings.ResolutionWidth);
            settings.ResolutionHeight = Mathf.Max(360, settings.ResolutionHeight);
            settings.TextSpeed = Mathf.Clamp(settings.TextSpeed, 0f, 0.08f);
            return settings;
        }
    }
}
