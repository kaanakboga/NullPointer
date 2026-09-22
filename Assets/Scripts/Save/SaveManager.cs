using System;
using System.IO;
using NullPointer.Core;
using UnityEngine;

namespace NullPointer.Save
{
    public sealed class SaveManager
    {
        private readonly ISaveStorage _storage;
        private readonly string _gameVersion;
        private readonly SaveMigrationPipeline _migrations;
        private readonly Action<SaveDiagnostic> _diagnosticSink;

        public SaveManager(
            ISaveStorage storage,
            string gameVersion,
            Action<SaveDiagnostic> diagnosticSink = null,
            SaveMigrationPipeline migrations = null)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _gameVersion = gameVersion ?? string.Empty;
            _diagnosticSink = diagnosticSink;
            _migrations = migrations ?? new SaveMigrationPipeline();
        }

        public bool HasSave => _storage.PrimaryExists || _storage.BackupExists;

        public bool HasValidSave => Validate().IsSuccess;

        public SaveLoadResult Save(GameState gameState)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            try
            {
                SaveBackupBehavior backupBehavior = DetermineBackupBehavior();
                var data = new SaveGameData
                {
                    SchemaVersion = SaveGameData.CurrentSchemaVersion,
                    GameVersion = _gameVersion,
                    SavedAtUtc = DateTime.UtcNow.ToString("O"),
                    State = gameState.CreateSnapshot()
                };
                data.PayloadHash = SavePayloadIntegrity.Compute(data.State);
                string json = JsonUtility.ToJson(data, true);
                _migrations.UpgradeToCurrent(json);
                _storage.WriteAtomic(json, backupBehavior);
                Report(SaveDiagnosticSeverity.Information, "Primary save write completed.");
                return Result(
                    SaveLoadStatus.Success,
                    gameState,
                    "Save completed.",
                    "Primary save write completed.",
                    SaveLoadSource.Primary,
                    false);
            }
            catch (NotSupportedException exception)
            {
                Report(SaveDiagnosticSeverity.Warning, "Save was blocked to preserve an unsupported primary file.", exception);
                return Result(
                    SaveLoadStatus.UnsupportedSchema,
                    null,
                    "This save was created by a newer game version.",
                    exception.Message,
                    SaveLoadSource.Primary,
                    false);
            }
            catch (Exception exception)
            {
                Report(SaveDiagnosticSeverity.Error, "Save write failed; existing primary and backup were preserved.", exception);
                return Result(
                    SaveLoadStatus.StorageFailure,
                    null,
                    "The game could not create a save.",
                    exception.Message,
                    SaveLoadSource.None,
                    false);
            }
        }

        public SaveLoadResult Load()
        {
            SaveLoadResult primary = _storage.PrimaryExists
                ? LoadCandidate(ReadPrimarySafely, SaveLoadSource.Primary)
                : Result(
                    SaveLoadStatus.MissingFile,
                    null,
                    "No local save exists.",
                    "Primary save is missing.",
                    SaveLoadSource.Primary,
                    false);

            if (primary.IsSuccess)
            {
                return primary;
            }

            if (primary.Status == SaveLoadStatus.UnsupportedSchema)
            {
                return primary;
            }

            SaveLoadResult backup = _storage.BackupExists
                ? LoadCandidate(ReadBackupSafely, SaveLoadSource.Backup)
                : Result(
                    SaveLoadStatus.MissingFile,
                    null,
                    "No recovery save exists.",
                    "Backup save is missing.",
                    SaveLoadSource.Backup,
                    false);

            if (backup.IsSuccess)
            {
                return RecoverValidatedBackup(backup, primary);
            }

            if (primary.Status == SaveLoadStatus.MissingFile && backup.Status == SaveLoadStatus.MissingFile)
            {
                return Result(
                    SaveLoadStatus.MissingFile,
                    null,
                    "No local save exists.",
                    "Neither primary nor backup save exists.",
                    SaveLoadSource.None,
                    false);
            }

            SaveLoadStatus failureStatus = backup.Status == SaveLoadStatus.UnsupportedSchema
                ? SaveLoadStatus.UnsupportedSchema
                : primary.Status == SaveLoadStatus.StorageFailure || backup.Status == SaveLoadStatus.StorageFailure
                    ? SaveLoadStatus.StorageFailure
                    : SaveLoadStatus.Corrupted;
            string diagnostic = $"Primary: {primary.DiagnosticMessage} Backup: {backup.DiagnosticMessage}";
            Report(SaveDiagnosticSeverity.Error, "No valid primary or backup save could be loaded.");
            return Result(
                failureStatus,
                null,
                failureStatus == SaveLoadStatus.UnsupportedSchema
                    ? "This save was created by a newer game version."
                    : "The local save and its recovery copy cannot be loaded.",
                diagnostic,
                SaveLoadSource.None,
                false);
        }

        public SaveLoadResult RestoreBackup()
        {
            if (!_storage.BackupExists)
            {
                return Result(
                    SaveLoadStatus.MissingFile,
                    null,
                    "No recovery save exists.",
                    "Backup save is missing.",
                    SaveLoadSource.Backup,
                    false);
            }

            SaveLoadResult backup = LoadCandidate(ReadBackupSafely, SaveLoadSource.Backup);
            return backup.IsSuccess ? RecoverValidatedBackup(backup, null) : backup;
        }

        public SaveLoadResult Validate()
        {
            return Load();
        }

        public void Delete()
        {
            _storage.Delete();
            Report(SaveDiagnosticSeverity.Information, "Primary and backup save files were deleted.");
        }

        private SaveBackupBehavior DetermineBackupBehavior()
        {
            if (!_storage.PrimaryExists)
            {
                return SaveBackupBehavior.PreserveExistingBackup;
            }

            SaveLoadResult current = LoadCandidate(ReadPrimarySafely, SaveLoadSource.Primary);
            if (current.Status == SaveLoadStatus.UnsupportedSchema)
            {
                throw new NotSupportedException(current.DiagnosticMessage);
            }

            return current.IsSuccess
                ? SaveBackupBehavior.RotatePrimaryToBackup
                : SaveBackupBehavior.PreserveExistingBackup;
        }

        private SaveLoadResult LoadCandidate(Func<string> read, SaveLoadSource source)
        {
            try
            {
                SaveMigrationResult migration = _migrations.UpgradeToCurrent(read());
                GameState state = GameState.FromSnapshot(migration.Data.State);
                string diagnostic = migration.WasMigrated
                    ? $"{source} save migrated from schema 1 to schema {SaveGameData.CurrentSchemaVersion}."
                    : $"{source} save validated.";
                Report(SaveDiagnosticSeverity.Information, diagnostic);
                return Result(
                    SaveLoadStatus.Success,
                    state,
                    "Save loaded.",
                    diagnostic,
                    source,
                    migration.WasMigrated);
            }
            catch (NotSupportedException exception)
            {
                Report(SaveDiagnosticSeverity.Warning, $"{source} save uses an unsupported schema.", exception);
                return Result(
                    SaveLoadStatus.UnsupportedSchema,
                    null,
                    "This save was created by a newer game version.",
                    exception.Message,
                    source,
                    false);
            }
            catch (IOException exception)
            {
                Report(SaveDiagnosticSeverity.Error, $"{source} save could not be read from storage.", exception);
                return Result(
                    SaveLoadStatus.StorageFailure,
                    null,
                    "The save storage is unavailable.",
                    exception.Message,
                    source,
                    false);
            }
            catch (UnauthorizedAccessException exception)
            {
                Report(SaveDiagnosticSeverity.Error, $"{source} save access was denied.", exception);
                return Result(
                    SaveLoadStatus.StorageFailure,
                    null,
                    "The save storage is unavailable.",
                    exception.Message,
                    source,
                    false);
            }
            catch (Exception exception)
            {
                Report(SaveDiagnosticSeverity.Warning, $"{source} save could not be validated.", exception);
                return Result(
                    SaveLoadStatus.Corrupted,
                    null,
                    "The save cannot be loaded.",
                    exception.Message,
                    source,
                    false);
            }
        }

        private SaveLoadResult RecoverValidatedBackup(SaveLoadResult backup, SaveLoadResult primary)
        {
            try
            {
                _storage.RestoreBackupToPrimary();
                string diagnostic = primary == null
                    ? "Validated backup was restored to the primary save."
                    : $"Validated backup replaced an unusable primary save. Primary result: {primary.Status}.";
                Report(SaveDiagnosticSeverity.Warning, diagnostic);
                return Result(
                    SaveLoadStatus.RecoveredFromBackup,
                    backup.GameState,
                    "The recovery save was loaded.",
                    diagnostic,
                    SaveLoadSource.Backup,
                    backup.WasMigrated);
            }
            catch (Exception exception)
            {
                string diagnostic = "Backup state was read successfully, but restoring the primary file failed.";
                Report(SaveDiagnosticSeverity.Warning, diagnostic, exception);
                return Result(
                    SaveLoadStatus.RecoveredFromBackup,
                    backup.GameState,
                    "The recovery save was loaded.",
                    $"{diagnostic} {exception.Message}",
                    SaveLoadSource.Backup,
                    backup.WasMigrated);
            }
        }

        private string ReadPrimarySafely()
        {
            return _storage.ReadPrimary();
        }

        private string ReadBackupSafely()
        {
            return _storage.ReadBackup();
        }

        private static SaveLoadResult Result(
            SaveLoadStatus status,
            GameState gameState,
            string message,
            string diagnostic,
            SaveLoadSource source,
            bool migrated)
        {
            return new SaveLoadResult(status, gameState, message, diagnostic, source, migrated);
        }

        private void Report(SaveDiagnosticSeverity severity, string message, Exception exception = null)
        {
            _diagnosticSink?.Invoke(new SaveDiagnostic(severity, message, exception));
        }
    }
}
