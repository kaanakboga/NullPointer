namespace NullPointer.Save
{
    public interface ISaveStorage
    {
        bool PrimaryExists { get; }

        bool BackupExists { get; }

        string ReadPrimary();

        string ReadBackup();

        void WriteAtomic(string contents, SaveBackupBehavior backupBehavior);

        void RestoreBackupToPrimary();

        void Delete();
    }
}
