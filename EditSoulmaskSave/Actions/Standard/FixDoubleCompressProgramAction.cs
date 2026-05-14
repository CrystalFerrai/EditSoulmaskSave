// Copyright 2026 Crystal Ferrai
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using EditSoulmaskSave.SaveData;

namespace EditSoulmaskSave.Actions.Standard
{
	/// <summary>
	/// Modifies a save file to fix a specific bug where palyer states in an account.db can become double compressed
	/// if a server cluster contains servers running different versions of the game when a player connects.
	/// </summary>
	internal class FixDoubleCompressProgramAction : ProgramAction
	{
		public string SavePath { get; private set; }

		public FixDoubleCompressProgramAction(string savePath)
		{
			SavePath = savePath;
		}

		public override bool ValidateAndPrepare(Logger logger)
		{
			string? savePath = ValidateFilePath(logger, SavePath);
			if (savePath is null) return false;

			SavePath = savePath;
			return true;
		}

		public override bool Execute(Logger logger)
		{
			logger.Important("Attempting to fix double compressed player states...");

			bool success = true;

			List<SaveDataRow> rows = new();
			foreach (SaveDataRow row in ActorDataUtil.GetActors(SavePath, "/Script/WS.HPlayerState", logger))
			{
				if (row.Version == -131074)
				{
					logger.Information($"Found potential fixable player state {row.Name}");

					Stream? inData = row.Data;
					if (inData is null)
					{
						logger.Warning($"Unable to fix player state {row.Name}. Actor data is missing or could not be loaded.");
						row.Dispose();
						continue;
					}

					Stream? outData = ActorDataUtil.DecompressBlob(row.Serial, inData, logger);
					if (outData is null)
					{
						logger.Warning($"Unable to fix player state {row.Name}. Actor data could not be decompressed.");
						row.Dispose();
						success = false;
						continue;
					}

					inData.Dispose();

					row.Version = -ActorDataUtil.DataVersion;
					row.Data = outData;

					rows.Add(row);
				}
				else
				{
					row.Dispose();
				}
			}

			if (rows.Count == 0)
			{
				if (success)
				{
					logger.Information("Found no player states with the double-compression issue.");
				}
				else
				{
					logger.Information("No player states are able to be repaired.");
				}
				return success;
			}

			logger.Information($"Fixing {rows.Count} player state{(rows.Count != 1 ? "s" : string.Empty)}...");

			bool updated = ActorDataUtil.UpdateActors(rows, SavePath, logger);
			return updated && success;
		}
	}
}
