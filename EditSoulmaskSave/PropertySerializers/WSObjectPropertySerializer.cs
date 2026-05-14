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

using EditSoulmaskSave.ObjectTypes;
using EditSoulmaskSave.ObjectTypeSerializers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SoulmaskSave.PropertyTypes;
using System.Reflection;
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
		private static Dictionary<Type, Type> sCustomGameObjectSerializerMap;

		private const string CustomDataName = "CustomData";

		static WSObjectPropertySerializer()
		{
			sCustomGameObjectSerializerMap = new();

			foreach (TypeInfo type in Assembly.GetExecutingAssembly().DefinedTypes)
			{
				if (!type.IsAssignableTo(typeof(IGameObjectSerializer)) || type.IsAbstract) continue;

				foreach (GameObjectSerializerAttribute attr in type.GetCustomAttributes<GameObjectSerializerAttribute>())
				{
					if (!sCustomGameObjectSerializerMap.TryAdd(attr.Type, type.AsType()))
					{
						throw new ApplicationException($"Found multiple game object serializer implementations for the type {attr.Type.Name}");
					}
				}
			}
		}

		public void ToJson(FProperty property, JsonWriter writer)
		{
			WSObjectProperty objectProperty = (WSObjectProperty)property;

			writer.WriteStartObject();

			writer.WritePropertyName(nameof(WSObjectProperty.ObjectFlags));
			writer.WriteValue((byte)objectProperty.ObjectFlags);

			if (objectProperty.ObjectFlags == WSObjectPropertyFlags.HasValue || objectProperty.ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference))
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
			
			if (objectProperty.ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference) && !objectProperty.ObjectFlags.HasFlag(WSObjectPropertyFlags.NoData))
			{
				writer.WritePropertyName(nameof(WSObjectProperty.ObjectProperties));
				PropertiesSerializer.ToJson(objectProperty.ObjectProperties, writer);
			}

			if (objectProperty.CustomGameObject is not null && sCustomGameObjectSerializerMap.TryGetValue(objectProperty.CustomGameObject.GetType(), out Type? type))
			{
				writer.WritePropertyName(CustomDataName);
				writer.WriteStartObject();

				writer.WritePropertyName("Type");
				writer.WriteValue(objectProperty.CustomGameObject.GetType().FullName);

				writer.WritePropertyName("Value");
				IGameObjectSerializer serializer = (IGameObjectSerializer)Activator.CreateInstance(type)!;
				serializer.ToJson(writer, objectProperty.CustomGameObject);

				writer.WriteEndObject();
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
						case CustomDataName:
							objectProperty.CustomGameObject = ReadGameObject(reader);
							break;
					}
				}
			}
		}

		private IGameObject? ReadGameObject(JsonReader reader)
		{
			string? typeName = null;
			Type? type = null;
			JToken? valueToken = null;

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
						case "Type":
							typeName = reader.ReadAsString();
							if (typeName is not null)
							{
								type = Type.GetType(typeName);
							}
							break;
						case "Value":
							if (reader.ReadAndMoveToContent())
							{
								valueToken = JToken.ReadFrom(reader);
							}
							break;
					}
				}
			}

			if (valueToken is null) return null;

			if (type is null) throw new InvalidDataException($"Custom data type {typeName} does not exist");
			if (!type.IsAssignableTo(typeof(IGameObject))) throw new InvalidOperationException($"Custom data type {typeName} is not an IGameObject");

			if (!sCustomGameObjectSerializerMap.TryGetValue(type, out Type? serializerType))
			{
				throw new InvalidOperationException($"Custom data type {typeName} has no serializer");
			}

			IGameObjectSerializer serializer = (IGameObjectSerializer)Activator.CreateInstance(serializerType)!;
			using JTokenReader valueReader = new(valueToken);
			return serializer.FromJson(valueReader);
		}
	}
}
