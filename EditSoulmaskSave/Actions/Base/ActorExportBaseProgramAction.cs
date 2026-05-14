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
using UeSaveGame.Json;

namespace EditSoulmaskSave.Actions.Base
{
	/// <summary>
	/// Base class for actions that export actor data to json
	/// </summary>
	internal abstract class ActorExportBaseProgramAction : ProgramAction
	{
		public string SavePath { get; private set; }

		public string OutDir { get; private set; }

		protected ActorExportBaseProgramAction(string savePath, string outDir)
		{
			SavePath = savePath;
			OutDir = outDir;
		}

		public override bool ValidateAndPrepare(Logger logger)
		{
			string? savePath = ValidateFilePath(logger, SavePath);
			if (savePath is null) return false;
			SavePath = savePath;

			string? outDir = ValidateDirectoryPath(logger, OutDir, true);
			if (outDir is null) return false;
			OutDir = outDir;

			return true;
		}

		protected bool Export(string? actorClass, Logger logger)
		{
			bool success = true;
			foreach (SaveDataRow row in ActorDataUtil.GetActors(SavePath, actorClass, logger))
			{
				string name = row.Name.Substring(row.Name.LastIndexOf(':') + 1);
				logger.Debug($"{row.Serial}_{name}");

				GameActorBase? actor = ActorDataUtil.ReadActorData(row, logger);
				if (actor is null)
				{
					continue;
				}

				string outPath = Path.Combine(OutDir, $"{row.Serial}_{name}.json");

				ExportActor(row, actor, outPath, logger);

				row.Dispose();
			}

			return success;
		}

		protected static void ExportActor(SaveDataRow row, GameActorBase actor, string outPath, Logger logger)
		{
			using FileStream outFile = File.Create(outPath);
			using StreamWriter stream = new(outFile);
			using JsonTextWriter jsonWriter = new(stream)
			{
				Formatting = Formatting.Indented,
				Indentation = 2,
				IndentChar = ' '
			};

			jsonWriter.WriteStartObject();

			jsonWriter.WritePropertyName("Row");
			row.ToJson(jsonWriter, logger);

			jsonWriter.WritePropertyName("Data");
			if (actor is GameSettings gameSettings)
			{
				jsonWriter.WriteRaw(gameSettings.Data);
			}
			else if (actor is GameActor gameActor)
			{
				PropertiesSerializer.ToJson(gameActor.Properties, jsonWriter);
			}
			else
			{
				jsonWriter.WriteNull();
			}

			jsonWriter.WriteEndObject();
		}
	}
}
