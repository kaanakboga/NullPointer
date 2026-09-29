using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using NullPointer.Visual;
using System.Collections;

namespace NullPointer.Menus
{
    [AddComponentMenu("Null Pointer/Menus/Pause Menu Panel")]
    public sealed class PauseMenuPanel : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _journalButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private UiTransition _transition;
        [SerializeField] private ScreenOverlayLayer _screenOverlay;
        [SerializeField] private PausePresentationFx _presentationFx;
        [SerializeField] private UiTransition[] _rowTransitions;
        private bool _hasBeenShown;
        private Coroutine _rowReveal;

        public void Configure(Button resume, Button journal, Button settings, Button mainMenu)
        {
            _resumeButton = resume;
            _journalButton = journal;
            _settingsButton = settings;
            _mainMenuButton = mainMenu;
        }

        public void ConfigurePresentation(UiTransition transition, ScreenOverlayLayer screenOverlay = null,
            PausePresentationFx presentationFx = null, UiTransition[] rowTransitions = null)
        {
            _transition = transition;
            _screenOverlay = screenOverlay;
            _presentationFx = presentationFx;
            _rowTransitions = rowTransitions;
        }

        public void SetAccessibility(bool reducedMotion, bool reducedFx)
        {
            _transition?.SetReducedMotion(reducedMotion);
            _screenOverlay?.SetIntensity(reducedFx ? 0.25f : 1f);
            _presentationFx?.SetReducedFx(reducedFx);
            if (_rowTransitions != null)
            {
                foreach (UiTransition row in _rowTransitions)
                {
                    row?.SetReducedMotion(reducedMotion);
                }
            }
        }

        public void Bind(PauseMenuController controller)
        {
            _resumeButton.onClick.AddListener(controller.Resume);
            _journalButton.onClick.AddListener(controller.OpenJournal);
            _settingsButton.onClick.AddListener(controller.OpenSettings);
            _mainMenuButton.onClick.AddListener(controller.ReturnToMainMenu);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            _hasBeenShown = true;
            _transition?.PlayReveal();
            if (_rowReveal != null)
            {
                StopCoroutine(_rowReveal);
            }

            _rowReveal = StartCoroutine(RevealRows());
            EventSystem.current?.SetSelectedGameObject(_resumeButton.gameObject);
        }

        public void Hide()
        {
            if (_hasBeenShown && gameObject.activeSelf && _transition != null)
            {
                _hasBeenShown = false;
                _transition.PlayExit();
                return;
            }

            _hasBeenShown = false;
            if (_rowReveal != null)
            {
                StopCoroutine(_rowReveal);
                _rowReveal = null;
            }
            gameObject.SetActive(false);
        }

        private IEnumerator RevealRows()
        {
            if (_rowTransitions == null)
            {
                yield break;
            }

            foreach (UiTransition row in _rowTransitions)
            {
                row?.SnapHidden(false);
            }

            foreach (UiTransition row in _rowTransitions)
            {
                row?.PlayReveal();
                float elapsed = 0f;
                while (elapsed < 0.055f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }

            _rowReveal = null;
        }
    }
}
