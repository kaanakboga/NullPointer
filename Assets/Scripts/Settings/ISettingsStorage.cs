namespace NullPointer.Settings
{
    public interface ISettingsStorage
    {
        bool Exists { get; }

        string Read();

        void Write(string contents);
    }
}
