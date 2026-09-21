using NullPointer.Menus;
using UnityEngine;

namespace NullPointer.Runtime
{
    [AddComponentMenu("Null Pointer/Runtime/Main Menu Scene Installer")]
    public sealed class MainMenuSceneInstaller : MonoBehaviour
    {
        [SerializeField] private MainMenuController _controller;

        public void Configure(MainMenuController controller)
        {
            _controller = controller;
        }

        public void Install(GameApplication application)
        {
            application.PrepareMainMenu();
            _controller.Initialize(application, application.SettingsManager);
        }
    }
}
