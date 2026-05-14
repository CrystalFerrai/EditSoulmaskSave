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
using Newtonsoft.Json;

namespace EditSoulmaskSave.ObjectTypeSerializers
{
	/// <summary>
	/// Interface for Json serializers that support IGameObject custom data
	/// </summary>
	internal interface IGameObjectSerializer
	{
		IGameObject FromJson(JsonReader reader);

		void ToJson(JsonWriter writer, IGameObject gameObject);
	}

	internal abstract class GameObjectSerializerBase<T> : IGameObjectSerializer where T : IGameObject
	{
		public IGameObject FromJson(JsonReader reader)
		{
			return ReadJson(reader);
		}

		public void ToJson(JsonWriter writer, IGameObject gameObject)
		{
			WriteJson(writer, (T)gameObject);
		}

		protected abstract T ReadJson(JsonReader reader);

		protected abstract void WriteJson(JsonWriter writer, T gameObject);
	}

	/// <summary>
	/// Attach to IGameObjectSerializer implementations to specify IGameObject implementations
	/// that are supported by the serializer.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal class GameObjectSerializerAttribute : Attribute
	{
		public Type Type { get; }

		public GameObjectSerializerAttribute(Type type)
		{
			Type = type;
		}
	}
}
