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

        public bool PrimaryExists => File.Exists(_path);

        public bool BackupExists => File.Exists(_backupPath);

        public string PrimaryPath => _path;

        public string BackupPath => _backupPath;

        public string ReadPrimary()
        {
            return File.ReadAllText(_path);
        }

        public string ReadBackup()
        {
            return File.ReadAllText(_backupPath);
        }

        public void WriteAtomic(string contents, SaveBackupBehavior backupBehavior)
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
                    string backupPath = backupBehavior == SaveBackupBehavior.RotatePrimaryToBackup
                        ? _backupPath
                        : null;
                    File.Replace(_temporaryPath, _path, backupPath, true);
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

        public void RestoreBackupToPrimary()
        {
            if (!File.Exists(_backupPath))
            {
                throw new FileNotFoundException("The save backup does not exist.", _backupPath);
            }

            string directory = Path.GetDirectoryName(_path);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.Copy(_backupPath, _temporaryPath, true);
            try
            {
                if (File.Exists(_path))
                {
                    File.Replace(_temporaryPath, _path, null, true);
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
