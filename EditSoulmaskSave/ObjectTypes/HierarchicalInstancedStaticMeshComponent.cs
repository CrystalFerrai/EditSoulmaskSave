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

using EditSoulmaskSave.Misc;
using System.Numerics;
using UeSaveGame.DataTypes;

namespace EditSoulmaskSave.ObjectTypes
{
	/// <summary>
	/// Custom game object data for instances of HierarchicalInstancedStaticMeshComponent
	/// </summary>
	[GameObject("/Script/WS.HJianZhuInstComponent")]
	internal class HierarchicalInstancedStaticMeshComponent : IGameObject
	{
		public List<Matrix4x4> PerInstanceSMData;

		public List<float> PerInstanceSMCustomData;

		public List<FClusterNode> ClusterTreePtr;

		public HierarchicalInstancedStaticMeshComponent()
		{
			PerInstanceSMData = new();
			PerInstanceSMCustomData = new();
			ClusterTreePtr = new();
		}

		public void Deserialize(BinaryReader reader)
		{
			int unknown1 = reader.ReadInt32();
			if (unknown1 != 0) throw new NotImplementedException();

			int unknown2 = reader.ReadInt32();
			if (unknown2 != 0) throw new NotImplementedException();

			int size, count;

			size = reader.ReadInt32();
			if (size != 64) throw new NotSupportedException("Unexpected size for PerInstanceSMData");

			count = reader.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				PerInstanceSMData.Add(reader.ReadMatrix());
			}

			size = reader.ReadInt32();
			if (size != 4) throw new NotSupportedException("Unexpected size for PerInstanceSMCustomData");

			count = reader.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				PerInstanceSMCustomData.Add(reader.ReadSingle());
			}

			size = reader.ReadInt32();
			if (size != 64) throw new NotSupportedException("Unexpected size for ClusterTreePtr");

			count = reader.ReadInt32();
			for (int i = 0; i < count; ++i)
			{
				ClusterTreePtr.Add(FClusterNode.Deserialize(reader));
			}
		}

		public int Serialize(BinaryWriter writer)
		{
			writer.Write(0);
			writer.Write(0);

			writer.Write(64);
			writer.Write(PerInstanceSMData.Count);
			foreach (Matrix4x4 value in PerInstanceSMData)
			{
				writer.WriteMatrix(value);
			}

			writer.Write(4);
			writer.Write(PerInstanceSMCustomData.Count);
			foreach (float value in PerInstanceSMCustomData)
			{
				writer.Write(value);
			}

			writer.Write(64);
			writer.Write(ClusterTreePtr.Count);
			foreach (FClusterNode value in ClusterTreePtr)
			{
				value.Serialize(writer);
			}

			return 8 +
				8 + PerInstanceSMData.Count * 64 +
				8 + PerInstanceSMCustomData.Count * 4 +
				8 + ClusterTreePtr.Count * 64;
		}
	}

	internal class FClusterNode
	{
		public FVector BoundMin { get; set; }
		public int FirstChild { get; set; }
		public FVector BoundMax { get; set; }
		public int LastChild { get; set; }
		public int FirstInstance { get; set; }
		public int LastInstance { get; set; }
		public FVector MinInstanceScale { get; set; }
		public FVector MaxInstanceScale { get; set; }

		public static FClusterNode Deserialize(BinaryReader reader)
		{
			FClusterNode instance = new();

			instance.BoundMin = reader.ReadVector();
			instance.FirstChild = reader.ReadInt32();
			instance.BoundMax = reader.ReadVector();
			instance.LastChild = reader.ReadInt32();
			instance.FirstInstance = reader.ReadInt32();
			instance.LastInstance = reader.ReadInt32();
			instance.MinInstanceScale = reader.ReadVector();
			instance.MaxInstanceScale = reader.ReadVector();

			return instance;
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.WriteVector(BoundMin);
			writer.Write(FirstChild);
			writer.WriteVector(BoundMax);
			writer.Write(LastChild);
			writer.Write(FirstInstance);
			writer.Write(LastInstance);
			writer.WriteVector(MinInstanceScale);
			writer.WriteVector(MaxInstanceScale);
		}

		public override string ToString()
		{
			return $"Min: {BoundMin}, Max: {BoundMax}";
		}
	}
}
