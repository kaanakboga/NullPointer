#if UNITY_EDITOR
using UnityEngine;

namespace NullPointer.Core.Engineering
{
    [AddComponentMenu("Null Pointer/Engineering/Game Mode Display")]
    public sealed class EngineeringModeDisplay : MonoBehaviour
    {
        [SerializeField] private GameModeController _gameModeController;
        [SerializeField] private TextMesh _label;

        public void Configure(GameModeController gameModeController, TextMesh label)
        {
            Unsubscribe();
            _gameModeController = gameModeController;
            _label = label;
            Subscribe();
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_gameModeController != null)
            {
                _gameModeController.ModeChanged -= OnModeChanged;
                _gameModeController.ModeChanged += OnModeChanged;
            }
        }

        private void Unsubscribe()
        {
            if (_gameModeController != null)
            {
                _gameModeController.ModeChanged -= OnModeChanged;
            }
        }

        private void OnModeChanged(GameModeChanged change)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (_label != null && _gameModeController != null)
            {
                _label.text = $"Mode: {_gameModeController.CurrentMode}";
            }
        }
    }
}
#endif
