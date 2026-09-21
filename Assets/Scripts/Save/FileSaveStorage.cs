using System;
using System.IO;

namespace NullPointer.Save
{
    public sealed class FileSaveStorage : ISaveStorage
    {
        private readonly string _path;
        private readonly string _backupPath;
        private readonly string _temporaryPath;

        public FileSaveStorage(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A save path is required.", nameof(path));
            }

            _path = Path.GetFullPath(path);
            _backupPath = _path + ".bak";
            _temporaryPath = _path + ".tmp";
        }

        public bool Exists => File.Exists(_path);

        public string Read()
        {
            return File.ReadAllText(_path);
        }

        public void WriteAtomic(string contents)
        {
            string directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_temporaryPath, contents ?? string.Empty);
            try
            {
                if (File.Exists(_path))
                {
                    File.Replace(_temporaryPath, _path, _backupPath, true);
                }
                else
                {
                    File.Move(_temporaryPath, _path);
                }
            }
            finally
            {
                if (File.Exists(_temporaryPath))
                {
                    File.Delete(_temporaryPath);
                }
            }
        }

        public void Delete()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }

            if (File.Exists(_backupPath))
            {
                File.Delete(_backupPath);
            }

            if (File.Exists(_temporaryPath))
            {
                File.Delete(_temporaryPath);
            }
        }
    }
}
