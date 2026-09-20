using System;
using UnityEngine;

namespace NullPointer.Memory
{
    [Serializable]
    public sealed class MemoryBeat
    {
        [SerializeField, TextArea] private string _text = string.Empty;
        [SerializeField, Min(0.05f)] private float _duration = 1.5f;
        [SerializeField] private Color _overlayColor = new Color(0.05f, 0.8f, 0.9f, 0.35f);
        [SerializeField] private AudioClip _audioCue;

        public string Text => _text;

        public float Duration => _duration;

        public Color OverlayColor => _overlayColor;

        public AudioClip AudioCue => _audioCue;
    }
}
