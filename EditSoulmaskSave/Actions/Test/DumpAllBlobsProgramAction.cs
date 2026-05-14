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

namespace EditSoulmaskSave.Actions.Test
{
	/// <summary>
	/// Dumps all actor data blobs in a save file to individual binary files
	/// </summary>
	internal class DumpAllBlobsProgramAction : ProgramAction
	{
		public string SavePath { get; private set; }

		public string OutDir { get; private set; }

		public DumpAllBlobsProgramAction(string savePath, string outDir)
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

		public override bool Execute(Logger logger)
		{
			logger.Important("Dumping all actor raw data...");

			bool success = true;
			foreach (SaveDataRow row in ActorDataUtil.GetActors(SavePath, null, logger))
			{
				string name = row.Name.Substring(row.Name.LastIndexOf(':') + 1);
				string outPath = Path.Combine(OutDir, $"{row.Serial}_{name}.dat");

				using FileStream outFile = File.Create(outPath);
				row.Data?.CopyTo(outFile);

				row.Dispose();
			}

			return success;
		}
	}
}
