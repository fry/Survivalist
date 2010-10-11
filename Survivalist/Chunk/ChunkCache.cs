using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Survivalist {
	/* The ChunkCache should deal with what chunks to keep in memory and saving of
	 * unloaded chunks */
	public class ChunkCache {
		ChunkSource source;
		Hashtable cache = new Hashtable();

		public ChunkCache(ChunkSource source) {
			this.source = source;
		}

		public ChunkData Get(int x, int y) {
			ulong hashKey = (uint)x | (ulong)(uint)y << 32;
			ChunkData chunk = cache[hashKey] as ChunkData;
			if (chunk == null) {
				Console.WriteLine("[ChunkCache] Loading {0}, {1}", x, y);
				chunk = source.Load(x, y);
				cache[hashKey] = chunk;
			}

			return chunk;
		}

		public void Unload(int x, int y) {
			Console.WriteLine("[ChunkCache] Unloading {0}, {1}", x, y);

		}
		// GetTileType
		// SetTile
	}
}
