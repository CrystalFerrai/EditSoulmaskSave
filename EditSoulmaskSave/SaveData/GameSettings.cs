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

using System.Text;
using UeSaveGame;
using UeSaveGame.Util;

namespace EditSoulmaskSave.SaveData
{
	/// <summary>
	/// Represents the game settings row stored in save files
	/// </summary>
	internal class GameSettings : GameActorBase
	{
		public string Data { get; set; }

		public GameSettings(string data)
		{
			Data = data;
		}

		public static GameSettings? Load(SaveDataRow saveData, Logger logger)
		{
			if (saveData.Data is null)
			{
				logger.Error($"Actor {saveData.Serial}: Missing GAME_SETTINGS data");
				return null;
			}

			using BinaryReader reader = new(saveData.Data, Encoding.ASCII, true);

			FString? data = reader.ReadUnrealString();
			if (data is null)
			{
				logger.Error($"Actor {saveData.Serial}: Unable to read GAME_SETTINGS data");
				return null;
			}

			return new(data.Value);
		}

		public void Save(SaveDataRow saveData, Logger logger)
		{
			MemoryStream stream = new();
			using BinaryWriter writer = new(stream, Encoding.ASCII, true);
			writer.WriteUnrealString(new(Data));
			writer.Flush();
			saveData.Data = stream;
		}
	}
}
