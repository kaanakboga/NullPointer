using NUnit.Framework;
using NullPointer.Menus;
using NullPointer.Save;

namespace NullPointer.Tests.EditMode
{
    public sealed class MainMenuAvailabilityTests
    {
        [TestCase(false, false)]
        [TestCase(true, true)]
        public void CanContinue_ReflectsValidatedSessionCommandState(bool hasContinue, bool expected)
        {
            Assert.That(
                MainMenuAvailability.CanContinue(new FakeCommands(hasContinue)),
                Is.EqualTo(expected));
        }

        private sealed class FakeCommands : IGameSessionCommands
        {
            public FakeCommands(bool hasContinue)
            {
                HasContinue = hasContinue;
            }

            public bool HasContinue { get; }
            public bool StartNewGame() => true;
            public bool ContinueGame() => HasContinue;
            public SaveLoadResult SaveGame() => null;
            public void ReturnToMainMenu() { }
            public void QuitGame() { }
        }
    }
}
