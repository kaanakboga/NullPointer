using System;
using System.IO;

namespace NullPointer.Settings
{
    public sealed class FileSettingsStorage : ISettingsStorage
    {
        private readonly string _path;
        private readonly string _temporaryPath;

        public FileSettingsStorage(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A settings path is required.", nameof(path));
            }

            _path = Path.GetFullPath(path);
            _temporaryPath = _path + ".tmp";
        }

        public bool Exists => File.Exists(_path);

        public string Read()
        {
            return File.ReadAllText(_path);
        }

        public void Write(string contents)
        {
            string directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_temporaryPath, contents ?? string.Empty);
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }

            File.Move(_temporaryPath, _path);
        }
    }
}
