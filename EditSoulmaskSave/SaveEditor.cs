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

using EditSoulmaskSave.Actions;
using SoulmaskSave.PropertySerializers;
using SoulmaskSave.PropertyTypes;
using UeSaveGame;
using UeSaveGame.Json;
using UeSaveGame.PropertyTypes;

namespace EditSoulmaskSave
{
	/// <summary>
	/// Main application class which runs program actions
	/// </summary>
	internal class SaveEditor
	{
		private readonly Logger mLogger;

		static SaveEditor()
		{
			FProperty.RegisterPropertyType(nameof(ObjectProperty), typeof(WSObjectProperty));
			PropertiesSerializer.RegisterPropertySerializer(nameof(ObjectProperty), new WSObjectPropertySerializer());
		}

		public SaveEditor(Logger logger)
		{
			mLogger = logger;
		}

		/// <summary>
		/// Run all actions specified in program options
		/// </summary>
		/// <param name="options">The program options defining what to do</param>
		/// <returns>True if all actions succeed, else false</returns>
		public bool Run(ProgramOptions options)
		{
			bool success = true;

			foreach (ProgramAction action in options.Actions)
			{
				bool isPrepared = action.ValidateAndPrepare(mLogger);
				if (!isPrepared)
				{
					success = false;
					continue;
				}

				try
				{
					success &= action.Execute(mLogger);
				}
				catch (Exception ex)
				{
					mLogger.Error($"Action failed. [{ex.GetType().FullName}] {ex.Message}");
					success = false;
				}
			}

			return success;
		}
	}
}
