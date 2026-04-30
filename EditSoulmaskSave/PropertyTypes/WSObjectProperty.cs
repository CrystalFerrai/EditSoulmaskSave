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
using UeSaveGame.PropertyTypes;
using UeSaveGame.Util;

namespace SoulmaskSave.PropertyTypes
{
	/// <summary>
	/// Custom Soulmask save file version of ObjectProperty
	/// </summary>
	internal class WSObjectProperty : ObjectProperty
	{
		public WSObjectPropertyFlags ObjectFlags;

		public FString? ObjectPath;

		public List<FPropertyTag>? ObjectProperties;

		public WSObjectProperty(FString name)
			: base(name)
		{
		}

		protected override void DeserializeValue(BinaryReader reader, int size, PackageVersion packageVersion)
		{
			long flagPosition = reader.BaseStream.Position;

			ObjectFlags = (WSObjectPropertyFlags)reader.ReadByte();

			if (ObjectFlags == WSObjectPropertyFlags.None) return;

			if (ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference))
			{
				ObjectPath = reader.ReadUnrealString();
			}

			base.DeserializeValue(reader, size, packageVersion);

			if (ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference) && !ObjectFlags.HasFlag(WSObjectPropertyFlags.NoData))
			{
				ObjectProperties = new(PropertySerializationHelper.ReadProperties(reader, packageVersion, true));
			}

			ValidateObjectFlags(flagPosition);
		}

		protected override int SerializeValue(BinaryWriter writer, PackageVersion packageVersion)
		{
			long flagPosition = writer.BaseStream.Position;
			
			ValidateObjectFlags(flagPosition);

			writer.Write((byte)ObjectFlags);

			if (ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference))
			{
				writer.WriteUnrealString(ObjectPath);
			}

			base.SerializeValue(writer, packageVersion);

			if (ObjectFlags.HasFlag(WSObjectPropertyFlags.InstanceReference) && !ObjectFlags.HasFlag(WSObjectPropertyFlags.NoData))
			{
				if (ObjectProperties is null)
				{
					throw new InvalidOperationException($"ObjectProperties cannot be null when {nameof(ObjectFlags)} contains {WSObjectPropertyFlags.InstanceReference} flag");
				}
				PropertySerializationHelper.WriteProperties(ObjectProperties, writer, packageVersion, true);
			}

			return (int)(writer.BaseStream.Position - flagPosition);
		}

		private void ValidateObjectFlags(long flagPosition)
		{
			// Check if there are any flags we have not previously seen in data
			int remainingFlags = (byte)ObjectFlags & ~(byte)(
				WSObjectPropertyFlags.Unknown1 |
				WSObjectPropertyFlags.InstanceReference |
				WSObjectPropertyFlags.NoData |
				WSObjectPropertyFlags.Unknown8);
			if (remainingFlags != 0)
			{
				throw new NotImplementedException($"ObjectProperty unknown flags {(byte)ObjectFlags} at {flagPosition}");
			}
		}
	}

	[Flags]
	internal enum WSObjectPropertyFlags : byte
	{
		None = 0x00,

		Unknown1 = 0x01,

		// References a specific object instance and contains properties unless the NoData flag is also present.
		InstanceReference = 0x02,

		// Have only seen this when InstanceReference is also set. Indicates there are no properties stored.
		NoData = 0x04,

		Unknown8 = 0x08
	}
}
