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

namespace EditSoulmaskSave.SaveData
{
	/// <summary>
	/// An actor record from a Soulmask save file
	/// </summary>
	internal class SaveDataRow : IDisposable
	{
		/// <summary>
		/// The primary key that identifies the record
		/// </summary>
		public int Serial { get; }

		/// <summary>
		/// The server_id field
		/// </summary>
		public int ServerId { get; set; }

		/// <summary>
		/// The actor_version field
		/// </summary>
		public int Version { get; set; }

		/// <summary>
		/// The actor_name field
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The actor_level field
		/// </summary>
		public string? Level { get; set; }

		/// <summary>
		/// The actor_script field
		/// </summary>
		public string Script { get; }

		/// <summary>
		/// The actor_owner field
		/// </summary>
		public string? Owner { get; set; }

		/// <summary>
		/// The actor_transf field
		/// </summary>
		public LinearTransform? Transform { get; set; }

		/// <summary>
		/// The actor_data field
		/// </summary>
		public Stream? Data { get; set; }

		/// <summary>
		/// The actor_time field
		/// </summary>
		public DateTime? Time { get; set; }

		public SaveDataRow(
			int serial,
			int serverId,
			int version,
			string name,
			string? level,
			string script,
			string? owner,
			LinearTransform? transform,
			Stream? data,
			DateTime? time)
		{
			Serial = serial;
			ServerId = serverId;
			Version = version;
			Name = name;
			Level = level;
			Script = script;
			Owner = owner;
			Transform = transform;
			Data = data;
			Time = time;
		}

		~SaveDataRow()
		{
			OnDispose(false);
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			OnDispose(true);
		}

		protected virtual void OnDispose(bool disposing)
		{
			if (disposing)
			{
				Data?.Dispose();
			}
		}

		public override int GetHashCode()
		{
			return Serial.GetHashCode();
		}

		public override bool Equals(object? obj)
		{
			return obj is SaveDataRow other && Serial.Equals(other.Serial);
		}

		public override string ToString()
		{
			return $"[{Serial}] {Name} ({Script})";
		}
	}
}
