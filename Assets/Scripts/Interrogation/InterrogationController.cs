using System;
using System.Collections.Generic;
using NullPointer.Core;
using NullPointer.Evidence;
using NullPointer.Input;
using UnityEngine;

namespace NullPointer.Interrogation
{
    [AddComponentMenu("Null Pointer/Interrogation/Interrogation Controller")]
    public sealed class InterrogationController : MonoBehaviour
    {
        [SerializeField] private InterrogationPanel _panel;
        private GameModeController _gameModes;
        private IGameplayInputSource _input;
        private EvidenceService _evidenceService;
        private InterrogationService _service;
        private InterrogationClaimData _claim;
        private IReadOnlyList<EvidenceData> _evidence = Array.Empty<EvidenceData>();

        public bool IsOpen => _gameModes != null && _gameModes.CurrentMode == GameMode.Interrogation;

        public void Configure(InterrogationPanel panel)
        {
            _panel = panel;
        }

        public void Initialize(
            GameModeController gameModes,
            IGameplayInputSource input,
            EvidenceService evidenceService,
            InterrogationService service)
        {
            _gameModes = gameModes ?? throw new ArgumentNullException(nameof(gameModes));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _evidenceService = evidenceService ?? throw new ArgumentNullException(nameof(evidenceService));
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _panel.Bind(this);
        }

        public bool Open(InterrogationClaimData claim)
        {
            if (claim == null || _service == null)
            {
                return false;
            }

            _claim = claim;
            _evidence = _evidenceService.GetCollectedEvidence();
            _gameModes.SetMode(GameMode.Interrogation);
            _panel.Show(claim, _evidence);
            _input.PausePressed += Close;
            return true;
        }

        public void PresentEvidence(int index)
        {
            if (!IsOpen || index < 0 || index >= _evidence.Count)
            {
                return;
            }

            InterrogationAttemptResult result = _service.PresentEvidence(
                _claim.StableId,
                _evidence[index].StableId);
            _panel.SetFeedback(result.Response);
        }

        public void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            _input.PausePressed -= Close;
            _claim = null;
            _evidence = Array.Empty<EvidenceData>();
            _panel.Hide();
            _gameModes.SetMode(GameMode.Gameplay);
        }

        private void OnDisable()
        {
            if (_input != null)
            {
                _input.PausePressed -= Close;
            }
        }
    }
}
