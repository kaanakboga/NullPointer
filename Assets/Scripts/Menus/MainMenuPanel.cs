using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Menus
{
    [AddComponentMenu("Null Pointer/Menus/Main Menu Panel")]
    public sealed class MainMenuPanel : MonoBehaviour
    {
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        private GameObject _lastSelection;

        public void Configure(Button newGame, Button continueGame, Button settings, Button quit)
        {
            _newGameButton = newGame;
            _continueButton = continueGame;
            _settingsButton = settings;
            _quitButton = quit;
        }

        public void Bind(MainMenuController controller)
        {
            _newGameButton.onClick.RemoveAllListeners();
            _continueButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _quitButton.onClick.RemoveAllListeners();
            _newGameButton.onClick.AddListener(controller.StartNewGame);
            _continueButton.onClick.AddListener(controller.ContinueGame);
            _settingsButton.onClick.AddListener(controller.OpenSettings);
            _quitButton.onClick.AddListener(controller.Quit);
        }

        public void Show(bool canContinue)
        {
            gameObject.SetActive(true);
            _continueButton.interactable = canContinue;
            GameObject fallback = canContinue ? _continueButton.gameObject : _newGameButton.gameObject;
            GameObject selection = _lastSelection != null &&
                                   _lastSelection.activeInHierarchy &&
                                   _lastSelection.GetComponent<Selectable>()?.IsInteractable() == true
                ? _lastSelection
                : fallback;
            EventSystem.current?.SetSelectedGameObject(selection);
        }

        public void Hide()
        {
            GameObject selected = EventSystem.current?.currentSelectedGameObject;
            if (selected != null && selected.transform.IsChildOf(transform))
            {
                _lastSelection = selected;
            }

            gameObject.SetActive(false);
        }
    }
}
