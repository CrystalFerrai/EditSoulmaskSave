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
using SoulmaskSave.PropertyTypes;
using UeSaveGame;
using UeSaveGame.Json;
using UeSaveGame.PropertyTypes;

namespace SoulmaskSave.PropertySerializers
{
	/// <summary>
	/// Sserializer for the custom Soulmask save file version of ObjectProperty
	/// </summary>
	internal class WSObjectPropertySerializer : IPropertySerializer
	{
		public void ToJson(FProperty property, JsonWriter writer)
		{
			WSObjectProperty objectProperty = (WSObjectProperty)property;

			writer.WriteStartObject();

			writer.WritePropertyName(nameof(WSObjectProperty.ObjectFlags));
			writer.WriteValue((byte)objectProperty.ObjectFlags);

			if (objectProperty.ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference))
			{
				writer.WritePropertyName(nameof(WSObjectProperty.ObjectPath));
				if (objectProperty.ObjectPath is null)
				{
					writer.WriteNull();
				}
				else
				{
					writer.WriteFStringValue(objectProperty.ObjectPath);
				}
			}

			writer.WritePropertyName(nameof(ObjectProperty.ObjectType));
			writer.WriteFStringValue(objectProperty.ObjectType);
			
			if (objectProperty.ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference))
			{
				writer.WritePropertyName(nameof(WSObjectProperty.ObjectProperties));
				PropertiesSerializer.ToJson(objectProperty.ObjectProperties, writer);
			}

			writer.WriteEndObject();
		}

		public void FromJson(FProperty property, JsonReader reader)
		{
			WSObjectProperty objectProperty = (WSObjectProperty)property;

			while (reader.Read())
			{
				if (reader.TokenType == JsonToken.EndObject)
				{
					break;
				}

				if (reader.TokenType == JsonToken.PropertyName)
				{
					switch ((string)reader.Value!)
					{
						case nameof(WSObjectProperty.ObjectFlags):
							objectProperty.ObjectFlags = (WSObjectPropertyFlags)(byte)reader.ReadAsInt32()!;
							break;
						case nameof(WSObjectProperty.ObjectPath):
							objectProperty.ObjectPath = reader.TokenType == JsonToken.Null ? null : reader.ReadAsFString();
							break;
						case nameof(ObjectProperty.ObjectType):
							objectProperty.ObjectType = reader.ReadAsFString();
							break;
						case nameof(WSObjectProperty.ObjectProperties):
							objectProperty.ObjectProperties = new(PropertiesSerializer.FromJson(reader));
							break;
					}
				}
			}
		}
	}
}
