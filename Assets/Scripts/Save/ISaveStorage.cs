namespace NullPointer.Save
{
    public interface ISaveStorage
    {
        bool Exists { get; }

        string Read();

        void WriteAtomic(string contents);

        void Delete();
    }
}
