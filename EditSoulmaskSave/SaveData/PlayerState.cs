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

using System.Diagnostics;
using UeSaveGame;
using UeSaveGame.PropertyTypes;

namespace EditSoulmaskSave.SaveData
{
	/// <summary>
	/// A Soulmask player state actor (WS.HPlayerState)
	/// </summary>
	/// <remarks>
	/// Work in progress. Currently read-only.
	/// </remarks>
	internal class PlayerState
	{
		private readonly List<FPropertyTag> mProperties;

		public string AccountId { get; }

		public string? PlayerName { get; }

		public DateTime? LastPlayed { get; }

		public int? LatestServerId { get; }

		public string? LatestServerName { get; }

		public IReadOnlyList<FPropertyTag> Properties => mProperties;

		private PlayerState(
			List<FPropertyTag> properties,
			string accountId,
			string? playerName,
			DateTime? lastPlayed,
			int? latestServerId,
			string? latestServerName)
		{
			mProperties = properties;
			AccountId = accountId;
			PlayerName = playerName;
			LastPlayed = lastPlayed;
			LatestServerId = latestServerId;
			LatestServerName = latestServerName;
		}

		public static PlayerState? Load(SaveDataRow saveData, Logger logger)
		{
			GameActor? actor = ActorDataUtil.ReadActorData(saveData, logger) as GameActor;
			if (actor is null)
			{
				// ReadBlob logs its own error message, so don't need one here
				return null;
			}

			string accountId = saveData.Name;
			DateTime? lastPlayed = saveData.Time;

			string? playerName = null;
			int? latestServerId = null;
			string? latestServerName = null;
			foreach (FPropertyTag property in actor.Properties)
			{
				switch (property.Name.Value)
				{
					case "PlayerName":
						playerName = ((StrProperty?)property.Property)?.Value?.Value;
						break;
					case "LatestServerID":
						latestServerId = ((IntProperty?)property.Property)?.Value;
						break;
					case "LatestServerName":
						latestServerName = ((StrProperty?)property.Property)?.Value?.Value;
						break;
				}
			}

			return new(actor.Properties.ToList(), accountId, playerName, lastPlayed, latestServerId, latestServerName);
		}
	}
}
