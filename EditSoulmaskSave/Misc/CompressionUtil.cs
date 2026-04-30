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

using K4os.Compression.LZ4;

namespace EditSoulmaskSave.Misc
{
	/// <summary>
	/// Helpers for compression and decompressing data
	/// </summary>
	internal static class CompressionUtil
	{
		/// <summary>
		/// Decompress LZ4 compressed data
		/// </summary>
		/// <param name="input">The data to decompress</param>
		/// <param name="decompressedSize">The expected size of the decompressed data</param>
		/// <param name="output">Where to write the decompressed data</param>
		/// <exception cref="ArgumentException">The decompressed size is too small</exception>
		public static void LZ4Decompress(Stream input, int decompressedSize, Stream output)
		{
			byte[] inData = new byte[input.Length - input.Position];
			input.Read(inData, 0, inData.Length);

			byte[] outData = new byte[decompressedSize];

			int result = LZ4Codec.Decode(inData, 0, inData.Length, outData, 0, outData.Length);
			if (result < 0)
			{
				throw new ArgumentException("Output buffer too small");
			}

			output.Write(outData, 0, outData.Length);
		}

		/// <summary>
		/// Compress data using LZ4
		/// </summary>
		/// <param name="input">The data to compress</param>
		/// <param name="output">Where to write compressed data</param>
		/// <returns>The size of the decompressed data</returns>
		/// <exception cref="InvalidOperationException">Unable to write all data to the output stream</exception>
		public static int LZ4Compress(Stream input, Stream output)
		{
			int length = (int)(input.Length - input.Position);

			byte[] inData = new byte[length];
			input.Read(inData, 0, length);

			byte[] outData = new byte[length];

			int result = LZ4Codec.Encode(inData, 0, inData.Length, outData, 0, outData.Length);
			if (result < 0)
			{
				throw new InvalidOperationException("Output buffer too small");
			}

			output.Write(outData, 0, result);

			return length;
		}
	}
}
