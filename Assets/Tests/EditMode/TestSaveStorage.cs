using NullPointer.Save;

namespace NullPointer.Tests.EditMode
{
    internal sealed class TestSaveStorage : ISaveStorage
    {
        public string Contents { get; set; }

        public bool Exists => Contents != null;

        public string Read()
        {
            return Contents;
        }

        public void WriteAtomic(string contents)
        {
            Contents = contents;
        }

        public void Delete()
        {
            Contents = null;
        }
    }
}
