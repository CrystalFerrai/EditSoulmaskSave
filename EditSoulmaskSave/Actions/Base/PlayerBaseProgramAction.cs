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

namespace EditSoulmaskSave.Actions.Base
{
	/// <summary>
	/// Base class for program actions operating on player states
	/// </summary>
	internal abstract class PlayerBaseProgramAction : ProgramAction
	{
		/// <summary>
		/// Perfoarm an action on every player state within a save file
		/// </summary>
		/// <param name="savePath">The save file path</param>
		/// <param name="action">The action to perform</param>
		/// <param name="logger">For logging errors. Also passed into the action.</param>
		/// <returns>True if the action reports success for all players. Else false.</returns>
		protected static bool ForEachPlayer(string savePath, Func<PlayerState, Logger, bool> action, Logger logger)
		{
			bool success = true;
			foreach (SaveDataRow row in ActorDataUtil.GetActors(savePath, "/Script/WS.HPlayerState", logger))
			{
				PlayerState? playerState = PlayerState.Load(row, logger);
				if (playerState is null)
				{
					// Load logs its own error message, so don't need one here
					success = false;
					continue;
				}

				success &= action(playerState, logger);

				row.Dispose();
			}

			return success;
		}
	}
}
