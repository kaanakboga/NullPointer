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
            EventSystem.current?.SetSelectedGameObject(canContinue
                ? _continueButton.gameObject
                : _newGameButton.gameObject);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
