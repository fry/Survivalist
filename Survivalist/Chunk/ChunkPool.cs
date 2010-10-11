﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Survivalist {
	public class ChunkPool {
		Hashtable chunks = new Hashtable();
		ChunkCache cache;

		public ChunkPool(ChunkCache cache) {
			this.cache = cache;
		}

		public LoadedChunk GetChunk(int x, int y, bool load = true) {
			ulong hashKey = (uint)x | (ulong)(uint)y << 32;
			LoadedChunk chunk = chunks[hashKey] as LoadedChunk;

			if (chunk == null && load) {
				chunk = new LoadedChunk(cache, x, y);
				chunks.Add(hashKey, chunk);
			}

			return chunk;
		}

		public void AddPlayer(Player player) {
			int chunkX = (int)player.X >> 4;
			int chunky = (int)player.Z >> 4;
			for (int x = chunkX - EntityTracker.ChunkRange; x < chunkX + EntityTracker.ChunkRange; x++) {
				for (int y = chunkX - EntityTracker.ChunkRange; y < chunkX + EntityTracker.ChunkRange; y++) {
					GetChunk(x, y).AddPlayer(player);
				}
			}
		}

		public void Tick() {

		}
	}
}