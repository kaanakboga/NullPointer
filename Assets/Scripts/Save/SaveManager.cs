using System;
using NullPointer.Core;
using UnityEngine;

namespace NullPointer.Save
{
    public sealed class SaveManager
    {
        private readonly ISaveStorage _storage;
        private readonly string _gameVersion;

        public SaveManager(ISaveStorage storage, string gameVersion)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _gameVersion = gameVersion ?? string.Empty;
        }

        public bool HasSave => _storage.Exists;

        public bool HasValidSave => Validate().IsSuccess;

        public SaveLoadResult Save(GameState gameState)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException(nameof(gameState));
            }

            try
            {
                var data = new SaveGameData
                {
                    SchemaVersion = SaveGameData.CurrentSchemaVersion,
                    GameVersion = _gameVersion,
                    SavedAtUtc = DateTime.UtcNow.ToString("O"),
                    State = gameState.CreateSnapshot()
                };
                _storage.WriteAtomic(JsonUtility.ToJson(data, true));
                return new SaveLoadResult(SaveLoadStatus.Success, gameState, "Save completed.");
            }
            catch (Exception exception)
            {
                return new SaveLoadResult(SaveLoadStatus.StorageFailure, null, exception.Message);
            }
        }

        public SaveLoadResult Load()
        {
            if (!_storage.Exists)
            {
                return new SaveLoadResult(SaveLoadStatus.MissingFile, null, "No local save exists.");
            }

            try
            {
                string json = _storage.Read();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return Corrupted("The save file is empty.");
                }

                SaveGameData data = JsonUtility.FromJson<SaveGameData>(json);
                if (data == null || data.State == null)
                {
                    return Corrupted("The save file has no state payload.");
                }

                if (data.SchemaVersion != SaveGameData.CurrentSchemaVersion)
                {
                    return new SaveLoadResult(
                        SaveLoadStatus.UnsupportedSchema,
                        null,
                        $"Save schema {data.SchemaVersion} is unsupported; expected {SaveGameData.CurrentSchemaVersion}.");
                }

                if (data.State.SchemaVersion != GameState.CurrentSchemaVersion)
                {
                    return new SaveLoadResult(
                        SaveLoadStatus.UnsupportedSchema,
                        null,
                        $"State schema {data.State.SchemaVersion} is unsupported; expected {GameState.CurrentSchemaVersion}.");
                }

                return new SaveLoadResult(
                    SaveLoadStatus.Success,
                    GameState.FromSnapshot(data.State),
                    "Save loaded.");
            }
            catch (NotSupportedException exception)
            {
                return new SaveLoadResult(SaveLoadStatus.UnsupportedSchema, null, exception.Message);
            }
            catch (Exception exception)
            {
                return Corrupted(exception.Message);
            }
        }

        public SaveLoadResult Validate()
        {
            return Load();
        }

        public void Delete()
        {
            _storage.Delete();
        }

        private static SaveLoadResult Corrupted(string message)
        {
            return new SaveLoadResult(SaveLoadStatus.Corrupted, null, message);
        }
    }
}
