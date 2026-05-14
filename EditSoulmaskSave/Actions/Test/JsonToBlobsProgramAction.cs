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
	/// Converts a directory of actor data json files to binary files
	/// </summary>
	internal class JsonToBlobsProgramAction : ProgramAction
	{
		public string InDir { get; private set; }

		public string OutDir { get; private set; }

		public JsonToBlobsProgramAction(string inDir, string outDir)
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
			logger.Important("Converting actor data blobs from json to binary...");

			string[] inPaths = Directory.GetFiles(InDir, "*.json", SearchOption.TopDirectoryOnly);
			foreach (string inPath in inPaths)
			{
				string fileName = Path.GetFileNameWithoutExtension(inPath);
				string outPath = Path.Combine(OutDir, $"{fileName}.dat");
				string className = fileName[(fileName.IndexOf('_') + 1)..];

				using FileStream inFile = File.OpenRead(inPath);
				using StreamReader stream = new(inFile);
				using JsonTextReader jsonReader = new(stream);

				using FileStream outFile = File.Create(outPath);
				using BinaryWriter writer = new(outFile, Encoding.ASCII, true);

				if (className.Equals("GAME_SETTINGS"))
				{
					writer.WriteUnrealString(new(stream.ReadToEnd()));
				}
				else
				{
					writer.Write(ActorDataUtil.DataVersion);

					IList<FPropertyTag> properties = UeSaveGame.Json.PropertiesSerializer.FromJson(jsonReader);
					PropertySerializationHelper.WriteProperties(properties, writer, ActorDataUtil.UEPackageVersion, true);
				}
			}

			return true;
		}
	}
}
