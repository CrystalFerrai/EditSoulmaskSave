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

using EditSoulmaskSave.Misc;
using Microsoft.Data.Sqlite;
using SoulmaskSave.PropertySerializers;
using SoulmaskSave.PropertyTypes;
using System.Text;
using UeSaveGame;
using UeSaveGame.Json;
using UeSaveGame.PropertyTypes;

namespace EditSoulmaskSave.SaveData
{
	/// <summary>
	/// Utility functions for working with actors store in a Soulmask save file
	/// </summary>
	internal static class ActorDataUtil
	{
		public static PackageVersion UEPackageVersion { get; }

		/// <summary>
		/// The currently known/supported data version that appears in actor_data blobs.
		/// </summary>
		/// <remarks>
		/// The negative of this value appears in the actor_version save file column.
		/// </remarks>
		public const int DataVersion = 2;

		static ActorDataUtil()
		{
			UEPackageVersion = new()
			{
				PackageVersionUE4 = EObjectUE4Version.VER_UE4_CORRECT_LICENSEE_FLAG
			};
		}

		/// <summary>
		/// Get all actor records of the specified actor type
		/// </summary>
		/// <param name="savePath">The path to the save file to read</param>
		/// <param name="actorClass">The actor class to get instances of, or null to get all actors</param>
		/// <param name="logger">For logging issues</param>
		/// <returns>The found actors. Note that these objects should be disposed when no longer needed.</returns>
		public static IEnumerable<SaveDataRow> GetActors(string savePath, string? actorClass, Logger logger)
		{
			SqliteConnection connection = new($"Data Source={savePath}");
			connection.Open();

			SqliteCommand command = connection.CreateCommand();
			command.CommandText = "select actor_serial, server_id, data_version, actor_name, actor_level, actor_script, actor_owner, actor_transf, actor_data, actor_time from actor_table";
			if (actorClass is not null)
			{
				command.CommandText += " where actor_script = $cls";
				command.Parameters.AddWithValue("$cls", actorClass);
			}

			SqliteDataReader reader = command.ExecuteReader();
			while (reader.Read())
			{
				int serial = reader.GetInt32(0);
				int serverId = reader.GetInt32(1);
				int version = reader.GetInt32(2);
				string name = reader.GetString(3);
				string? level = reader.GetString(4);
				string script = reader.GetString(5);
				string? owner = reader.GetString(6);
				string? transformStr = reader.GetString(7);
				Stream? compressedData = reader.GetStream(8);
				string? timeStr = reader.GetString(9);

				LinearTransform? transform = null;
				if (transformStr is not null)
				{
					if (LinearTransform.TryParse(transformStr, out LinearTransform value))
					{
						transform = value;
					}
					else
					{
						transform = null;
					}
				}

				DateTime? time = null;
				if (timeStr is not null)
				{
					if (DateTime.TryParse(timeStr, out DateTime value))
					{
						time = value;
					}
					else
					{
						time = null;
					}
				}

				Stream? data = null;
				if (compressedData is not null)
				{
					if (name.Equals("GAME_SETTINGS"))
					{
						data = compressedData;
					}
					else
					{
						data = DecompressBlob(serial, compressedData, logger);
						compressedData.Dispose();
					}
				}

				yield return new(serial, serverId, version, name, level, script, owner, transform, data, time);
			}

			connection.Close();
		}

