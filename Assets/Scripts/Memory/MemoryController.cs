using System;
using System.Collections;
using NullPointer.Core;
using NullPointer.Input;
using NullPointer.Visual;
using UnityEngine;

namespace NullPointer.Memory
{
    [AddComponentMenu("Null Pointer/Memory/Memory Controller")]
    public sealed class MemoryController : MonoBehaviour
    {
        [SerializeField] private MemoryPanel _panel;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private MemoryDistortionController _distortionController;
        [SerializeField] private MemoryDistortionProfile _distortionProfile;

        private GameModeController _gameModes;
        private IGameplayInputSource _input;
        private MemoryService _memoryService;
        private Coroutine _sequence;
        private bool _isInputSubscribed;

        public bool IsPlaying => _gameModes != null && _gameModes.CurrentMode == GameMode.Memory;

        public void Configure(MemoryPanel panel, AudioSource audioSource)
        {
            _panel = panel;
            _audioSource = audioSource;
        }

        public void ConfigureVisualEffects(
            MemoryDistortionController distortionController,
            MemoryDistortionProfile distortionProfile)
        {
            _distortionController = distortionController;
            _distortionProfile = distortionProfile;
        }

        public void Initialize(
            GameModeController gameModes,
            IGameplayInputSource input,
            MemoryService memoryService)
        {
            StopSequence(false);
            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _memoryService = memoryService ?? throw new ArgumentNullException(nameof(memoryService));
        }

        public bool Play(MemoryData memory)
        {
            if (memory == null || _gameModes == null || IsPlaying)
            {
                return false;
            }

            _memoryService.Unlock(memory);
            _gameModes.SetMode(GameMode.Memory);
            _panel.Show(memory);
            if (_distortionController != null && _distortionProfile != null)
            {
                float duration = 0f;
                foreach (MemoryBeat beat in memory.Beats)
                {
                    if (beat != null)
                    {
                        duration += Mathf.Max(0.05f, beat.Duration);
                    }
                }

                _distortionController.Play(_distortionProfile, duration);
            }

            Subscribe();
            _sequence = StartCoroutine(PlaySequence(memory));
            return true;
        }

        public void Skip()
        {
            StopSequence(true);
        }

        private IEnumerator PlaySequence(MemoryData memory)
        {
            foreach (MemoryBeat beat in memory.Beats)
            {
                if (beat == null)
                {
                    continue;
                }

                _panel.ShowBeat(beat);
                if (_audioSource != null && beat.AudioCue != null)
                {
                    _audioSource.PlayOneShot(beat.AudioCue);
                }

                yield return new WaitForSecondsRealtime(Mathf.Max(0.05f, beat.Duration));
            }

            StopSequence(true);
        }

        private void StopSequence(bool returnToGameplay)
        {
            if (_sequence != null)
            {
                StopCoroutine(_sequence);
                _sequence = null;
            }

            Unsubscribe();
            if (_audioSource != null)
            {
                _audioSource.Stop();
            }

            _distortionController?.StopImmediate();

            if (_panel != null)
            {
                _panel.Hide();
            }

            if (returnToGameplay && IsPlaying)
            {
                _gameModes.SetMode(GameMode.Gameplay);
            }
        }

        private void Subscribe()
        {
            if (_isInputSubscribed)
            {
                return;
            }

            _input.PausePressed += Skip;
            _isInputSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isInputSubscribed || _input == null)
            {
                return;
            }

            _input.PausePressed -= Skip;
            _isInputSubscribed = false;
        }

        private void OnDisable()
        {
            StopSequence(true);
        }
    }
}
