using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public interface ChunkGenerator {
		ChunkData GenerateNewChunk(int x, int y);
	}
}