		/// <summary>
		/// Updates the data for a set of actors
		/// </summary>
		/// <param name="actors">The actors to update. Will match rows based on the value of the "Serial" property</param>
		/// <param name="savePath">The path to the save file to modify</param>
		/// <param name="logger">For logging issues</param>
		/// <returns>True if all actors were updated, else false. If false, logger will receive information about what went wrong.</returns>
		public static bool UpdateActors(IEnumerable<SaveDataRow> actors, string savePath, Logger logger)
		{
			if (!actors.Any())
			{
				return true;
			}

			SqliteConnection connection = new($"Data Source={savePath}");
			try
			{
				connection.Open();
			}
			catch (Exception ex)
			{
				logger.Error($"Failed to open save file. [{ex.GetType().FullName}] {ex.Message}");
				return false;
			}

			bool success = true;

			SqliteTransaction transaction = connection.BeginTransaction();

			string lastCheckpoint = "start";
			transaction.Save(lastCheckpoint);
			foreach (SaveDataRow actor in actors)
			{
				try
				{
					Stream? dataStream = null;
					byte[]? data = null;
					if (actor.Data is not null)
					{
						if (actor.Name.Equals("GAME_SETTINGS"))
						{
							data = new byte[actor.Data.Length];
							actor.Data.Read(data, 0, data.Length);
						}
						dataStream = CompressBlob(actor.Serial, actor.Data, logger);
						if (dataStream is not null)
						{
							data = new byte[dataStream.Length];
							dataStream.Read(data, 0, data.Length);
						}
					}

					try
					{
						SqliteCommand command = connection.CreateCommand();
						command.CommandText = "update actor_table set server_id=$serverId, data_version=$data_version, actor_name=$actor_name, actor_level=$actorLevel, actor_script=$actorScript, actor_owner=$actorOwner, actor_transf=$actorTransform, actor_data=$actorData, actor_time=$actorTime where actor_serial=$actorSerial";
						command.Parameters.AddWithValue("$actorSerial", actor.Serial);
						command.Parameters.AddWithValue("$serverId", actor.ServerId);
						command.Parameters.AddWithValue("$data_version", actor.Version);
						command.Parameters.AddWithValue("$actor_name", actor.Name);
						command.Parameters.AddWithValue("$actorLevel", actor.Level);
						command.Parameters.AddWithValue("$actorScript", actor.Script);
						command.Parameters.AddWithValue("$actorOwner", actor.Owner);
						command.Parameters.AddWithValue("$actorTransform", actor.Transform.ToString());
						command.Parameters.Add(new SqliteParameter("$actorData", SqliteType.Blob) { DbType = System.Data.DbType.Binary, Value = data });
						command.Parameters.AddWithValue("$actorTime", actor.Time.ToString());

						int updated = command.ExecuteNonQuery();
						if (updated == 0)
						{
							logger.Warning($"[{actor.Name}] Failed to update actor in database");
						}

						lastCheckpoint = actor.Serial.ToString();
						transaction.Save(lastCheckpoint);
					}
					finally
					{
						if (dataStream is not null)
						{
							dataStream.Dispose();
						}
					}
				}
				catch (Exception ex)
				{
					logger.Error($"[{actor.Name}] Failed to update actor in database. [{ex.GetType().FullName}] {ex.Message}");
					transaction.Rollback(lastCheckpoint);
					success = false;
				}
			}

			transaction.Commit();
			connection.Close();

			return success;
		}

		/// <summary>
		/// Deserialize a save record's Data property
		/// </summary>
		/// <param name="row">The record to read</param>
		/// <param name="logger">For logging issues</param>
		/// <returns>A list of actor properties, or null if there was an error</returns>
		public static GameActorBase? ReadActorData(SaveDataRow row, Logger logger)
		{
			if (row.Data is null)
			{
				logger.Error($"[{row.Name}] Missing data for actor");
				return null;
			}

			FPropertyTag[]? tryRead(BinaryReader reader)
			{
				int dataVersion = reader.ReadInt32();
				if (dataVersion != DataVersion)
				{
					logger.Error($"[{row.Name}] Unexpected version in decompressed data: {dataVersion}");
					return null;
				}

				try
				{
					// Need to enumerate result now so that it is safe to dispose of the data stream. Using ToArray() for this
					return UeSaveGame.Util.PropertySerializationHelper.ReadProperties(reader, UEPackageVersion, true).ToArray();
				}
				catch (Exception ex)
				{
					logger.Error($"[{row.Name}] Failed to read actor data. [{ex.GetType().FullName}] {ex.Message}");
				}

				return null;
			}

			long streamPosition = row.Data.Position;

			if (row.Name.Equals("GAME_SETTINGS"))
			{
				return GameSettings.Load(row, logger);
			}
			else
			{
				if (row.Version == -131074)
				{
					// This is a rare case that has come up due to some bug in the game server software
					using Stream? stream = DecompressBlob(row.Serial, row.Data, logger);
					if (stream is not null)
					{
						logger.Warning($"[{row.Name}] Actor data is double compressed and will not be readable by the game.");

						using BinaryReader newReader = new(stream, Encoding.ASCII, true);
						FPropertyTag[]? newProperties = tryRead(newReader);
						return newProperties is null ? null : new GameActor(newProperties);
					}

					row.Data.Seek(streamPosition, SeekOrigin.Begin);
				}
				else if (row.Version != -DataVersion)
				{
					logger.Warning($"[{row.Name}] Unrecognized actor version {row.Version}");
				}
			}

			using BinaryReader reader = new(row.Data, Encoding.ASCII, true);
			FPropertyTag[]? properties = tryRead(reader);
			return properties is null ? null : new GameActor(properties);
		}

