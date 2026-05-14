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

using EditSoulmaskSave.Actions.Base;
using EditSoulmaskSave.SaveData;

namespace EditSoulmaskSave.Actions.Standard
{
	/// <summary>
	/// Prints details about all player states in a save file
	/// </summary>
	internal class ListPlayersProgramAction : PlayerBaseProgramAction
	{
		public string SavePath { get; private set; }

		public ListPlayersProgramAction(string savePath)
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
			logger.Important("Listing player details...");
			logger.LogEmptyLine(LogLevel.Important);

			logger.Information("Name                Server ID  Server Name                             Last Played");

			return ForEachPlayer(SavePath, ListPlayer, logger);
		}

		private static bool ListPlayer(PlayerState player, Logger logger)
		{
			logger.LogEmptyLine(LogLevel.Information);
			logger.Information(player.AccountId);
			logger.Information($"{player.PlayerName,-20}{player.LatestServerId,-11}{player.LatestServerName,-40}{player.LastPlayed}");
			return true;
		}
	}
}
