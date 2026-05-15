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
using EditSoulmaskSave.Actions.Standard;
using EditSoulmaskSave.Actions.Test;
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
		/// List of actions the program should perform
		/// </summary>
		public IReadOnlyList<ProgramAction> Actions { get; private set; }

		private ProgramOptions()
		{
			Actions = null!;
		}

		public static bool TryParseCommandLine(string[] args, Logger logger, [NotNullWhen(true)] out ProgramOptions? result)
		{
			result = null;
			if (args.Length == 0)
			{
				return false;
			}

			List<ProgramAction> actions = new();
			ProgramOptions instance = new() { Actions = actions };

			for (int i = 0; i < args.Length; ++i)
			{
				if (args[i].StartsWith("--"))
				{
					if (TryParseAction(args, ref i, logger, out ProgramAction? action))
					{
						actions.Add(action);
					}
					else
					{
						return false;
					}
				}
				else
				{
					// Standalone positional args are not supported by this application
					logger.Error($"Unrecognized argument '{args[i]}'");
					return false;
				}
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

			logger.Log(logLevel, $"{indent}Performs actions on or related to Soulmask save files.");
			logger.Log(logLevel, $"{indent}Usage: {programName} [action [options]] [[additional actions]]");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}Notes");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  1. One or more actions must be specified as parameters. Actions will be run");
			logger.Log(logLevel, $"{indent}     in the order they are specified.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  2. Actions requiring a [save] paramter should be given a path to a Soulmask");
			logger.Log(logLevel, $"{indent}     save file such as world.db, account.db, etc.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  3. Actions requiring [in] or [out] paramters should be given paths to files");
			logger.Log(logLevel, $"{indent}     or directories as indicated by the action.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}Actions");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --list-players [save]          Prints details about player accounts.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --export-players [save] [out]  Exports all player state actors as json to the");
			logger.Log(logLevel, $"{indent}                                 specified directory.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --export-all [save] [out]      Exports all actors as json to the specified");
			logger.Log(logLevel, $"{indent}                                 directory.");
			//logger.LogEmptyLine(logLevel);
			//logger.Log(logLevel, $"{indent}  --import [save] [in]           Imports previously exported actors from the");
			//logger.Log(logLevel, $"{indent}                                 specified directory.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --fix-double-compress [save]   Attempts to fix player accounts that have");
			logger.Log(logLevel, $"{indent}                                 double compressed actor data due to connecting");
			logger.Log(logLevel, $"{indent}                                 to a cluster with mismatched server versions.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}Debug/Test Actions");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --dump-player-blobs [save] [out]  Decompresses and dumps all player state");
			logger.Log(logLevel, $"{indent}                                    actor blobs as raw binary to the specified");
			logger.Log(logLevel, $"{indent}                                    directory.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --dump-all-blobs [save] [out]     Decompresses and dumps all actor blobs as");
			logger.Log(logLevel, $"{indent}                                    raw binary to the specified directory.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --blobs-to-json [in] [out]        Convert a directory of binary actor blobs");
			logger.Log(logLevel, $"{indent}                                    to json.");
			logger.LogEmptyLine(logLevel);
			logger.Log(logLevel, $"{indent}  --json-to-blobs [in] [out]        Convert a directory of json actor blobs to");
			logger.Log(logLevel, $"{indent}                                    binary.");
		}

		private static bool TryParseAction(string[] args, ref int argIndex, Logger logger, [NotNullWhen(true)] out ProgramAction? action)
		{
			action = null;

			string argValue = args[argIndex][2..];
			switch (argValue)
			{
				case "list-players":
					{
						if (ParseArguments(args, ref argIndex, out string? param1))
						{
							action = new ListPlayersProgramAction(param1);
						}
						break;
					}
				case "export-players":
					{
						if (ParseArguments(args, ref argIndex, out string? param1, out string? param2))
						{
							action = new ExportPlayersProgramAction(param1, param2);
						}
						break;
					}
				case "export-all":
					{
						if (ParseArguments(args, ref argIndex, out string? param1, out string? param2))
						{
							action = new ExportAllProgramAction(param1, param2);
						}
						break;
					}
				case "fix-double-compress":
					{
						if (ParseArguments(args, ref argIndex, out string? param1))
						{
							action = new FixDoubleCompressProgramAction(param1);
						}
						break;
					}
				case "dump-player-blobs":
					{
						if (ParseArguments(args, ref argIndex, out string? param1, out string? param2))
						{
							action = new DumpPlayerBlobsProgramAction(param1, param2);
						}
						break;
					}
				case "dump-all-blobs":
					{
						if (ParseArguments(args, ref argIndex, out string? param1, out string? param2))
						{
							action = new DumpAllBlobsProgramAction(param1, param2);
						}
						break;
					}
				case "blobs-to-json":
					{
						if (ParseArguments(args, ref argIndex, out string? param1, out string? param2))
						{
							action = new BlobsToJsonProgramAction(param1, param2);
						}
						break;
					}
				case "json-to-blobs":
					{
						if (ParseArguments(args, ref argIndex, out string? param1, out string? param2))
						{
							action = new JsonToBlobsProgramAction(param1, param2);
						}
						break;
					}
				default:
					logger.Error($"Unrecognized argument '{args[argIndex]}'");
					return false;
			}

			if (action is null)
			{
				logger.Error($"Failed to parse parameters for argument '{args[argIndex]}'");
				return false;
			}

			return true;
		}

		private static bool ParseArguments(string[] args, ref int argIndex, [NotNullWhen(true)] out string? param1)
		{
			if (argIndex < args.Length - 1 && !args[argIndex + 1].StartsWith("--"))
			{
				++argIndex;
				param1 = args[argIndex];
				return true;
			}

			param1 = null;
			return false;
		}

		private static bool ParseArguments(string[] args, ref int argIndex, [NotNullWhen(true)] out string? param1, [NotNullWhen(true)] out string? param2)
		{
			param1 = null;
			param2 = null;
			if (argIndex + 2 >= args.Length)
			{
				return false;
			}

			if (args[argIndex + 1].StartsWith("--") || args[argIndex + 2].StartsWith("--"))
			{
				return false;
			}

			param1 = args[argIndex + 1];
			param2 = args[argIndex + 2];
			argIndex += 2;

			return true;
		}
	}
}
