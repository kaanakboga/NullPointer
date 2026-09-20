using System.Linq;
using NUnit.Framework;
using NullPointer.Input;
using UnityEditor;
using UnityEngine.InputSystem;

namespace NullPointer.Tests.EditMode
{
    public sealed class InputActionsTests
    {
        private const string InputAssetPath = "Assets/Settings/InputSystem_Actions.inputactions";

        [Test]
        public void InputAsset_ContainsOnlyRequiredGameplayActionsAndRequiredUiActions()
        {
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
            Assert.That(asset, Is.Not.Null);

            InputActionMap gameplay = asset.FindActionMap(GameplayInputActionNames.Map, true);
            string[] gameplayActions = gameplay.actions.Select(action => action.name).OrderBy(name => name).ToArray();
            Assert.That(
                gameplayActions,
                Is.EqualTo(new[]
                {
                    GameplayInputActionNames.Interact,
                    GameplayInputActionNames.Move,
                    GameplayInputActionNames.Pause
                }));

            InputActionMap ui = asset.FindActionMap("UI", true);
            Assert.That(ui.FindAction("Navigate", false), Is.Not.Null);
            Assert.That(ui.FindAction("Submit", false), Is.Not.Null);
            Assert.That(ui.FindAction("Cancel", false), Is.Not.Null);
            Assert.That(ui.FindAction("Point", false), Is.Not.Null);
            Assert.That(ui.FindAction("Click", false), Is.Not.Null);
            Assert.That(ui.FindAction("ScrollWheel", false), Is.Not.Null);
        }
    }
}
