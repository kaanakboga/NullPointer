using UnityEngine;

namespace NullPointer.Visual
{
    [CreateAssetMenu(menuName = "Null Pointer/Visual/Memory Distortion Profile", fileName = "VFX_MemoryDistortion")]
    public sealed class MemoryDistortionProfile : ScriptableObject
    {
        [SerializeField, Min(0.05f)] private float _duration = 1.8f;
        [SerializeField, Range(0f, 1f)] private float _intensity = 0.55f;
        [SerializeField, Range(0f, 24f)] private float _chromaticOffset = 5f;
        [SerializeField, Range(0f, 80f)] private float _horizontalTearing = 28f;
        [SerializeField, Range(0f, 1f)] private float _staticOpacity = 0.16f;
        [SerializeField, Range(0f, 1f)] private float _vignetteShift = 0.18f;
        [SerializeField, Range(0f, 1f)] private float _flashOpacity = 0.12f;
        [SerializeField, Range(0f, 1f)] private float _overlayOpacity = 0.22f;

        public float Duration => _duration;
        public float Intensity => _intensity;
        public float ChromaticOffset => _chromaticOffset;
        public float HorizontalTearing => _horizontalTearing;
        public float StaticOpacity => _staticOpacity;
        public float VignetteShift => _vignetteShift;
        public float FlashOpacity => _flashOpacity;
        public float OverlayOpacity => _overlayOpacity;
    }
}
