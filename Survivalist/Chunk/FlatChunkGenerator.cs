using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class FlatChunkGenerator: ChunkGenerator {
		LightingEngine lighting;
		public FlatChunkGenerator(LightingEngine lighting) {
			this.lighting = lighting;
		}


		public ChunkData GenerateNewChunk(int x, int y) {
			var flatChunk = new ChunkData(x, y, new byte[16 * 16 * 128]);
			for (int tx = 0; tx < 16; tx++) {
				for (int tz = 0; tz < 16; tz++) {

					for (int ty = 0; ty < 5; ty++) {
						flatChunk.SetBlock(tx, ty, tz, (int)BlockType.Grass);
					}
					for (int ty = 5; ty < 128; ty++) {
						flatChunk.SkyLight.SetValue(tx, ty, tz, 15);
					}
				}
			}

			lighting.RecalculateLighting(flatChunk);

			return flatChunk;
		}
	}
}
