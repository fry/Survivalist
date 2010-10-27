using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class World {
		public EntityHandler EntityHandler;
		public ChunkCache ChunkCache;
		public ActiveChunkPool ChunkPool;
		public LightingEngine Lighting;
		public double TimeFactor;

		double time;
		public double Time {
			get {
				return time;
			}
			set {
				time = value % 24000;
			}
		}

		/// <summary>
		/// Update time on server and all clients
		/// </summary>
		public void UpdateTime(int time) {
			Time = time;
			EntityHandler.Broadcast(new UpdateTimePacket(time));
		}

		public World() {
			TimeFactor = 0.02;
			ChunkCache = new ChunkCache(new NBTChunkSource(@"..\..\..\World2"));
			ChunkPool = new ActiveChunkPool(this, ChunkCache);
			EntityHandler = new EntityHandler(this);
			Lighting = new LightingEngine(this);
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

		public int GetLight(LightType type, int x, int y, int z) {
			if (!IsValid(x, y, z))
				return 0;

			int chunkX = x >> 4;
			int chunkY = z >> 4;
			if (!IsChunkLoaded(chunkX, chunkY))
				return 0;

			var chunk = GetChunk(chunkX, chunkY);
			return chunk.GetLight(type, x & 0xF, y, z & 0xF);
		}

		public void SetLight(LightType type, int x, int y, int z, int value) {
			if (!IsValid(x, y, z))
				return;

			int chunkX = x >> 4;
			int chunkY = z >> 4;
			if (!IsChunkLoaded(chunkX, chunkY))
				return;

			var chunk = GetChunk(chunkX, chunkY);
			chunk.SetLight(type, x & 0xF, y, z & 0xF, value);
		}

		public void SetBlockType(int x, int y, int z, int typeID, bool invokeEvents = true) {
			if (!IsValid(x, y, z))
				return;
			var chunk = GetChunk(x >> 4, z >> 4);
			int tileX = x & 0xF;
			int tileZ = z & 0xF;
			int oldTypeId = chunk.GetBlock(tileX, y, tileZ);
			// Send Destroyed event
			if (invokeEvents) {
				var block = Block.Blocks[oldTypeId];
				if (block != null) {
					block.OnDestroyed(this, x, y, z);
				}
			}

			chunk.MetaData.SetValue(tileX, y, tileZ, 0);
			chunk.SetBlock(tileX, y, tileZ, (byte)typeID);
			ChunkPool.OnTileChanged(oldTypeId, x, y, z);
			Lighting.OnTileChanged(x, y, z);

			// Send Placed event
			if (invokeEvents) {
				var block = Block.Blocks[typeID];
				if (block != null)
					block.OnPlaced(this, x, y, z);
			}
		}

		public void SetBlockData(int x, int y, int z, int metaData) {
			if (!IsValid(x, y, z))
				return;
			var chunk = GetChunk(x >> 4, z >> 4);
			int tileX = x & 0xF;
			int tileZ = z & 0xF;

			chunk.MetaData.SetValue(tileX, y, tileZ, metaData);
			ChunkPool.OnTileChanged(null, x, y, z);
		}

		public void SetBlock(int x, int y, int z, int typeID, int metaData, bool invokeEvents = true) {
			if (!IsValid(x, y, z))
				return;
			var chunk = GetChunk(x >> 4, z >> 4);
			int tileX = x & 0xF;
			int tileZ = z & 0xF;
			int oldTypeId = chunk.GetBlock(tileX, y, tileZ);
			// Send Destroyed event
			if (invokeEvents) {
				var block = Block.Blocks[oldTypeId];
				if (block != null) {
					block.OnDestroyed(this, x, y, z);
				}
			}
			chunk.MetaData.SetValue(tileX, y, tileZ, metaData);
			chunk.SetBlock(tileX, y, tileZ, (byte)typeID);
			ChunkPool.OnTileChanged(oldTypeId, x, y, z);
			Lighting.OnTileChanged(x, y, z);

			// Send Placed event
			if (invokeEvents) {
				var block = Block.Blocks[typeID];
				if (block != null)
					block.OnPlaced(this, x, y, z);
			}
		}

		public int GetHeight(int x, int z) {
			if (!IsValid(x, 0, z))
				return 0;

			var chunk = GetChunk(x >> 4, z >> 4);
			return chunk.GetHeight(x & 0xF, z & 0xF);
		}

		public void OnTick(int delta) {
			// Update and broadcast time
			Time += delta * TimeFactor;
			if (Time % 20 == 0)
				EntityHandler.Broadcast(new UpdateTimePacket((int)Time));

			EntityHandler.Tick();
			ChunkPool.OnTick();
			Lighting.RunUpdates();
		}

		public bool IsValid(int x, int y, int z) {
			return Math.Abs(x) <= 32000000 && Math.Abs(z) <= 32000000 && y >= 0 && y < 128;
		}

		public bool IsLoaded(int x, int y) {
			return ChunkPool.IsChunkLoaded(x >> 4, y >> 4);
		}

		public bool IsLoaded(int x, int y, int z) {
			return y >= 0 && y < 128 && IsLoaded(x, z);
		}

		public bool IsChunkLoaded(int x, int y) {
			return ChunkPool.IsChunkLoaded(x, y);
		}

		public bool RecievesSkyLight(int x, int y, int z) {
			if (!IsValid(x, y, z))
				return false;

			int chunkX = x >> 4;
			int chunkY = z >> 4;

			var chunk = GetChunk(chunkX, chunkY);
			if (chunk == null)
				return false;

			return chunk.RecievesSkyLight(x & 0xF, y, z & 0xF);
		}
	}
}
