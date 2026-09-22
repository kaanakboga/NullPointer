using System;
using System.IO;
using NUnit.Framework;
using NullPointer.Save;

namespace NullPointer.Tests.EditMode
{
    public sealed class FileSaveStorageTests
    {
        private string _directory;
        private string _path;

        [SetUp]
        public void SetUp()
        {
            _directory = Path.Combine(Path.GetTempPath(), "NullPointerSaveTests", Guid.NewGuid().ToString("N"));
            _path = Path.Combine(_directory, "save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_directory))
            {
                Directory.Delete(_directory, true);
            }
        }

        [Test]
        public void WriteAtomic_RotatesPreviousPrimaryAndCanRestoreBackup()
        {
            var storage = new FileSaveStorage(_path);
            storage.WriteAtomic("first", SaveBackupBehavior.PreserveExistingBackup);
            storage.WriteAtomic("second", SaveBackupBehavior.RotatePrimaryToBackup);

            Assert.That(storage.ReadPrimary(), Is.EqualTo("second"));
            Assert.That(storage.ReadBackup(), Is.EqualTo("first"));

            storage.RestoreBackupToPrimary();

            Assert.That(storage.ReadPrimary(), Is.EqualTo("first"));
            Assert.That(storage.ReadBackup(), Is.EqualTo("first"));
            Assert.That(File.Exists(_path + ".tmp"), Is.False);
        }

        [Test]
        public void WriteAtomic_PreserveBackup_DoesNotReplaceLastKnownGoodBackup()
        {
            var storage = new FileSaveStorage(_path);
            storage.WriteAtomic("known-good", SaveBackupBehavior.PreserveExistingBackup);
            storage.WriteAtomic("new-primary", SaveBackupBehavior.RotatePrimaryToBackup);
            storage.WriteAtomic("repaired-primary", SaveBackupBehavior.PreserveExistingBackup);

            Assert.That(storage.ReadPrimary(), Is.EqualTo("repaired-primary"));
            Assert.That(storage.ReadBackup(), Is.EqualTo("known-good"));
        }
    }
}
