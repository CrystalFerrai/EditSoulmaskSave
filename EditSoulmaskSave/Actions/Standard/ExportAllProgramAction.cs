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

using EditSoulmaskSave.Actions.Base;

namespace EditSoulmaskSave.Actions.Standard
{
	/// <summary>
	/// Exports all actors from a save file to individual json files
	/// </summary>
	internal class ExportAllProgramAction : ActorExportBaseProgramAction
	{
		public ExportAllProgramAction(string savePath, string outDir)
			: base(savePath, outDir)
		{
		}

		public override bool Execute(Logger logger)
		{
			logger.Important("Exporting all actor data...");
			return Export(null, logger);
		}
	}
}