		/// <summary>
		/// Serialize properties to a save record's Data property
		/// </summary>
		/// <remarks>
		/// If successful, the actor's Data property will be set to a enw stream, and the previous stream will be disposed.
		/// </remarks>
		/// <param name="row">The record to modify</param>
		/// <param name="actor">The actor to write</param>
		/// <param name="logger">For logging issues</param>
		/// <returns>True if the actor was updated, else false. If false, logger will receive information about what went wrong.</returns>
		public static bool WriteActorData(SaveDataRow row, GameActorBase actor, Logger logger)
		{
			if (actor is GameSettings gameSettings)
			{
				gameSettings.Save(row, logger);
				return true;
			}

			if (actor is GameActor gameActor)
			{
				MemoryStream stream = new();
				using BinaryWriter writer = new(stream, Encoding.ASCII, true);

				writer.Write(DataVersion);
				try
				{
					UeSaveGame.Util.PropertySerializationHelper.WriteProperties(gameActor.Properties, writer, UEPackageVersion, true);
					stream.Seek(0, SeekOrigin.Begin);
					row.Data = stream;
				}
				catch (Exception ex)
				{
					logger.Error($"[{row.Name}] Error writing actor data. [{ex.GetType().FullName}] {ex.Message}");
					return false;
				}

				return true;
			}

			logger.Error($"Unknown actor class");
			return false;
		}

		/// <summary>
		/// Decompress an actor data binary blob
		/// </summary>
		/// <param name="blob">The data to decompress</param>
		/// <param name="logger">For logging issues</param>
		/// <returns>A stream containing the decompressed data, or null if there was an error</returns>
		public static Stream? DecompressBlob(int actorIndex, Stream blob, Logger logger)
		{
			using BinaryReader blobReader = new(blob, Encoding.ASCII, true);

			int version = blobReader.ReadInt32();
			if (version != DataVersion)
			{
				logger.Error($"Actor {actorIndex}: Unexpected version: {version}");
				return null;
			}

			int decompressedSize = blobReader.ReadInt32();
			MemoryStream dataStream = new(decompressedSize);

			try
			{
				CompressionUtil.LZ4Decompress(blob, decompressedSize, dataStream);
			}
			catch (Exception ex)
			{
				logger.Error($"Actor {actorIndex}: Decompression failed: {ex.Message}");
				dataStream.Dispose();
				return null;
			}

			dataStream.Seek(0, SeekOrigin.Begin);
			return dataStream;
		}

		/// <summary>
		/// Compress an actor binary blob
		/// </summary>
		/// <param name="blob">The data to compress</param>
		/// <param name="logger">For logging issues</param>
		/// <returns>A stream containing the compressed data, or null if there was an error</returns>
		public static Stream? CompressBlob(int actorIndex, Stream blob, Logger logger)
		{
			MemoryStream outStream = new();
			using BinaryWriter writer = new(outStream, Encoding.ASCII, true);

			writer.Write(DataVersion);

			long sizeOffset = outStream.Position;
			writer.Write(0);

			try
			{
				int length = CompressionUtil.LZ4Compress(blob, outStream);
				outStream.Seek(sizeOffset, SeekOrigin.Begin);
				writer.Write(length);
				outStream.Seek(0, SeekOrigin.Begin);
			}
			catch (Exception ex)
			{
				logger.Error($"Actor {actorIndex}: Compression failed: {ex.Message}");
				outStream.Dispose();
				return null;
			}

			return outStream;
		}
	}
}
