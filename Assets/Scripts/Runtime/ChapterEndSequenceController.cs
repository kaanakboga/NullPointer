using System;
using NullPointer.Core;
using NullPointer.Deduction;
using NullPointer.Dialogue;
using UnityEngine;

namespace NullPointer.Runtime
{
    [AddComponentMenu("Null Pointer/Runtime/Chapter End Sequence Controller")]
    public sealed class ChapterEndSequenceController : MonoBehaviour
    {
        [SerializeField] private string _triggerDeductionId = string.Empty;
        [SerializeField] private DialogueData _dialogue;
        [SerializeField] private DialogueController _dialogueController;
        private GameApplication _application;
        private bool _pending;

        public void Configure(
            string triggerDeductionId,
            DialogueData dialogue,
            DialogueController dialogueController)
        {
            _triggerDeductionId = triggerDeductionId;
            _dialogue = dialogue;
            _dialogueController = dialogueController;
        }

        public void Initialize(GameApplication application)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _application.DeductionService.DeductionCompleted += OnDeductionCompleted;
            _application.GameModes.ModeChanged += OnModeChanged;
        }

        private void OnDeductionCompleted(DeductionCompleted change)
        {
            _pending = string.Equals(change.DeductionId, _triggerDeductionId, StringComparison.Ordinal);
        }

        private void OnModeChanged(GameModeChanged change)
        {
            if (!_pending || change.CurrentMode != GameMode.Gameplay)
            {
                return;
            }

            _pending = false;
            _dialogueController.Open(_dialogue);
        }

        private void OnDestroy()
        {
            if (_application != null)
            {
                _application.DeductionService.DeductionCompleted -= OnDeductionCompleted;
                _application.GameModes.ModeChanged -= OnModeChanged;
            }
        }
    }
}
