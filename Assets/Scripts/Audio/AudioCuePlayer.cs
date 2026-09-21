using UnityEngine;

namespace NullPointer.Audio
{
    [AddComponentMenu("Null Pointer/Audio/Audio Cue Player")]
    public sealed class AudioCuePlayer : MonoBehaviour
    {
        [SerializeField] private AudioCueSet _cues;
        [SerializeField] private AudioSource _ambienceSource;
        [SerializeField] private AudioSource _sfxSource;

        public void Configure(AudioCueSet cues, AudioSource ambienceSource, AudioSource sfxSource)
        {
            _cues = cues;
            _ambienceSource = ambienceSource;
            _sfxSource = sfxSource;
        }

        public void PlayRainAmbience()
        {
            PlayLoop(_cues == null ? null : _cues.RainAmbience);
        }

        public void PlayElectricalHum()
        {
            PlayLoop(_cues == null ? null : _cues.ElectricalHum);
        }

        public void PlayDistantTraffic()
        {
            PlayLoop(_cues == null ? null : _cues.DistantTraffic);
        }

        public void PlayTerminalBeep() => PlayOneShot(_cues == null ? null : _cues.TerminalBeep);

        public void PlayUiInteraction() => PlayOneShot(_cues == null ? null : _cues.UiInteraction);

        public void PlayEvidenceAcquired() => PlayOneShot(_cues == null ? null : _cues.EvidenceAcquired);

        public void PlayMemoryGlitch() => PlayOneShot(_cues == null ? null : _cues.MemoryGlitch);

        public void PlayChapterEndSting() => PlayOneShot(_cues == null ? null : _cues.ChapterEndSting);

        public void SetVolumes(float ambienceVolume, float sfxVolume)
        {
            if (_ambienceSource != null)
            {
                _ambienceSource.volume = Mathf.Clamp01(ambienceVolume);
            }

            if (_sfxSource != null)
            {
                _sfxSource.volume = Mathf.Clamp01(sfxVolume);
            }
        }

        private void PlayLoop(AudioClip clip)
        {
            if (_ambienceSource == null || clip == null)
            {
                return;
            }

            _ambienceSource.clip = clip;
            _ambienceSource.loop = true;
            _ambienceSource.Play();
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (_sfxSource != null && clip != null)
            {
                _sfxSource.PlayOneShot(clip);
            }
        }
    }
}
