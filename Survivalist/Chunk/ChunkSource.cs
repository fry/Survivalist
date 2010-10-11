using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public interface ChunkSource {
		ChunkData Load(int x, int y);
		void Save(int x, int y, ChunkData chunk);
	}
}
