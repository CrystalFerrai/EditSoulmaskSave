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

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace EditSoulmaskSave
{
    internal class Program
    {
        private static int Main(string[] args)
		{
			Logger? logger;
			if (!TryCreateLoggger(out logger))
			{
				Console.Error.WriteLine("No logger could be created. Program will exit.");
				return OnExit(1);
			}

			if (args.Length == 0)
			{
				ProgramOptions.PrintUsage(logger, LogLevel.Information);
				return OnExit(0);
			}

			ProgramOptions? options;
			if (!ProgramOptions.TryParseCommandLine(args, logger, out options))
			{
				ProgramOptions.PrintUsage(logger, LogLevel.Information);
				return OnExit(1);
			}

			bool success;
			try
			{
				SaveEditor editor = new(logger);
				success = editor.Run(options);
			}
			catch (Exception ex)
			{
				logger.Error($"{ex.GetType().FullName}: {ex.Message}");
				success = false;
			}

			logger.Log(LogLevel.Important, "\nFinished");

			return OnExit(success ? 0 : 1);
        }

		private static bool TryCreateLoggger([NotNullWhen(true)] out Logger? logger)
		{
			logger = null;

			try
			{
				logger = ConsoleLogger.Create(Encoding.UTF8);
				return true;
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine($"Failed to create UTF8 logger. Error: [{ex.GetType().FullName}] {ex.Message}");
			}

			if (logger is null)
			{
				try
				{
					logger = new ConsoleLogger();
					return true;
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine($"Failed to create default logger. Error: [{ex.GetType().FullName}] {ex.Message}");
				}
			}

			return false;
		}

		private static int OnExit(int code)
		{
			if (System.Diagnostics.Debugger.IsAttached)
			{
				Console.ReadKey(true);
			}
			return code;
		}
	}
}
