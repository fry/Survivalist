using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class World {
		public EntityHandler EntityHandler;
		public ChunkCache ChunkCache;
		public ChunkPool ChunkPool;

		public World() {
			ChunkCache = new ChunkCache(new RandomChunkSource());
			ChunkPool = new ChunkPool(ChunkCache);
			EntityHandler = new EntityHandler(this);
		}

		public ChunkData GetChunk(int x, int y) {
			return ChunkCache.Get(x, y);
		}

		public int GetBlockType(int x, int y, int z) {
			if (!IsValid(x, y, z))
				return 0;
			return GetChunk(x >> 4, z >> 4).GetBlock(x & 0xF, y, z & 0xF);
		}

		public int GetBlockData(int x, int y, int z) {
			return GetChunk(x >> 4, z >> 4).MetaData.GetValue(x & 0xF, y, z & 0xF);
		}

		public void SetBlockType(int x, int y, int z, int typeID, bool invokeEvents = true) {
			if (!IsValid(x, y, z))
				return;
			var chunk = GetChunk(x >> 4, z >> 4);
			int tileX = x & 0xF;
			int tileZ = z & 0xF;

			BlockUpdateEventArgs args = null;
			// Send Destroyed event
			if (invokeEvents) {
				args = new BlockUpdateEventArgs { World = this, X = x, Y = y, Z = z };
				int oldTypeId = chunk.GetBlock(tileX, y, tileZ);
				var block = Block.Blocks[oldTypeId];
				if (block != null)
					block.OnBlockDestroyed(args);
			}
			chunk.SetBlock(tileX, y, tileZ, (byte)typeID);

			// Send Created event(int)BlockType.Dirt
			if (invokeEvents) {
				var block = Block.Blocks[typeID];
				if (block != null)
					block.OnBlockCreated(args);
			}

			ChunkPool.GetChunk(x >> 4, z >> 4).Broadcast(new UpdateBlockPacket(this, x, y, z));
		}

		public void Tick() {
			EntityHandler.Tick();
		}

		public bool IsValid(int x, int y, int z) {
			return Math.Abs(x) <= 32000000 && Math.Abs(z) <= 32000000 && y >= 0 && y < 128;
		}
	}
}
