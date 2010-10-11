using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class Player: NamedEntity {
		World world;
		public LoginHandler Client { get; protected set; }
		protected HashSet<ChunkPosition> chunksToSend = new HashSet<ChunkPosition>();

		public Player(LoginHandler client, World world, string name): base(name) {
			Client = client;
			this.world = world;
		}

		public void AddChunkToSend(ChunkPosition position) {
			chunksToSend.Add(position);
		}

		public void SendChunks() {
			if (chunksToSend.Count > 0) {
				var closest = chunksToSend.OrderBy(c => c.Distance(this)).FirstOrDefault();
				if (closest != null) {
					//Console.WriteLine("Sending chunk {0}, {1} dist {2}, player {3}, {4}", closest.X, closest.Y, closest.Distance(this), (int)X, (int)Y);
					chunksToSend.Remove(closest);
					var chunk = world.ChunkCache.Get(closest.X, closest.Y);
					Client.SendPacket(new UpdateFullChunkPacket(closest.X*16, 0, closest.Y*16, 16, 128, 16, chunk));
				}
			}
		}
	}
}
