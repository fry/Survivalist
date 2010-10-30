using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class BlockData {
		public byte[] Data;

		public BlockData(int size) {
			Data = new byte[size / 2];
		}

		public static int GetIndex(int x, int y, int z) {
			return x << 11 | z << 7 | y;
		}

		public byte GetValue(int x, int y, int z) {
			int index = GetIndex(x, y, z);
			int half = index & 1;
			index = index / 2;

			var entry = Data[index];
			if (half == 0)
				return (byte)(entry & 0xF);
			else
				return (byte)((entry >> 4) & 0xF);
		}

		public void SetValue(int x, int y, int z, int value) {
			int index = GetIndex(x, y, z);
			int half = index & 1;
			index = index / 2;

			if (half == 0)
				Data[index] = (byte)((Data[index] & 0xF0) | (value & 0xF));
			else
				Data[index] = (byte)((Data[index] & 0xF) | ((value & 0xF) << 4));
		}

		public int Write(byte[] dest, int xstart, int ystart, int zstart, int xend, int yend, int zend, int start) {
			for (int x = xstart; x < xend; x++) {
				for (int z = zstart; z < zend; z++) {
					int index = GetIndex(x, ystart, z) / 2;
					int size = (yend - ystart) / 2;
					Array.Copy(Data, index, dest, start, size);
					start += size;
				}
			}

			return start;
		}
	}

	public class ChunkData {
		public byte[] Blocks;
		public BlockData MetaData;
		public BlockData SkyLight;
		public BlockData BlockLight; 

		public byte[] Heightmap = new byte[16*16];

		public int X { get; protected set; }
		public int Y { get; protected set; }

		public ChunkData(int chunkX, int chunkY, byte[] blocks) {
			X = chunkX;
			Y = chunkY;
			Blocks = blocks;
			MetaData = new BlockData(blocks.Length);
			SkyLight = new BlockData(blocks.Length);
			BlockLight = new BlockData(blocks.Length);
		}

		public bool RecievesSkyLight(int x, int y, int z) {
			return y >= GetHeight(x, z);
		}

		public void SetBlock(int x, int y, int z, byte type) {
			var index = BlockData.GetIndex(x, y, z);
			Blocks[index] = type;
		}
		 
		public byte GetBlock(int x, int y, int z) {
			return Blocks[BlockData.GetIndex(x, y, z)];
		}

		public int GetLight(LightType type, int x, int y, int z) {
			if (type == LightType.Block)
				return BlockLight.GetValue(x, y, z);
			else if (type == LightType.Sky)
				return SkyLight.GetValue(x, y, z);
			return 0;
		}

		public void SetLight(LightType type, int x, int y, int z, int value) {
			if (type == LightType.Block)
				BlockLight.SetValue(x, y, z, value);
			else if (type == LightType.Sky)
				SkyLight.SetValue(x, y, z, value);
		}

		public int GetHeight(int x, int z) {
			return Heightmap[z << 4 | x];
		}

		public void SetHeight(int x, int z, int height) {
			Heightmap[z << 4 | x] = (byte)height;
		}

		public int Write(byte[] dest, int xstart, int ystart, int zstart, int xend, int yend, int zend, int start) {
			for (int x = xstart; x < xend; x++) {
				for (int z = zstart; z < zend; z++) {
					int index = BlockData.GetIndex(x, ystart, z);
					int size = yend - ystart;
					Array.Copy(Blocks, index, dest, start, size);
					start += size;
				}
			}

			start = MetaData.Write(dest, xstart, ystart, zstart, xend, yend, zend, start);
			start = BlockLight.Write(dest, xstart, ystart, zstart, xend, yend, zend, start);
			start = SkyLight.Write(dest, xstart, ystart, zstart, xend, yend, zend, start);
			return start;
		}
	}
}
