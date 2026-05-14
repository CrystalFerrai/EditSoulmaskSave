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

namespace EditSoulmaskSave.Actions
{
	/// <summary>
	/// An action for the program to perform
	/// </summary>
	internal abstract class ProgramAction
	{
		protected ProgramAction()
		{
		}

		public abstract bool ValidateAndPrepare(Logger logger);

		public abstract bool Execute(Logger logger);

		protected static string? ValidateFilePath(Logger logger, string path)
		{
			try
			{
				string resolvedPath = Path.GetFullPath(path);
				if (!File.Exists(resolvedPath))
				{
					logger.Error($"File not accessible: '{resolvedPath}'");
					return null;
				}
				return resolvedPath;
			}
			catch (Exception ex)
			{
				logger.Error($"Failed to resolve file path: '{path}' [{ex.GetType().FullName}] {ex.Message}");
				return null;
			}
		}

		protected static string? ValidateDirectoryPath(Logger logger, string path, bool create)
		{
			string resolvedPath;
			try
			{
				resolvedPath = Path.GetFullPath(path);
			}
			catch (Exception ex)
			{
				logger.Error($"Failed to resolve directory path: '{path}' [{ex.GetType().FullName}] {ex.Message}");
				return null;
			}

			if (create)
			{
				try
				{
					Directory.CreateDirectory(resolvedPath);
				}
				catch (Exception ex)
				{
					logger.Error($"Unable to create or access directory: '{path}' [{ex.GetType().FullName}] {ex.Message}");
					return null;
				}
			}
			else if (!Directory.Exists(resolvedPath))
			{
				logger.Error($"Directory not accessible: '{resolvedPath}'");
				return null;
			}

			return resolvedPath;
		}
	}
}
