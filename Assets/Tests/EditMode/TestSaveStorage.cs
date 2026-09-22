using NullPointer.Save;

namespace NullPointer.Tests.EditMode
{
    internal sealed class TestSaveStorage : ISaveStorage
    {
        public string Contents
        {
            get => PrimaryContents;
            set => PrimaryContents = value;
        }

        public string PrimaryContents { get; set; }

        public string BackupContents { get; set; }

        public bool FailNextWrite { get; set; }

        public bool PrimaryExists => PrimaryContents != null;

        public bool BackupExists => BackupContents != null;

        public string ReadPrimary()
        {
            return PrimaryContents;
        }

        public string ReadBackup()
        {
            return BackupContents;
        }

        public void WriteAtomic(string contents, SaveBackupBehavior backupBehavior)
        {
            if (FailNextWrite)
            {
                FailNextWrite = false;
                throw new System.IO.IOException("Simulated interrupted write.");
            }

            if (backupBehavior == SaveBackupBehavior.RotatePrimaryToBackup && PrimaryExists)
            {
                BackupContents = PrimaryContents;
            }

            PrimaryContents = contents;
        }

        public void RestoreBackupToPrimary()
        {
            if (!BackupExists)
            {
                throw new System.IO.FileNotFoundException();
            }

            PrimaryContents = BackupContents;
        }

        public void Delete()
        {
            PrimaryContents = null;
            BackupContents = null;
        }
    }
}
