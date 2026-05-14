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

using Newtonsoft.Json;

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

		/// <summary>
		/// Creates a save data row from serialized json data. The Data property of the resulting row will be null.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="logger">For logging warnings and errors</param>
		/// <returns>The row if reading was successul, else null</returns>
		public static SaveDataRow? FromJson(JsonReader reader, Logger logger)
		{
			int? serial = null, serverId = null, version = null;
			string? name = null, level = null, script = null, owner = null;
			LinearTransform? transform = null;
			DateTime? time = null;

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject)
				{
					break;
				}

				if (reader.TokenType == JsonToken.PropertyName)
				{
					switch (reader.Value)
					{
						case nameof(Serial):
							serial = reader.ReadAsInt32();
							break;
						case nameof(ServerId):
							serverId = reader.ReadAsInt32();
							break;
						case nameof(Version):
							version = reader.ReadAsInt32();
							break;
						case nameof(Name):
							name = reader.ReadAsString();
							break;
						case nameof(Level):
							level = reader.ReadAsString();
							break;
						case nameof(Script):
							script = reader.ReadAsString();
							break;
						case nameof(Owner):
							owner = reader.ReadAsString();
							break;
						case nameof(Transform):
							{
								reader.Read();
								if (reader.TokenType == JsonToken.String && LinearTransform.TryParse((string)reader.Value, out LinearTransform value))
								{
									transform = value;
								}
								else
								{
									logger.Warning("Unable to read actor transform from json");
								}
							}
							break;
						case nameof(Time):
							{
								reader.Read();
								if (reader.TokenType == JsonToken.String && DateTime.TryParse((string)reader.Value, out DateTime value))
								{
									time = value;
								}
								else
								{
									logger.Warning("Unable to read actor time from json");
								}
							}
							break;
					}
				}
			}

			if (!serial.HasValue || !serverId.HasValue || !version.HasValue || name is null || script is null)
			{
				logger.Error("Unable to read actor data from json. Missing one or more required properties.");
				return null;
			}

			return new(serial.Value, serverId.Value, version.Value, name, level, script, owner, transform, null, time);
		}

		/// <summary>
		/// Serializes the save data row to json. This function does not serialize the Data property.
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		/// <param name="logger">For logging warnings and errors</param>
		public void ToJson(JsonWriter writer, Logger logger)
		{
			writer.WriteStartObject();

			writer.WritePropertyName(nameof(Serial));
			writer.WriteValue(Serial);

			writer.WritePropertyName(nameof(ServerId));
			writer.WriteValue(ServerId);

			writer.WritePropertyName(nameof(Version));
			writer.WriteValue(Version);

			writer.WritePropertyName(nameof(Name));
			writer.WriteValue(Name);

			writer.WritePropertyName(nameof(Level));
			writer.WriteValue(Level);

			writer.WritePropertyName(nameof(Script));
			writer.WriteValue(Script);

			writer.WritePropertyName(nameof(Owner));
			writer.WriteValue(Owner);

			writer.WritePropertyName(nameof(Transform));
			writer.WriteValue(Transform?.ToString());

			writer.WritePropertyName(nameof(Time));
			writer.WriteValue(Time);

			writer.WriteEndObject();
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
