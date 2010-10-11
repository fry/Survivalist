using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class ChunkPosition {
		public int X;
		public int Y;

		public ChunkPosition(int x, int y) {
			X = x;
			Y = y;
		}

		public override int GetHashCode() {
			return X << 8 | Y;
		}

		public override bool Equals(object obj) {
			if (obj is ChunkPosition) {
				var myobj = obj as ChunkPosition;
				return X == myobj.X && Y == myobj.Y;
			}

			return false;
		}

		public double Distance(Entity entity) {
			double xdiff = entity.X - X*16;
			double ydiff = entity.Z - Y*16;
			return xdiff * xdiff + ydiff * ydiff;
		}
	}

	public class LoadedChunk {
		HashSet<Player> players = new HashSet<Player>();
		ChunkData data;
		ChunkCache cache;
		public ChunkPosition Position;

		public LoadedChunk(ChunkCache cache, int x, int y) {
			Position = new ChunkPosition(x, y);
			this.cache = cache;
			data = cache.Get(x, y);
		}

		public void Broadcast(Packet packet) {
			foreach (var player in players) {
				player.Client.SendPacket(packet);
			}
		}

		public bool IsEmpty {
			get {
				return players.Count == 0;
			}
		}

		public void AddPlayer(Player player) {
			if (players.Add(player)) {
				player.Client.SendPacket(new AddToChunkPacket(Position.X, Position.Y, true));
				player.AddChunkToSend(Position);
			}
		}

		public void RemovePlayer(Player player) {
			if (players.Remove(player)) {
				player.Client.SendPacket(new AddToChunkPacket(Position.X, Position.Y, false));
			} else {
				Console.WriteLine("Tried to remove a player from a chunk it wasn't in!");
			}
		}

		public void RemoveSelf() {
			cache.Unload(Position.X, Position.Y);
		}

		public void UpdatePlayers() {
			
			// TODO: send changes done to the chunk
		}
	}
}
