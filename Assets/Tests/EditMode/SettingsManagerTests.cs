using NUnit.Framework;
using NullPointer.Settings;

namespace NullPointer.Tests.EditMode
{
    public sealed class SettingsManagerTests
    {
        [Test]
        public void Update_PersistsSeparatelyAndClampsSupportedValues()
        {
            var storage = new TestSettingsStorage();
            var manager = new SettingsManager(storage);

            manager.Update(new GameSettings
            {
                MasterVolume = 2f,
                MusicVolume = -1f,
                SfxVolume = 0.4f,
                TextSpeed = 0.5f,
                ResolutionWidth = 320,
                ResolutionHeight = 200,
                Fullscreen = false
            }, false);
            var restored = new SettingsManager(storage);

            Assert.That(restored.Current.MasterVolume, Is.EqualTo(1f));
            Assert.That(restored.Current.MusicVolume, Is.EqualTo(0f));
            Assert.That(restored.Current.SfxVolume, Is.EqualTo(0.4f));
            Assert.That(restored.Current.TextSpeed, Is.EqualTo(0.08f));
            Assert.That(restored.Current.ResolutionWidth, Is.EqualTo(640));
            Assert.That(restored.Current.ResolutionHeight, Is.EqualTo(360));
            Assert.That(restored.Current.Fullscreen, Is.False);
        }

        private sealed class TestSettingsStorage : ISettingsStorage
        {
            public string Contents { get; private set; }
            public bool Exists => Contents != null;
            public string Read() => Contents;
            public void Write(string contents) => Contents = contents;
        }
    }
}
