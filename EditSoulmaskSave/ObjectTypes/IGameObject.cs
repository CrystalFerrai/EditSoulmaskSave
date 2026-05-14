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

namespace EditSoulmaskSave.ObjectTypes
{
	/// <summary>
	/// Interface for serializable custom game data attached to actors
	/// </summary>
	internal interface IGameObject
	{
		void Deserialize(BinaryReader reader);

		int Serialize(BinaryWriter writer);
	}

	/// <summary>
	/// Attach to IGameObject implementations to specify object types which contain
	/// custom data the implementation is able to serialize.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	internal class GameObjectAttribute : Attribute
	{
		public string TypeName { get; }

		public GameObjectAttribute(string typeName)
		{
			TypeName = typeName;
		}
	}
}
