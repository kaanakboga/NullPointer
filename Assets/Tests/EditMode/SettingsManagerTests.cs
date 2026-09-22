using NUnit.Framework;
using NullPointer.Core;
using NullPointer.Save;
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

        [Test]
        public void StorySaveDeletion_ForNewGame_DoesNotResetGlobalSettings()
        {
            var settingsStorage = new TestSettingsStorage();
            var settings = new SettingsManager(settingsStorage);
            settings.Update(new GameSettings
            {
                MasterVolume = 0.35f,
                MusicVolume = 0.45f,
                SfxVolume = 0.55f,
                Fullscreen = false,
                ResolutionWidth = 2560,
                ResolutionHeight = 1440,
                TextSpeed = 0.015f
            }, false);
            var storyStorage = new TestSaveStorage();
            var saves = new SaveManager(storyStorage, "test");
            saves.Save(new GameState());

            saves.Delete();
            var restored = new SettingsManager(settingsStorage);

            Assert.That(restored.Current.MasterVolume, Is.EqualTo(0.35f));
            Assert.That(restored.Current.MusicVolume, Is.EqualTo(0.45f));
            Assert.That(restored.Current.SfxVolume, Is.EqualTo(0.55f));
            Assert.That(restored.Current.Fullscreen, Is.False);
            Assert.That(restored.Current.ResolutionWidth, Is.EqualTo(2560));
            Assert.That(restored.Current.ResolutionHeight, Is.EqualTo(1440));
            Assert.That(restored.Current.TextSpeed, Is.EqualTo(0.015f));
            Assert.That(storyStorage.PrimaryExists, Is.False);
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
