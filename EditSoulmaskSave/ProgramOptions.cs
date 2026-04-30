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
using System.Reflection;

namespace EditSoulmaskSave
{
	/// <summary>
	/// General configuration information the program needs to run
	/// </summary>
	internal class ProgramOptions
	{
		/// <summary>
		/// The location of the save file
		/// </summary>
		public string SavePath { get; private set; }

		/// <summary>
		/// List of actions the program should perform
		/// </summary>
		public IReadOnlyList<ProgramAction> Actions { get; private set; }

		private ProgramOptions()
		{
			SavePath = null!;
			Actions = null!;
		}

		public static bool TryParseCommandLine(string[] args, Logger logger, [NotNullWhen(true)] out ProgramOptions? result)
		{
			if (args.Length == 0)
			{
				result = null;
				return false;
			}

			List<ProgramAction> actions = new();
			ProgramOptions instance = new() { Actions = actions };

			int positionalArgIndex = 0;

			for (int i = 0; i < args.Length; ++i)
			{
				if (args[i].StartsWith("--"))
				{
					// Explicit arg
					string argValue = args[i][2..];
					switch (argValue)
					{
						case "list-players":
							actions.Add(new(ProgramActionType.ListPlayers));
							break;
						case "dump-players":
							if (i < args.Length - 1 && !args[i + 1].StartsWith("--"))
							{
								actions.Add(new(ProgramActionType.DumpPlayers, Path.GetFullPath(args[i + 1])));
								++i;
							}
							else
							{
								logger.Error("Missing parameter for --dump-players argument");
								result = null;
								return false;
							}
							break;
						case "dump-player-blobs":
							if (i < args.Length - 1 && !args[i + 1].StartsWith("--"))
							{
								actions.Add(new(ProgramActionType.DumpPlayerBlobs, Path.GetFullPath(args[i + 1])));
								++i;
							}
							else
							{
								logger.Error("Missing parameter for --dump-player-blobs argument");
								result = null;
								return false;
							}
							break;
						case "fix-double-compress":
							actions.Add(new(ProgramActionType.FixDoubleCompress));
							break;
						default:
							logger.Error($"Unrecognized argument '{args[i]}'");
							result = null;
							return false;
					}
				}
				else
				{
					// Positional arg
					switch (positionalArgIndex)
					{
						case 0:
							instance.SavePath = Path.GetFullPath(args[i]);
							break;
						default:
							logger.Error("Too many positional arguments.");
							result = null;
							return false;
					}
					++positionalArgIndex;
				}
			}

			if (positionalArgIndex < 1)
			{
				logger.Error($"Not enough positional arguments");
				result = null;
				return false;
			}

			if (!File.Exists(instance.SavePath))
			{
				logger.Error($"The specified save file path \"{instance.SavePath}\" does not exist or is inaccessible");
				result = null;
				return false;
			}

			if (instance.Actions.Count == 0)
			{
				logger.Error($"Must specify at least one action to perform");
				result = null;
				return false;
			}

			result = instance;
			return true;
		}

		/// <summary>
		/// Prints how to use the program, including all possible command line arguments
		/// </summary>
		/// <param name="logger">Where the message will be printed</param>
		/// <param name="logLevel">The log level for the message</param>
		/// <param name="indent">Every line of the output will be prefixed with this</param>
		public static void PrintUsage(Logger logger, LogLevel logLevel, string indent = "")
		{
			string programName = Assembly.GetExecutingAssembly().GetName().Name ?? "EditSoulmaskSave";

			logger.Log(logLevel, $"{indent}Usage: {programName} [save file path] [action [options]]");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  [save file path]  Path to a save file (world.db, account.db, etc.)");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}Actions");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --list-players             Prints details about player accounts.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --dump-players [dir]       Dumps all player state actors as json to the");
			logger.Log(logLevel, $"{indent}                             specified directory.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --dump-player-blobs [dir]  Decompresses and dumps all player state actor");
			logger.Log(logLevel, $"{indent}                             blobs as raw binary to the specified directory.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --fix-double-compress      Attempts to fix player accounts that have double");
			logger.Log(logLevel, $"{indent}                             compressed actor data due to connecting to a");
			logger.Log(logLevel, $"{indent}                             cluster with mismatched server versions.");
		}
	}

	internal class ProgramAction
	{
		public ProgramActionType ActionType { get; }

		public string? Parameter { get; }

		public ProgramAction(ProgramActionType actionType, string? parameter = null)
		{
			ActionType = actionType;
			Parameter = parameter;
		}
	}

	internal enum ProgramActionType
	{
		None,
		ListPlayers,
		DumpPlayers,
		DumpPlayerBlobs,
		FixDoubleCompress
	}
}
