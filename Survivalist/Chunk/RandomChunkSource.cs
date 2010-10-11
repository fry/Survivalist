using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class RandomChunkSource: ChunkSource {
		int seed;

		public RandomChunkSource() {
			seed = (int)DateTime.Now.ToFileTime();
		}

		public RandomChunkSource(int seed) {
			this.seed = seed;
		}

		public ChunkData Load(int x, int y) {
			var rand = new Random(seed + x + y * 1024);
			var chunk = new ChunkData(new byte[16 * 16 * 128]);
			for (int tx = 0; tx < 16; tx++) {
				for (int tz = 0; tz < 16; tz++) {
					/*for (int ty = 0; ty < 1; ty++) {
						chunk.SetBlock(tx, ty, tz, 1);
					}
					for (int ty = 1; ty < 3; ty++) {
						if (rand.Next(10) <= 7) {
							chunk.SetBlock(tx, ty, tz, 10);
						} else {
							chunk.SetBlock(tx, ty, tz, 1);
						}
					}

					if (rand.Next(20) < 1) {
						for (int ty = 0; ty < 128; ty++) {
							chunk.SetBlock(tx, ty, tz, 8);
						}
					} else {
						for (int ty = 64; ty < 100; ty++) {
							chunk.SetBlock(tx, ty, tz, 8);
						}
					}

					for (int ty = 100; ty < 101; ty++) {
						if (rand.Next(10) <= 7)
							chunk.SetBlock(tx, ty, tz, 2);
					}

					for (int ty = 0; ty < 128; ty++) {
						var t = chunk.GetBlock(tx, ty, tz);
						if (t == 0 || t == 10 || t == 8)
							chunk.SkyLight.SetValue(tx, ty, tz, 15);
						if (t == 10)
							chunk.BlockLight.SetValue(tx, ty, tz, 15);
					}*/

					for (int ty = 0; ty < 5; ty++) {
						chunk.SetBlock(tx, ty, tz, (int)BlockType.Grass);
					}
					for (int ty = 5; ty < 128; ty++) {
						chunk.SkyLight.SetValue(tx, ty, tz, 15);
					}
				}
			}
			return chunk;
		}

		public void Save(int x, int y, ChunkData chunk) {
		}
	}
}
