using System;
using NullPointer.Settings;
using UnityEngine;

namespace NullPointer.Visual
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Null Pointer/Visual/Environment Presentation Controller")]
    public sealed class EnvironmentPresentationController : MonoBehaviour
    {
        [SerializeField] private Camera _worldCamera;
        [SerializeField] private Transform _player;
        [SerializeField] private Transform[] _parallaxLayers = Array.Empty<Transform>();
        [SerializeField] private float[] _parallaxStrengths = Array.Empty<float>();
        [SerializeField] private Transform[] _rainStreaks = Array.Empty<Transform>();
        [SerializeField] private SpriteRenderer[] _atmosphereRenderers = Array.Empty<SpriteRenderer>();
        [SerializeField] private SpriteRenderer[] _signalRenderers = Array.Empty<SpriteRenderer>();
        [SerializeField] private int _farRainCount;
        [SerializeField] private float _rainTop = 8.5f;
        [SerializeField] private float _rainBottom = -4.5f;

        private Vector3[] _parallaxOrigins = Array.Empty<Vector3>();
        private Vector3[] _rainOrigins = Array.Empty<Vector3>();
        private Color[] _atmosphereColors = Array.Empty<Color>();
        private Color[] _signalColors = Array.Empty<Color>();
        private SettingsManager _settings;
        private float _clock;
        private bool _reducedMotion;
        private bool _reducedEffects;

        public bool ReducedMotion => _reducedMotion;
        public bool ReducedEffects => _reducedEffects;
        public int RainStreakCount => _rainStreaks.Length;

        public void Configure(
            Camera worldCamera,
            Transform player,
            Transform[] parallaxLayers,
            float[] parallaxStrengths,
            Transform[] rainStreaks,
            SpriteRenderer[] atmosphereRenderers,
            SpriteRenderer[] signalRenderers,
            int farRainCount,
            float rainTop = 8.5f,
            float rainBottom = -4.5f)
        {
            _worldCamera = worldCamera;
            _player = player;
            _parallaxLayers = parallaxLayers ?? Array.Empty<Transform>();
            _parallaxStrengths = parallaxStrengths ?? Array.Empty<float>();
            _rainStreaks = rainStreaks ?? Array.Empty<Transform>();
            _atmosphereRenderers = atmosphereRenderers ?? Array.Empty<SpriteRenderer>();
            _signalRenderers = signalRenderers ?? Array.Empty<SpriteRenderer>();
            _farRainCount = Mathf.Clamp(farRainCount, 0, _rainStreaks.Length);
            _rainTop = rainTop;
            _rainBottom = rainBottom;
            CacheInitialState();
        }

        public void Initialize(SettingsManager settings)
        {
            if (_settings != null)
            {
                _settings.Changed -= OnSettingsChanged;
            }

            _settings = settings;
            if (_settings != null)
            {
                _settings.Changed += OnSettingsChanged;
                ApplyAccessibility(_settings.Current.ReducedUiMotion, _settings.Current.ReducedVisualFx);
            }
        }

        public void ApplyAccessibility(bool reducedMotion, bool reducedEffects)
        {
            _reducedMotion = reducedMotion;
            _reducedEffects = reducedEffects;
            ApplyFrame();
        }

        public void SetPreviewTime(float time)
        {
            _clock = Mathf.Max(0f, time);
            ApplyFrame();
        }

        private void Awake()
        {
            if (_parallaxOrigins.Length != _parallaxLayers.Length || _rainOrigins.Length != _rainStreaks.Length)
            {
                CacheInitialState();
            }
        }

        private void Update()
        {
            _clock += Time.unscaledDeltaTime;
            ApplyFrame();
        }

        private void OnDisable()
        {
            ResetParallax();
        }

        private void OnDestroy()
        {
            if (_settings != null)
            {
                _settings.Changed -= OnSettingsChanged;
            }
        }

        private void OnSettingsChanged(GameSettings settings)
        {
            ApplyAccessibility(settings.ReducedUiMotion, settings.ReducedVisualFx);
        }

        private void CacheInitialState()
        {
            _parallaxOrigins = new Vector3[_parallaxLayers.Length];
            for (int index = 0; index < _parallaxLayers.Length; index++)
            {
                if (_parallaxLayers[index] != null)
                {
                    _parallaxOrigins[index] = _parallaxLayers[index].localPosition;
                }
            }

            _rainOrigins = new Vector3[_rainStreaks.Length];
            for (int index = 0; index < _rainStreaks.Length; index++)
            {
                if (_rainStreaks[index] != null)
                {
                    _rainOrigins[index] = _rainStreaks[index].localPosition;
                }
            }

            _atmosphereColors = CacheColors(_atmosphereRenderers);
            _signalColors = CacheColors(_signalRenderers);
        }

        private void ApplyFrame()
        {
            if (_parallaxOrigins.Length != _parallaxLayers.Length ||
                _rainOrigins.Length != _rainStreaks.Length ||
                _atmosphereColors.Length != _atmosphereRenderers.Length ||
                _signalColors.Length != _signalRenderers.Length)
            {
                CacheInitialState();
            }

            ApplyParallax();
            ApplyRain();
            ApplyAtmosphere();
        }

        private void ApplyParallax()
        {
            float playerX = _player != null ? _player.position.x : 0f;
            float cameraX = _worldCamera != null ? _worldCamera.transform.position.x : 0f;
            float driver = Mathf.Clamp(playerX - cameraX, -7f, 7f);
            for (int index = 0; index < _parallaxLayers.Length; index++)
            {
                Transform layer = _parallaxLayers[index];
                if (layer == null)
                {
                    continue;
                }

                float strength = index < _parallaxStrengths.Length ? _parallaxStrengths[index] : 0f;
                float offset = _reducedMotion ? 0f : -driver * strength;
                layer.localPosition = _parallaxOrigins[index] + Vector3.right * offset;
            }
        }

        private void ApplyRain()
        {
            float span = Mathf.Max(0.01f, _rainTop - _rainBottom);
            for (int index = 0; index < _rainStreaks.Length; index++)
            {
                Transform streak = _rainStreaks[index];
                if (streak == null)
                {
                    continue;
                }

                bool sampledOut = _reducedEffects && index % 3 != 0;
                streak.gameObject.SetActive(!sampledOut);
                if (sampledOut)
                {
                    continue;
                }

                float speed = index < _farRainCount ? 1.25f : 2.45f;
                float time = _reducedMotion ? 0f : _clock;
                float localY = _rainTop - Mathf.Repeat(time * speed + index * 0.731f, span);
                Vector3 origin = _rainOrigins[index];
                streak.localPosition = new Vector3(origin.x + (localY - origin.y) * 0.085f, localY, origin.z);
            }
        }

        private void ApplyAtmosphere()
        {
            float motionScale = _reducedMotion ? 0f : 1f;
            float effectsScale = _reducedEffects ? 0.35f : 1f;
            for (int index = 0; index < _atmosphereRenderers.Length; index++)
            {
                SpriteRenderer renderer = _atmosphereRenderers[index];
                if (renderer == null || index >= _atmosphereColors.Length)
                {
                    continue;
                }

                Color baseColor = _atmosphereColors[index];
                float pulse = 0.82f + Mathf.Sin(_clock * (0.22f + index * 0.031f)) * 0.18f * motionScale;
                baseColor.a *= pulse * effectsScale;
                renderer.color = baseColor;
            }

            for (int index = 0; index < _signalRenderers.Length; index++)
            {
                SpriteRenderer renderer = _signalRenderers[index];
                if (renderer == null || index >= _signalColors.Length)
                {
                    continue;
                }

                Color baseColor = _signalColors[index];
                float fault = Mathf.Sin(_clock * 0.63f + index * 1.91f) > 0.92f ? 0.42f : 1f;
                baseColor.a *= _reducedEffects ? 0.55f : Mathf.Lerp(1f, fault, motionScale);
                renderer.color = baseColor;
            }
        }

        private void ResetParallax()
        {
            for (int index = 0; index < _parallaxLayers.Length; index++)
            {
                if (_parallaxLayers[index] != null && index < _parallaxOrigins.Length)
                {
                    _parallaxLayers[index].localPosition = _parallaxOrigins[index];
                }
            }
        }

        private static Color[] CacheColors(SpriteRenderer[] renderers)
        {
            Color[] colors = new Color[renderers.Length];
            for (int index = 0; index < renderers.Length; index++)
            {
                colors[index] = renderers[index] != null ? renderers[index].color : Color.clear;
            }

            return colors;
        }
    }
}
