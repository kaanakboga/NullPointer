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

        [Test]
        public void GameplayActions_ProvideKeyboardAndGamepadBindings()
        {
            InputActionAsset asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
            InputActionMap gameplay = asset.FindActionMap(GameplayInputActionNames.Map, true);

            AssertBinding(gameplay.FindAction(GameplayInputActionNames.Move, true), "<Gamepad>/leftStick");
            AssertBindingContains(gameplay.FindAction(GameplayInputActionNames.Move, true), "<Keyboard>/w");
            AssertBindingContains(gameplay.FindAction(GameplayInputActionNames.Move, true), "<Keyboard>/upArrow");
            AssertBindingContains(gameplay.FindAction(GameplayInputActionNames.Interact, true), "<Keyboard>");
            AssertBindingContains(gameplay.FindAction(GameplayInputActionNames.Interact, true), "<Gamepad>");
            AssertBindingContains(gameplay.FindAction(GameplayInputActionNames.Pause, true), "<Keyboard>");
            AssertBindingContains(gameplay.FindAction(GameplayInputActionNames.Pause, true), "<Gamepad>");
        }

        private static void AssertBinding(InputAction action, string path)
        {
            Assert.That(action.bindings.Any(binding => binding.path == path), Is.True, $"Missing binding {path}.");
        }

        private static void AssertBindingContains(InputAction action, string pathFragment)
        {
            Assert.That(
                action.bindings.Any(binding => binding.path != null && binding.path.Contains(pathFragment)),
                Is.True,
                $"Missing binding containing {pathFragment}.");
        }
    }
}
