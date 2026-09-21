using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NullPointer.Menus
{
    [AddComponentMenu("Null Pointer/Menus/Pause Menu Panel")]
    public sealed class PauseMenuPanel : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _journalButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _mainMenuButton;

        public void Configure(Button resume, Button journal, Button settings, Button mainMenu)
        {
            _resumeButton = resume;
            _journalButton = journal;
            _settingsButton = settings;
            _mainMenuButton = mainMenu;
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
            EventSystem.current?.SetSelectedGameObject(_resumeButton.gameObject);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
