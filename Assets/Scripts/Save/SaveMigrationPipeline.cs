using System;
using NullPointer.Core;
using UnityEngine;

namespace NullPointer.Save
{
    public sealed class SaveMigrationPipeline
    {
        public SaveMigrationResult UpgradeToCurrent(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                throw new FormatException("The save payload is empty.");
            }

            SaveSchemaHeader header = JsonUtility.FromJson<SaveSchemaHeader>(json);
            if (header == null || header.SchemaVersion < 1)
            {
                throw new FormatException("The save payload has no supported schema version.");
            }

            if (header.SchemaVersion > SaveGameData.CurrentSchemaVersion)
            {
                throw new NotSupportedException(
                    $"Save schema {header.SchemaVersion} is newer than supported schema {SaveGameData.CurrentSchemaVersion}.");
            }

            return header.SchemaVersion switch
            {
                1 => MigrateVersionOne(json),
                SaveGameData.CurrentSchemaVersion => ReadCurrent(json),
                _ => throw new NotSupportedException($"Save schema {header.SchemaVersion} is unsupported.")
            };
        }

        private static SaveMigrationResult MigrateVersionOne(string json)
        {
            LegacySaveGameDataV1 legacy = JsonUtility.FromJson<LegacySaveGameDataV1>(json);
            if (legacy == null || legacy.State == null)
            {
                throw new FormatException("The legacy save has no state payload.");
            }

            GameStateSnapshot migratedState = MigrateStateToCurrent(legacy.State);
            var current = new SaveGameData
            {
                SchemaVersion = SaveGameData.CurrentSchemaVersion,
                GameVersion = legacy.GameVersion ?? string.Empty,
                SavedAtUtc = legacy.SavedAtUtc ?? string.Empty,
                State = migratedState
            };
            current.PayloadHash = SavePayloadIntegrity.Compute(current.State);
            return new SaveMigrationResult(current, true);
        }

        private static SaveMigrationResult ReadCurrent(string json)
        {
            SaveGameData current = JsonUtility.FromJson<SaveGameData>(json);
            if (current == null || current.State == null)
            {
                throw new FormatException("The save has no state payload.");
            }

            if (current.State.SchemaVersion > GameState.CurrentSchemaVersion)
            {
                throw new NotSupportedException(
                    $"State schema {current.State.SchemaVersion} is newer than supported schema {GameState.CurrentSchemaVersion}.");
            }

            if (current.State.SchemaVersion < GameState.CurrentSchemaVersion)
            {
                throw new NotSupportedException(
                    $"State schema {current.State.SchemaVersion} requires a save-schema migration that is not available.");
            }

            if (!SavePayloadIntegrity.Matches(current.State, current.PayloadHash))
            {
                throw new FormatException("The save payload integrity check failed.");
            }

            return new SaveMigrationResult(current, false);
        }

        private static GameStateSnapshot MigrateStateToCurrent(GameStateSnapshot state)
        {
            if (state.SchemaVersion > GameState.CurrentSchemaVersion)
            {
                throw new NotSupportedException(
                    $"State schema {state.SchemaVersion} is newer than supported schema {GameState.CurrentSchemaVersion}.");
            }

            if (state.SchemaVersion == GameState.CurrentSchemaVersion)
            {
                return GameState.FromSnapshot(state).CreateSnapshot();
            }

            if (state.SchemaVersion != 1)
            {
                throw new NotSupportedException($"State schema {state.SchemaVersion} is unsupported.");
            }

            var migrated = new GameStateSnapshot
            {
                SchemaVersion = GameState.CurrentSchemaVersion,
                CurrentCaseId = state.CurrentCaseId ?? string.Empty,
                LocationId = state.LocationId ?? string.Empty,
                CheckpointId = state.CheckpointId ?? string.Empty,
                StoryFlags = state.StoryFlags,
                CollectedEvidenceIds = state.CollectedEvidenceIds,
                UnlockedMemoryIds = state.UnlockedMemoryIds,
                CompletedDeductionIds = state.CompletedDeductionIds
            };
            return GameState.FromSnapshot(migrated).CreateSnapshot();
        }

        [Serializable]
        private sealed class SaveSchemaHeader
        {
            public int SchemaVersion;
        }

        [Serializable]
        private sealed class LegacySaveGameDataV1
        {
            public int SchemaVersion = 1;
            public string GameVersion = string.Empty;
            public string SavedAtUtc = string.Empty;
            public GameStateSnapshot State = new GameStateSnapshot();
        }
    }

    public sealed class SaveMigrationResult
    {
        public SaveMigrationResult(SaveGameData data, bool wasMigrated)
        {
            Data = data ?? throw new ArgumentNullException(nameof(data));
            WasMigrated = wasMigrated;
        }

        public SaveGameData Data { get; }

        public bool WasMigrated { get; }
    }
}
