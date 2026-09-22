using System;
using NullPointer.Core;
using NullPointer.Evidence;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Inspect
{
    [AddComponentMenu("Null Pointer/Inspect/Inspect Controller")]
    public sealed class InspectController : MonoBehaviour
    {
        [SerializeField] private InspectPanel _panel;

        private GameModeController _gameModes;
        private IGameplayInputSource _input;
        private EvidenceService _evidenceService;
        private EvidenceData _currentEvidence;
        private Action<EvidenceData> _onEvidenceCollected;
        private bool _isInputSubscribed;

        public bool IsOpen => _gameModes != null && _gameModes.CurrentMode == GameMode.Inspect;

        public void Configure(InspectPanel panel)
        {
            _panel = panel;
        }

        public void Initialize(
            GameModeController gameModes,
            IGameplayInputSource input,
            EvidenceService evidenceService)
        {
            Unsubscribe();
            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _panel.Bind(this);
        }

        public void Open(InspectData inspection)
        {
            OpenInternal(inspection, null, null);
        }

        public void OpenEvidence(
            InspectData inspection,
            EvidenceData evidence,
            Action<EvidenceData> onEvidenceCollected = null)
        {
            OpenInternal(inspection, evidence, onEvidenceCollected);
        }

        public void CollectCurrentEvidence()
        {
            if (!IsOpen || _currentEvidence == null)
            {
                return;
            }

            EvidenceCollectionStatus result = _evidenceService.Collect(_currentEvidence);
            bool isCollected = _evidenceService.IsCollected(_currentEvidence.StableId);
            _panel.SetCollectionState(true, isCollected);

            if (result == EvidenceCollectionStatus.Collected)
            {
                EvidenceData collectedEvidence = _currentEvidence;
                Action<EvidenceData> followUp = _onEvidenceCollected;
                if (followUp != null)
                {
                    Close();
                    followUp.Invoke(collectedEvidence);
                }
            }
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            Unsubscribe();
            _currentEvidence = null;
            _onEvidenceCollected = null;
            _panel.Hide();
            _gameModes.SetMode(GameMode.Gameplay);
        }

        private void OpenInternal(
            InspectData inspection,
            EvidenceData evidence,
            Action<EvidenceData> onEvidenceCollected)
        {
            if (inspection == null || _gameModes == null || _panel == null)
            {
                return;
            }

            _currentEvidence = evidence;
            _onEvidenceCollected = onEvidenceCollected;
            _gameModes.SetMode(GameMode.Inspect);
            _panel.Show(
                inspection,
                evidence != null,
                evidence != null && _evidenceService.IsCollected(evidence.StableId));
            Subscribe();
        }

        private void Subscribe()
        {
            if (_isInputSubscribed || _input == null)
            {
                return;
            }

            _input.PausePressed += Close;
            _isInputSubscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_isInputSubscribed || _input == null)
            {
                return;
            }

            _input.PausePressed -= Close;
            _isInputSubscribed = false;
        }

        private void OnDisable()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Unsubscribe();
            }
        }
    }
}
