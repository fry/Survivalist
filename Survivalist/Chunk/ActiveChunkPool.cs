using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using Wintellect.PowerCollections;

namespace Survivalist {
	/* Contains active chunks on the server
	 * Deals with sending out chunk updates and adding and removing players */
	public class ActiveChunkPool {
		Hashtable chunks = new Hashtable();
		ChunkCache cache;
		// What world this pool serves
		World world;

		public ActiveChunkPool(World world, ChunkCache cache) {
			this.cache = cache;
			this.world = world;
		}

		public ActiveChunk GetChunk(int x, int y, bool load = true) {
			ulong hashKey = (uint)x | (ulong)(uint)y << 32;
			ActiveChunk chunk = chunks[hashKey] as ActiveChunk;

			if (chunk == null && load) {
				var data = cache.Get(x, y);
				chunk = new ActiveChunk(world, data, x, y);
				InitializeChunk(data, x, y);
				chunks.Add(hashKey, chunk);
			}

			return chunk;
		}

		public void SaveAll() {
			Console.WriteLine("[ActiveChunkPool] Saving all chunks");
			foreach (DictionaryEntry entry in chunks) {
				var chunk = entry.Value as ActiveChunk;
				cache.Save(chunk.Position.X, chunk.Position.Y);
			}
			Console.WriteLine("[ActiveChunkPool] .. done");
		}

		public void OnTick() {
			ProcessChunkUpdates();
		}

		public void OnTileChanged(int? oldType, int x, int y, int z) {
			var chunk = GetChunk(x >> 4, z >> 4, false);
			if (chunk != null)
				chunk.OnTileChanged(oldType, x & 0xF, y, z & 0xF);
		}

		protected void ProcessChunkUpdates() {
			foreach (DictionaryEntry entry in chunks) {
				var chunk = entry.Value as ActiveChunk;
				if (!chunk.IsEmpty) {
					chunk.SendUpdates();
				}
				chunk.ProcessUpdates();
			}
		}

		// Invokes the OnCreated event on every dynamic block in this chunk
		protected void InitializeChunk(ChunkData data, int chunkX, int chunkY) {
			int tileX = chunkX * 16;
			int tileZ = chunkY * 16;
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 128; y++) {
					for (int z = 0; z < 16; z++) {
						var type = data.GetBlock(x, y, z);
						if (Block.DynamicBlocks[type]) {
							Block.Blocks[type].OnCreated(world, x, y, z);
						}
					}
				}
			}
		}

		public void ScheduleUpdate(int delay, int x, int y, int z) {
			var chunk = GetChunk(x >> 4, z >> 4, false);
			if (chunk != null) {
				chunk.ScheduleUpdate(delay, x & 0xF, y, z & 0xF);
			}
		}
	}
}
