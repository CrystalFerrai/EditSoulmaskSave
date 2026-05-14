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
using System.Numerics;
using UeSaveGame.DataTypes;

namespace EditSoulmaskSave.Misc
{
	/// <summary>
	/// Helper for serializing some shared custom types
	/// </summary>
	internal static class SerializationUtil
	{
		public static FVector ReadVector(this BinaryReader reader)
		{
			FVector instance = new FVector();
			instance.X = reader.ReadSingle();
			instance.Y = reader.ReadSingle();
			instance.Z = reader.ReadSingle();
			return instance;
		}

		public static void WriteVector(this BinaryWriter writer, FVector value)
		{
			writer.Write((float)value.X);
			writer.Write((float)value.Y);
			writer.Write((float)value.Z);
		}

		public static Matrix4x4 ReadMatrix(this BinaryReader reader)
		{
			Matrix4x4 matrix = new();

			matrix.M11 = reader.ReadSingle();
			matrix.M12 = reader.ReadSingle();
			matrix.M13 = reader.ReadSingle();
			matrix.M14 = reader.ReadSingle();

			matrix.M21 = reader.ReadSingle();
			matrix.M22 = reader.ReadSingle();
			matrix.M23 = reader.ReadSingle();
			matrix.M24 = reader.ReadSingle();

			matrix.M31 = reader.ReadSingle();
			matrix.M32 = reader.ReadSingle();
			matrix.M33 = reader.ReadSingle();
			matrix.M34 = reader.ReadSingle();

			matrix.M41 = reader.ReadSingle();
			matrix.M42 = reader.ReadSingle();
			matrix.M43 = reader.ReadSingle();
			matrix.M44 = reader.ReadSingle();

			return matrix;
		}

		public static void WriteMatrix(this BinaryWriter writer, Matrix4x4 value)
		{
			writer.Write(value.M11);
			writer.Write(value.M12);
			writer.Write(value.M13);
			writer.Write(value.M14);

			writer.Write(value.M21);
			writer.Write(value.M22);
			writer.Write(value.M23);
			writer.Write(value.M24);

			writer.Write(value.M31);
			writer.Write(value.M32);
			writer.Write(value.M33);
			writer.Write(value.M34);

			writer.Write(value.M41);
			writer.Write(value.M42);
			writer.Write(value.M43);
			writer.Write(value.M44);
		}

		public static FVector ReadVector(this JsonReader reader)
		{
			FVector instance = new();
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
						case nameof(FVector.X):
							instance.X = (float)reader.ReadAsDouble()!;
							break;
						case nameof(FVector.Y):
							instance.Y = (float)reader.ReadAsDouble()!;
							break;
						case nameof(FVector.Z):
							instance.Z = (float)reader.ReadAsDouble()!;
							break;
					}
				}
			}
			return instance;
		}

		public static void WriteVector(this JsonWriter writer, FVector value)
		{
			Formatting previousFormatting = writer.Formatting;
			writer.Formatting = Formatting.None;

			writer.WriteWhitespace(" ");
			writer.WriteStartObject();

			writer.WritePropertyName(nameof(FVector.X));
			writer.WriteValue(value.X);
			writer.WritePropertyName(nameof(FVector.Y));
			writer.WriteValue(value.Y);
			writer.WritePropertyName(nameof(FVector.Z));
			writer.WriteValue(value.Z);

			writer.WriteEndObject();

			writer.Flush();
			writer.Formatting = previousFormatting;
		}

		public static Matrix4x4 ReadMatrix(this JsonReader reader)
		{
			Matrix4x4 instance = new();
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
						case nameof(Matrix4x4.M11):
							instance.M11 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M12):
							instance.M12 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M13):
							instance.M13 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M14):
							instance.M14 = (float)reader.ReadAsDouble()!;
							break;

						case nameof(Matrix4x4.M21):
							instance.M21 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M22):
							instance.M22 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M23):
							instance.M23 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M24):
							instance.M24 = (float)reader.ReadAsDouble()!;
							break;

						case nameof(Matrix4x4.M31):
							instance.M31 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M32):
							instance.M32 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M33):
							instance.M33 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M34):
							instance.M34 = (float)reader.ReadAsDouble()!;
							break;

						case nameof(Matrix4x4.M41):
							instance.M41 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M42):
							instance.M42 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M43):
							instance.M43 = (float)reader.ReadAsDouble()!;
							break;
						case nameof(Matrix4x4.M44):
							instance.M44 = (float)reader.ReadAsDouble()!;
							break;
					}
				}
			}
			return instance;
		}

		public static void WriteMatrix(this JsonWriter writer, Matrix4x4 value)
		{
			Formatting previousFormatting = writer.Formatting;

			writer.WriteStartObject();
			writer.Formatting = Formatting.None;

			writer.WritePropertyName(nameof(Matrix4x4.M11));
			writer.WriteValue(value.M11);
			writer.WritePropertyName(nameof(Matrix4x4.M12));
			writer.WriteValue(value.M12);
			writer.WritePropertyName(nameof(Matrix4x4.M13));
			writer.WriteValue(value.M13);
			writer.WritePropertyName(nameof(Matrix4x4.M14));
			writer.WriteValue(value.M14);

			writer.WritePropertyName(nameof(Matrix4x4.M21));
			writer.WriteValue(value.M21);
			writer.WritePropertyName(nameof(Matrix4x4.M22));
			writer.WriteValue(value.M22);
			writer.WritePropertyName(nameof(Matrix4x4.M23));
			writer.WriteValue(value.M23);
			writer.WritePropertyName(nameof(Matrix4x4.M24));
			writer.WriteValue(value.M24);

			writer.WritePropertyName(nameof(Matrix4x4.M31));
			writer.WriteValue(value.M31);
			writer.WritePropertyName(nameof(Matrix4x4.M32));
			writer.WriteValue(value.M32);
			writer.WritePropertyName(nameof(Matrix4x4.M33));
			writer.WriteValue(value.M33);
			writer.WritePropertyName(nameof(Matrix4x4.M34));
			writer.WriteValue(value.M34);

			writer.WritePropertyName(nameof(Matrix4x4.M41));
			writer.WriteValue(value.M41);
			writer.WritePropertyName(nameof(Matrix4x4.M42));
			writer.WriteValue(value.M42);
			writer.WritePropertyName(nameof(Matrix4x4.M43));
			writer.WriteValue(value.M43);
			writer.WritePropertyName(nameof(Matrix4x4.M44));
			writer.WriteValue(value.M44);

			writer.WriteEndObject();

			writer.Flush();
			writer.Formatting = previousFormatting;
		}
	}
}
