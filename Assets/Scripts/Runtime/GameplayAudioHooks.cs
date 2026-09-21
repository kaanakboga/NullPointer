using System;
using NullPointer.Audio;
using NullPointer.Deduction;
using NullPointer.Evidence;
using NullPointer.Memory;
using NullPointer.Terminal;
using NullPointer.Settings;
using UnityEngine;

namespace NullPointer.Runtime
{
    public enum LocationAmbienceKind
    {
        None = 0,
        Rain = 1,
        ElectricalHum = 2,
        DistantTraffic = 3
    }

    [AddComponentMenu("Null Pointer/Runtime/Gameplay Audio Hooks")]
    public sealed class GameplayAudioHooks : MonoBehaviour
    {
        [SerializeField] private AudioCuePlayer _player;
        [SerializeField] private TerminalController _terminal;
        [SerializeField] private LocationAmbienceKind _ambience;
        [SerializeField] private string _chapterEndDeductionId = string.Empty;
        private GameApplication _application;

        public void Configure(
            AudioCuePlayer player,
            TerminalController terminal,
            LocationAmbienceKind ambience,
            string chapterEndDeductionId = "")
        {
            _player = player;
            _terminal = terminal;
            _ambience = ambience;
            _chapterEndDeductionId = chapterEndDeductionId ?? string.Empty;
        }

        public void Initialize(GameApplication application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _application.EvidenceService.EvidenceCollected += OnEvidenceCollected;
            _application.MemoryService.MemoryUnlocked += OnMemoryUnlocked;
            _application.DeductionService.DeductionCompleted += OnDeductionCompleted;
            _application.SettingsManager.Changed += OnSettingsChanged;
            if (_terminal != null)
            {
                _terminal.EntryOpened += OnTerminalEntryOpened;
            }

            OnSettingsChanged(_application.SettingsManager.Current);

            switch (_ambience)
            {
                case LocationAmbienceKind.Rain:
                    _player.PlayRainAmbience();
                    break;
                case LocationAmbienceKind.ElectricalHum:
                    _player.PlayElectricalHum();
                    break;
                case LocationAmbienceKind.DistantTraffic:
                    _player.PlayDistantTraffic();
                    break;
            }
        }

        private void OnEvidenceCollected(EvidenceCollected change) => _player.PlayEvidenceAcquired();

        private void OnMemoryUnlocked(MemoryUnlocked change) => _player.PlayMemoryGlitch();

        private void OnTerminalEntryOpened(TerminalEntryOpened change) => _player.PlayTerminalBeep();

        private void OnDeductionCompleted(DeductionCompleted change)
        {
            if (string.Equals(change.DeductionId, _chapterEndDeductionId, StringComparison.Ordinal))
            {
                _player.PlayChapterEndSting();
            }
            else
            {
                _player.PlayUiInteraction();
            }
        }

        private void OnSettingsChanged(GameSettings settings)
        {
            _player.SetVolumes(settings.MusicVolume, settings.SfxVolume);
        }

        private void OnDestroy()
        {
            if (_application != null)
            {
                _application.EvidenceService.EvidenceCollected -= OnEvidenceCollected;
                _application.MemoryService.MemoryUnlocked -= OnMemoryUnlocked;
                _application.DeductionService.DeductionCompleted -= OnDeductionCompleted;
                _application.SettingsManager.Changed -= OnSettingsChanged;
            }

            if (_terminal != null)
            {
                _terminal.EntryOpened -= OnTerminalEntryOpened;
            }
        }
    }
}
