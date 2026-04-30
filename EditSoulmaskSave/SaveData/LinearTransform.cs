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

using UeSaveGame.DataTypes;

namespace EditSoulmaskSave.SaveData
{
	/// <summary>
	/// Represents a linear transform as it appears in a Soulmask save file
	/// </summary>
	internal struct LinearTransform
	{
		public FVector Translation;

		public FVector Rotation;

		public FVector Scale;

		public static bool TryParse(string value, out LinearTransform result)
		{
			// String format from save file
			// 156800.000000,-188800.000000,32000.000000|0.000000,0.000000,0.000000|1.000000,1.000000,1.000000

			string[] parts = value.Split('|');
			if (parts.Length != 3)
			{
				result = default;
				return false;
			}

			FVector[] vectors = new FVector[3];
			for (int i = 0; i < 3; ++i)
			{
				string[] vectorParts = parts[i].Split(',');
				if (vectorParts.Length != 3)
				{
					result = default;
					return false;
				}

				float[] components = new float[3];
				for (int c = 0; c < 3; ++c)
				{
					if (!float.TryParse(vectorParts[c], out components[c]))
					{
						result = default;
						return false;
					}
				}

				vectors[i] = new()
				{
					X = components[0],
					Y = components[1],
					Z = components[2]
				};
			}

			result = new()
			{
				Translation = vectors[0],
				Rotation = vectors[1],
				Scale = vectors[2]
			};
			return true;
		}

		public override string ToString()
		{
			return $"{VectorToString(Translation)}|{VectorToString(Rotation)}|{VectorToString(Scale)}";
		}

		private static string VectorToString(FVector v)
		{
			// String format for save file
			return $"{v.X:0.000000},{v.Y:0.000000},{v.Z:0.000000}";
		}
	}
}
