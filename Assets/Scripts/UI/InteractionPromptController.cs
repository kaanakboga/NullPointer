using NullPointer.Interaction;
using UnityEngine;
using UnityEngine.UI;

namespace NullPointer.UI
{
    [AddComponentMenu("Null Pointer/UI/Interaction Prompt Controller")]
    public sealed class InteractionPromptController : MonoBehaviour
    {
        [SerializeField] private GameObject _promptRoot;
        [SerializeField] private Text _promptLabel;

        private PlayerInteractionDetector _detector;

        public void Configure(GameObject promptRoot, Text promptLabel)
        {
            _promptRoot = promptRoot;
            _promptLabel = promptLabel;
        }

        public void Initialize(PlayerInteractionDetector detector)
        {
            Unsubscribe();
            _detector = detector;
            if (_detector != null)
            {
                _detector.ActiveTargetChanged += OnActiveTargetChanged;
                OnActiveTargetChanged(_detector.ActiveTarget);
            }
        }

        private void OnActiveTargetChanged(IInteractable target)
        {
            bool isVisible = target != null && target.CanInteract;
            _promptRoot.SetActive(isVisible);
            if (!isVisible)
            {
                return;
            }

            string prompt = target is IInteractionPromptSource source
                ? source.InteractionPrompt
                : "Etkileşim";
            _promptLabel.text = $"[E / A] {prompt}";
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_detector != null)
            {
                _detector.ActiveTargetChanged -= OnActiveTargetChanged;
            }
        }
    }
}
