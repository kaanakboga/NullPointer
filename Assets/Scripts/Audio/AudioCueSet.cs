using UnityEngine;

namespace NullPointer.Audio
{
    [CreateAssetMenu(menuName = "Null Pointer/Audio/Cue Set", fileName = "AUD_CueSet")]
    public sealed class AudioCueSet : ScriptableObject
    {
        [SerializeField] private AudioClip _rainAmbience;
        [SerializeField] private AudioClip _electricalHum;
        [SerializeField] private AudioClip _distantTraffic;
        [SerializeField] private AudioClip _terminalBeep;
        [SerializeField] private AudioClip _uiInteraction;
        [SerializeField] private AudioClip _evidenceAcquired;
        [SerializeField] private AudioClip _memoryGlitch;
        [SerializeField] private AudioClip _chapterEndSting;

        public AudioClip RainAmbience => _rainAmbience;
        public AudioClip ElectricalHum => _electricalHum;
        public AudioClip DistantTraffic => _distantTraffic;
        public AudioClip TerminalBeep => _terminalBeep;
        public AudioClip UiInteraction => _uiInteraction;
        public AudioClip EvidenceAcquired => _evidenceAcquired;
        public AudioClip MemoryGlitch => _memoryGlitch;
        public AudioClip ChapterEndSting => _chapterEndSting;
    }
}
