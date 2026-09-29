using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Menu Atmosphere Controller")]
    public sealed class MenuAtmosphereController : MonoBehaviour
    {
        [SerializeField] private RectTransform[] _parallaxLayers = Array.Empty<RectTransform>();
        [SerializeField] private float[] _parallaxStrengths = Array.Empty<float>();
        [SerializeField] private RectTransform[] _rainStreaks = Array.Empty<RectTransform>();
        [SerializeField] private int _farRainCount;
        [SerializeField] private int _midRainCount;
        [SerializeField] private CanvasGroup _haze;
        [SerializeField] private RectTransform _hazeMotion;
        [SerializeField] private RectTransform _title;
        [SerializeField] private CanvasGroup _titleEcho;
        [SerializeField] private RectTransform _titleFault;
        [SerializeField] private CanvasGroup[] _distantLights = Array.Empty<CanvasGroup>();
        [SerializeField] private ScreenOverlayLayer _screenOverlay;
        [SerializeField] private EnvironmentFxLayer _environmentFx;

        private Vector2[] _parallaxOrigins = Array.Empty<Vector2>();
        private Vector2[] _rainOrigins = Array.Empty<Vector2>();
        private Vector2 _hazeOrigin;
        private Vector2 _titleOrigin;
        private bool _reducedMotion;
        private bool _reducedFx;
        private float _clock;

        public bool ReducedMotion => _reducedMotion;
        public bool ReducedFx => _reducedFx;

        public void Configure(
            RectTransform[] parallaxLayers,
            float[] parallaxStrengths,
            RectTransform[] rainStreaks,
            CanvasGroup haze,
            RectTransform hazeMotion,
            RectTransform title,
            CanvasGroup titleEcho,
            ScreenOverlayLayer screenOverlay,
            EnvironmentFxLayer environmentFx,
            int farRainCount = 0,
            int midRainCount = 0,
            RectTransform titleFault = null,
            CanvasGroup[] distantLights = null)
        {
            _parallaxLayers = parallaxLayers ?? Array.Empty<RectTransform>();
            _parallaxStrengths = parallaxStrengths ?? Array.Empty<float>();
            _rainStreaks = rainStreaks ?? Array.Empty<RectTransform>();
            _haze = haze;
            _hazeMotion = hazeMotion;
            _title = title;
            _titleEcho = titleEcho;
            _titleFault = titleFault;
            _farRainCount = Mathf.Max(0, farRainCount);
            _midRainCount = Mathf.Max(0, midRainCount);
            _distantLights = distantLights ?? Array.Empty<CanvasGroup>();
            _screenOverlay = screenOverlay;
            _environmentFx = environmentFx;
            CaptureOrigins();
            ApplyAccessibility();
        }

        public void SetAccessibility(bool reducedMotion, bool reducedFx)
        {
            _reducedMotion = reducedMotion;
            _reducedFx = reducedFx;
            ApplyAccessibility();
        }

        private void Awake()
        {
            CaptureOrigins();
        }

        private void Update()
        {
            float delta = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            _clock += delta;
            UpdateParallax();
            UpdateRain(delta);
            UpdateHaze();
            UpdateDistantLights();
            UpdateTitleInstability();
        }

        private void UpdateParallax()
        {
            Vector2 normalized = Vector2.zero;
            if (!_reducedMotion && Pointer.current != null)
            {
                Vector2 position = Pointer.current.position.ReadValue();
                normalized = new Vector2(
                    Screen.width > 0 ? position.x / Screen.width - 0.5f : 0f,
                    Screen.height > 0 ? position.y / Screen.height - 0.5f : 0f);
            }

            float drift = _reducedMotion ? 0f : Mathf.Sin(_clock * 0.17f) * 0.18f;
            int count = Mathf.Min(_parallaxLayers.Length, _parallaxOrigins.Length);
            for (int index = 0; index < count; index++)
            {
                RectTransform layer = _parallaxLayers[index];
                if (layer == null)
                {
                    continue;
                }

                float strength = index < _parallaxStrengths.Length ? _parallaxStrengths[index] : 8f;
                Vector2 offset = new Vector2(normalized.x + drift, normalized.y * 0.55f) * strength;
                layer.anchoredPosition = Vector2.Lerp(
                    layer.anchoredPosition,
                    _parallaxOrigins[index] + offset,
                    1f - Mathf.Exp(-Time.unscaledDeltaTime * 3.5f));
            }
        }

        private void UpdateRain(float delta)
        {
            if (_reducedFx)
            {
                return;
            }

            for (int index = 0; index < _rainStreaks.Length; index++)
            {
                RectTransform streak = _rainStreaks[index];
                if (streak == null)
                {
                    continue;
                }

                int band = index < _farRainCount ? 0 : index < _farRainCount + _midRainCount ? 1 : 2;
                float speed = band == 0 ? 170f : band == 1 ? 430f : 820f;
                float drift = band == 0 ? 22f : band == 1 ? 48f : 88f;
                float burstPhase = Mathf.Repeat(_clock + index * 0.37f, 8.6f);
                float burst = band > 0 && burstPhase < 0.42f ? 1.55f : 1f;
                Vector2 position = streak.anchoredPosition;
                position.y -= speed * burst * delta;
                position.x -= drift * burst * delta;
                if (position.y < -610f)
                {
                    Vector2 origin = index < _rainOrigins.Length ? _rainOrigins[index] : Vector2.zero;
                    position.y = 610f + Mathf.Repeat(origin.y + index * 47f, 180f);
                    position.x = origin.x;
                }

                streak.anchoredPosition = position;
            }
        }

        private void UpdateDistantLights()
        {
            for (int index = 0; index < _distantLights.Length; index++)
            {
                CanvasGroup light = _distantLights[index];
                if (light == null)
                {
                    continue;
                }

                if (_reducedFx)
                {
                    light.alpha = 0.18f;
                    continue;
                }

                float wave = Mathf.Sin(_clock * (0.19f + index * 0.017f) + index * 1.73f);
                float passing = Mathf.Repeat(_clock + index * 2.1f, 11.4f) < 0.7f ? 0.18f : 0f;
                light.alpha = 0.28f + wave * 0.08f + passing;
            }
        }

        private void UpdateHaze()
        {
            if (_hazeMotion != null)
            {
                float movement = _reducedMotion ? 0f : Mathf.Sin(_clock * 0.12f) * 38f;
                _hazeMotion.anchoredPosition = _hazeOrigin + Vector2.right * movement;
            }

            if (_haze != null)
            {
                _haze.alpha = _reducedFx ? 0.12f : 0.26f + Mathf.Sin(_clock * 0.21f) * 0.035f;
            }
        }

        private void UpdateTitleInstability()
        {
            if (_title == null || _titleEcho == null)
            {
                return;
            }

            float phase = Mathf.Repeat(_clock, 6.7f);
            bool pulse = !_reducedFx && phase > 6.51f;
            _title.anchoredPosition = _titleOrigin + (pulse
                ? new Vector2(Mathf.Sin(phase * 170f) * 2.5f, 0f)
                : Vector2.zero);
            _titleEcho.alpha = pulse ? 0.24f : 0f;
            if (_titleFault != null)
            {
                float displacement = pulse ? Mathf.Sin(phase * 190f) * 12f : 0f;
                _titleFault.anchoredPosition = new Vector2(displacement, _titleFault.anchoredPosition.y);
            }
        }

        private void CaptureOrigins()
        {
            _parallaxOrigins = new Vector2[_parallaxLayers.Length];
            for (int index = 0; index < _parallaxLayers.Length; index++)
            {
                _parallaxOrigins[index] = _parallaxLayers[index] != null
                    ? _parallaxLayers[index].anchoredPosition
                    : Vector2.zero;
            }

            _rainOrigins = new Vector2[_rainStreaks.Length];
            for (int index = 0; index < _rainStreaks.Length; index++)
            {
                _rainOrigins[index] = _rainStreaks[index] != null
                    ? _rainStreaks[index].anchoredPosition
                    : Vector2.zero;
            }

            _hazeOrigin = _hazeMotion != null ? _hazeMotion.anchoredPosition : Vector2.zero;
            _titleOrigin = _title != null ? _title.anchoredPosition : Vector2.zero;
        }

        private void ApplyAccessibility()
        {
            _screenOverlay?.SetIntensity(_reducedFx ? 0.25f : 1f);
            _environmentFx?.SetActive(true);
            for (int index = 0; index < _rainStreaks.Length; index++)
            {
                if (_rainStreaks[index] != null)
                {
                    bool far = index < _farRainCount;
                    bool midSample = index < _farRainCount + _midRainCount && index % 4 == 0;
                    _rainStreaks[index].gameObject.SetActive(!_reducedFx || far && index % 2 == 0 || midSample);
                }
            }
        }
    }
}
