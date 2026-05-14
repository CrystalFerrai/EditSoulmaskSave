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
using EditSoulmaskSave.ObjectTypes;
using Newtonsoft.Json;
using System.Numerics;

namespace EditSoulmaskSave.ObjectTypeSerializers
{
	/// <summary>
	/// Json serializer for HierarchicalInstancedStaticMeshComponent
	/// </summary>
	[GameObjectSerializer(typeof(HierarchicalInstancedStaticMeshComponent))]
	internal class HeirarchicalInstancedStaticMeshComponentSerializer : GameObjectSerializerBase<HierarchicalInstancedStaticMeshComponent>
	{
		protected override HierarchicalInstancedStaticMeshComponent ReadJson(JsonReader reader)
		{
			HierarchicalInstancedStaticMeshComponent instance = new();

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
						case nameof(HierarchicalInstancedStaticMeshComponent.PerInstanceSMData):
							while (reader.Read())
							{
								if (reader.TokenType == JsonToken.EndArray)
								{
									break;
								}

								if (reader.TokenType == JsonToken.StartObject)
								{
									instance.PerInstanceSMData.Add(reader.ReadMatrix());
								}
							}
							break;
						case nameof(HierarchicalInstancedStaticMeshComponent.PerInstanceSMCustomData):
							while (reader.Read())
							{
								if (reader.TokenType == JsonToken.EndArray)
								{
									break;
								}

								if (reader.TokenType == JsonToken.Float)
								{
									instance.PerInstanceSMCustomData.Add((float)(double)reader.Value);
								}
							}
							break;
						case nameof(HierarchicalInstancedStaticMeshComponent.ClusterTreePtr):
							while (reader.Read())
							{
								if (reader.TokenType == JsonToken.EndArray)
								{
									break;
								}

								if (reader.TokenType == JsonToken.StartObject)
								{
									instance.ClusterTreePtr.Add(FClusterNodeSerializer.ReadClusterNode(reader));
								}
							}
							break;
					}
				}
			}

			return instance;
		}

		protected override void WriteJson(JsonWriter writer, HierarchicalInstancedStaticMeshComponent gameObject)
		{
			Formatting previousFormatting = writer.Formatting;

			writer.WriteStartObject();

			writer.WritePropertyName(nameof(HierarchicalInstancedStaticMeshComponent.PerInstanceSMData));
			writer.WriteStartArray();
			foreach (Matrix4x4 value in gameObject.PerInstanceSMData)
			{
				writer.WriteMatrix(value);
			}
			writer.WriteEndArray();

			writer.WritePropertyName(nameof(HierarchicalInstancedStaticMeshComponent.PerInstanceSMCustomData));
			writer.Formatting = Formatting.None;
			writer.WriteStartArray();
			foreach (float value in gameObject.PerInstanceSMCustomData)
			{
				writer.WriteValue(value);
			}
			writer.WriteEndArray();
			writer.Formatting = previousFormatting;

			writer.WritePropertyName(nameof(HierarchicalInstancedStaticMeshComponent.ClusterTreePtr));
			writer.WriteStartArray();
			foreach (FClusterNode value in gameObject.ClusterTreePtr)
			{
				FClusterNodeSerializer.WriteClusterNode(writer, value);
			}
			writer.WriteEndArray();

			writer.WriteEndObject();
		}
	}

	internal static class FClusterNodeSerializer
	{
		public static FClusterNode ReadClusterNode(JsonReader reader)
		{
			FClusterNode instance = new();

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
						case nameof(FClusterNode.BoundMin):
							instance.BoundMin = reader.ReadVector();
							break;
						case nameof(FClusterNode.FirstChild):
							instance.FirstChild = reader.ReadAsInt32()!.Value;
							break;
						case nameof(FClusterNode.BoundMax):
							instance.BoundMax = reader.ReadVector();
							break;
						case nameof(FClusterNode.LastChild):
							instance.LastChild = reader.ReadAsInt32()!.Value;
							break;
						case nameof(FClusterNode.FirstInstance):
							instance.FirstInstance = reader.ReadAsInt32()!.Value;
							break;
						case nameof(FClusterNode.LastInstance):
							instance.LastInstance = reader.ReadAsInt32()!.Value;
							break;
						case nameof(FClusterNode.MinInstanceScale):
							instance.MinInstanceScale = reader.ReadVector();
							break;
						case nameof(FClusterNode.MaxInstanceScale):
							instance.MaxInstanceScale = reader.ReadVector();
							break;
					}
				}
			}

			return instance;
		}

		public static void WriteClusterNode(JsonWriter writer, FClusterNode value)
		{
			writer.WriteStartObject();

			writer.WritePropertyName(nameof(FClusterNode.BoundMin));
			writer.WriteVector(value.BoundMin);

			writer.WritePropertyName(nameof(FClusterNode.FirstChild));
			writer.WriteValue(value.FirstChild);

			writer.WritePropertyName(nameof(FClusterNode.BoundMax));
			writer.WriteVector(value.BoundMax);

			writer.WritePropertyName(nameof(FClusterNode.LastChild));
			writer.WriteValue(value.LastChild);

			writer.WritePropertyName(nameof(FClusterNode.FirstInstance));
			writer.WriteValue(value.FirstInstance);

			writer.WritePropertyName(nameof(FClusterNode.LastInstance));
			writer.WriteValue(value.LastInstance);

			writer.WritePropertyName(nameof(FClusterNode.MinInstanceScale));
			writer.WriteVector(value.MinInstanceScale);

			writer.WritePropertyName(nameof(FClusterNode.MaxInstanceScale));
			writer.WriteVector(value.MaxInstanceScale);

			writer.WriteEndObject();
		}
	}
}
