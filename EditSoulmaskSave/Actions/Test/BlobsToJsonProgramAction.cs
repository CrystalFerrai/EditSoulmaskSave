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
using System.Text;
using UeSaveGame;
using UeSaveGame.Util;

namespace EditSoulmaskSave.Actions.Test
{
	/// <summary>
	/// Converts a directory of actor data binary files to json files
	/// </summary>
	internal class BlobsToJsonProgramAction : ProgramAction
	{
		public string InDir { get; private set; }

		public string OutDir { get; private set; }

		public BlobsToJsonProgramAction(string inDir, string outDir)
		{
			InDir = inDir;
			OutDir = outDir;
		}

		public override bool ValidateAndPrepare(Logger logger)
		{
			string? inDir = ValidateDirectoryPath(logger, InDir, false);
			if (inDir is null) return false;
			InDir = inDir;

			string? outDir = ValidateDirectoryPath(logger, OutDir, true);
			if (outDir is null) return false;
			OutDir = outDir;

			return true;
		}

		public override bool Execute(Logger logger)
		{
			logger.Important("Converting actor data blobs from binary to json...");

			string[] inPaths = Directory.GetFiles(InDir, "*.dat", SearchOption.TopDirectoryOnly);
			foreach (string inPath in inPaths)
			{
				string fileName = Path.GetFileNameWithoutExtension(inPath);
				string outPath = Path.Combine(OutDir, $"{fileName}.json");
				string className = fileName[(fileName.IndexOf('_') + 1)..];

				using FileStream inFile = File.OpenRead(inPath);
				using BinaryReader reader = new(inFile, Encoding.ASCII, true);

				using FileStream outFile = File.Create(outPath);
				using StreamWriter stream = new(outFile);
				using JsonTextWriter jsonWriter = new(stream)
				{
					Formatting = Formatting.Indented,
					Indentation = 2,
					IndentChar = ' '
				};

				if (className.Equals("GAME_SETTINGS"))
				{
					FString? value = reader.ReadUnrealString();
					if (value is null)
					{
						logger.Warning("Failed to read GAME_SETTINGS");
						continue;
					}

					jsonWriter.WriteRaw(value.Value);
				}
				else
				{
					int version = reader.ReadInt32();
					if (version != ActorDataUtil.DataVersion)
					{
						logger.Warning($"Actor version {version} does not match supported version {ActorDataUtil.DataVersion}");
					}

					FPropertyTag[] properties = PropertySerializationHelper.ReadProperties(reader, ActorDataUtil.UEPackageVersion, true).ToArray();
					UeSaveGame.Json.PropertiesSerializer.ToJson(properties, jsonWriter);
				}
			}

			return true;
		}
	}
}
