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

using UeSaveGame;

namespace EditSoulmaskSave.SaveData
{
	/// <summary>
	/// Base for classes that represent game actor types
	/// </summary>
	internal abstract class GameActorBase
	{
	}

	/// <summary>
	/// Represents a saved game actor
	/// </summary>
	internal class GameActor : GameActorBase
	{
		public List<FPropertyTag> Properties { get; }

		public GameActor()
		{
			Properties = new();
		}

		public GameActor(IEnumerable<FPropertyTag> properties)
		{
			Properties = new(properties);
		}
	}
}
