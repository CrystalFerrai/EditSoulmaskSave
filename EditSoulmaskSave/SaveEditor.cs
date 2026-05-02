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
using Newtonsoft.Json;
using System.Data;
using UeSaveGame.Json;

namespace EditSoulmaskSave
{
	/// <summary>
	/// Main application class which runs program actions
	/// </summary>
	internal class SaveEditor
	{
		private readonly Logger mLogger;

		public SaveEditor(Logger logger)
		{
			mLogger = logger;
		}

		/// <summary>
		/// Run all actions specified in program options
		/// </summary>
		/// <param name="options">The program options defining what to do</param>
		/// <returns>True if all actions succeed, else false</returns>
		public bool Run(ProgramOptions options)
		{
			bool success = true;

			foreach (ProgramAction action in options.Actions)
			{
				try
				{
					switch (action.ActionType)
					{
						case ProgramActionType.ListPlayers:
							success &= ListPlayers(options.SavePath, action);
							break;
						case ProgramActionType.DumpPlayers:
							success &= DumpPlayers(options.SavePath, action);
							break;
						case ProgramActionType.DumpPlayerBlobs:
							success &= DumpPlayerBlobs(options.SavePath, action);
							break;
						case ProgramActionType.FixDoubleCompress:
							success &= FixDoubleCompress(options.SavePath, action);
							break;
					}
				}
				catch (Exception ex)
				{
					mLogger.Error($"Action failed. [{ex.GetType().FullName}] {ex.Message}");
					success = false;
				}
			}

			return success;
		}

		private bool ListPlayers(string savePath, ProgramAction action)
		{
			mLogger.Important("Listing player details...");
			mLogger.LogEmptyLine(LogLevel.Important);

			mLogger.Information("Name                Server ID  Server Name                             Last Played");

			return ForEachPlayer(savePath, ListPlayer);
		}

		private bool ListPlayer(PlayerState player)
		{
			mLogger.LogEmptyLine(LogLevel.Information);
			mLogger.Information(player.AccountId);
			mLogger.Information($"{player.PlayerName,-20}{player.LatestServerId,-11}{player.LatestServerName,-40}{player.LastPlayed}");
			return true;
		}

		private bool DumpPlayers(string savePath, ProgramAction action)
		{
			mLogger.Important("Dumping player state actor data...");

			string outputDirectory = action.Parameter!;
			Directory.CreateDirectory(outputDirectory);

			return ForEachPlayer(savePath, (s) => DumpPlayer(s, outputDirectory));
		}

		private bool DumpPlayer(PlayerState player, string outputDirectory)
		{
			string outPath = Path.Combine(outputDirectory, $"{player.AccountId}.json");

			using FileStream outFile = File.Create(outPath);
			using StreamWriter stream = new(outFile);
			using JsonTextWriter jsonWriter = new(stream)
			{
				Formatting = Formatting.Indented,
				Indentation = 2,
				IndentChar = ' '
			};

			PropertiesSerializer.ToJson(player.Properties, jsonWriter);

			return true;
		}

		private bool DumpPlayerBlobs(string savePath, ProgramAction action)
		{
			mLogger.Important("Dumping player state actor raw data...");

			string outputDirectory = action.Parameter!;
			Directory.CreateDirectory(outputDirectory);

			bool success = true;
			foreach (SaveDataRow row in ActorDataUtil.GetActors(savePath, "/Script/WS.HPlayerState", mLogger))
			{
				string outPath = Path.Combine(outputDirectory, $"{row.Name}.dat");

				using FileStream outFile = File.Create(outPath);
				row.Data?.CopyTo(outFile);

				row.Dispose();
			}

			return success;
		}

		private bool FixDoubleCompress(string savePath, ProgramAction action)
		{
			mLogger.Important("Attempting to fix double compressed player states...");

			bool success = true;

			List<SaveDataRow> rows = new();
			foreach (SaveDataRow row in ActorDataUtil.GetActors(savePath, "/Script/WS.HPlayerState", mLogger))
			{
				if (row.Version == -131074)
				{
					mLogger.Information($"Found potential fixable player state {row.Name}");

					Stream? inData = row.Data;
					if (inData is null)
					{
						mLogger.Warning($"Unable to fix player state {row.Name}. Actor data is missing or could not be loaded.");
						row.Dispose();
						continue;
					}

					Stream? outData = ActorDataUtil.DecompressBlob(inData, mLogger);
					if (outData is null)
					{
						mLogger.Warning($"Unable to fix player state {row.Name}. Actor data could not be decompressed.");
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
					mLogger.Information("Found no player states with the double-compression issue.");
				}
				else
				{
					mLogger.Information("No player states are able to be repaired.");
				}
				return success;
			}

			mLogger.Information($"Fixing {rows.Count} player state{(rows.Count != 1 ? "s" : string.Empty)}...");

			bool updated = ActorDataUtil.UpdateActors(rows, savePath, mLogger);
			return updated && success;
		}

		private bool ForEachPlayer(string savePath, Predicate<PlayerState> action)
		{
			bool success = true;
			foreach (SaveDataRow row in ActorDataUtil.GetActors(savePath, "/Script/WS.HPlayerState", mLogger))
			{
				PlayerState? playerState = PlayerState.Load(row, mLogger);
				if (playerState is null)
				{
					// Load logs its own error message, so don't need one here
					success = false;
					continue;
				}

				success &= action(playerState);

				row.Dispose();
			}

			return success;
		}
	}
}
