namespace NullPointer.Save
{
    public enum SaveLoadStatus
    {
        Success = 0,
        MissingFile = 1,
        Corrupted = 2,
        UnsupportedSchema = 3,
        StorageFailure = 4
    }
}
