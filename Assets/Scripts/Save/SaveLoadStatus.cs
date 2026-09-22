namespace NullPointer.Save
{
    public enum SaveLoadStatus
    {
        Success = 0,
        RecoveredFromBackup = 1,
        MissingFile = 2,
        Corrupted = 3,
        UnsupportedSchema = 4,
        StorageFailure = 5
    }
}
