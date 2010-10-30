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
			var hashKey = GetChunkHash(x, y);
			ActiveChunk chunk = chunks[hashKey] as ActiveChunk;

			if (chunk == null && load) {
				var data = cache.Get(x, y);
				chunk = new ActiveChunk(world, data, x, y);
				chunk.Initialize();
				chunks.Add(hashKey, chunk);
			}

			return chunk;
		}

		public bool IsChunkLoaded(int x, int y) {
			return chunks.ContainsKey(GetChunkHash(x, y));
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

		protected ulong GetChunkHash(int x, int y) {
			return (uint)x | (ulong)(uint)y << 32;
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

		public void ScheduleUpdate(int delay, int x, int y, int z) {
			var chunk = GetChunk(x >> 4, z >> 4, false);
			if (chunk != null) {
				chunk.ScheduleUpdate(delay, x & 0xF, y, z & 0xF);
			}
		}
	}
}
