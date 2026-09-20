using NUnit.Framework;
using NullPointer.Core;

namespace NullPointer.Tests.EditMode
{
    public sealed class GameModeServiceTests
    {
        [Test]
        public void SetMode_WhenModeChanges_UpdatesStateAndRaisesOneEvent()
        {
            var service = new GameModeService(GameMode.Gameplay);
            GameModeChanged observedChange = default;
            int eventCount = 0;
            service.ModeChanged += change =>
            {
                observedChange = change;
                eventCount++;
            };

            bool changed = service.SetMode(GameMode.Dialogue);

            Assert.That(changed, Is.True);
            Assert.That(service.CurrentMode, Is.EqualTo(GameMode.Dialogue));
            Assert.That(eventCount, Is.EqualTo(1));
            Assert.That(observedChange.PreviousMode, Is.EqualTo(GameMode.Gameplay));
            Assert.That(observedChange.CurrentMode, Is.EqualTo(GameMode.Dialogue));
        }

        [Test]
        public void SetMode_WhenModeIsUnchanged_DoesNotRaiseEvent()
        {
            var service = new GameModeService(GameMode.Gameplay);
            int eventCount = 0;
            service.ModeChanged += _ => eventCount++;

            bool changed = service.SetMode(GameMode.Gameplay);

            Assert.That(changed, Is.False);
            Assert.That(eventCount, Is.Zero);
        }
    }
}
