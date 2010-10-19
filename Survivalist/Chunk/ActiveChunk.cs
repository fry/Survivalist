using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Wintellect.PowerCollections;

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

	public class ScheduledUpdate : IComparable<ScheduledUpdate> {
		public DateTime Time;
		public byte X;
		public byte Y;
		public byte Z;

		public int CompareTo(ScheduledUpdate other) {
			return Time.CompareTo(other.Time);
		}

		public override bool Equals(object obj) {
			if (obj is ScheduledUpdate) {
				var myobj = obj as ScheduledUpdate;
				return X == myobj.X && Y == myobj.Y && Z == myobj.Z;
			}
			return false;
		}

		public override int GetHashCode() {
			return X << 20 | Y << 10 | Z;
		}
	}

	public class ActiveChunk {
		HashSet<Player> players = new HashSet<Player>();
		OrderedBag<ScheduledUpdate> scheduledBlockUpdates = new OrderedBag<ScheduledUpdate>();

		ChunkData data;
		World world;
		public ChunkPosition Position { get; protected set; }
		public DateTime LastChanged { get; protected set; }
		public DateTime LastSent { get; protected set; }
		int tilesChanged = 0;
		int tileChangeX;
		int tileChangeY;
		int tileChangeZ;

		public ActiveChunk(World world, ChunkData data, int x, int y) {
			Position = new ChunkPosition(x, y);
			this.data = data;
			this.world = world;

			LastChanged = DateTime.MinValue;
			LastSent = DateTime.Now;
		}

		// Invokes the OnCreated event on every dynamic block in this chunk
		public void Initialize() {
			int tileX = Position.X * 16;
			int tileZ = Position.Y * 16;
			for (int x = 0; x < 16; x++) {
				for (int y = 0; y < 128; y++) {
					for (int z = 0; z < 16; z++) {
						var type = data.GetBlock(x, y, z);
						var block = Block.Blocks[type];
						if (block.Dynamic)
							ScheduleUpdate(block.Delay, x, y, z);
					}
				}
			}
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

		public bool NeedsUpdatesSent {
			get {
				return tilesChanged > 0;
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

		public void OnTileChanged(int? oldType, int x, int y, int z) {
			LastChanged = DateTime.Now;
			// TODO: build bounding box etc
			tilesChanged++;
			tileChangeX = x;
			tileChangeY = y;
			tileChangeZ = z;

			// if the old block was a dynamic block, remove any updates on it
			if (oldType.HasValue && Block.DynamicBlocks[oldType.Value]) {
				scheduledBlockUpdates.RemoveAll(u => u.X == x && u.Y == y && u.Z == z);
			}
		}

		public void SendUpdates() {
			if (!NeedsUpdatesSent)
				return;
			LastSent = DateTime.Now;

			if (tilesChanged == 1) {
				int chunkTileX = Position.X * 16;
				int chunkTileZ = Position.Y * 16;
				var packet = new UpdateBlockPacket {
					ChunkX = chunkTileX + tileChangeX,
					ChunkY = (byte)tileChangeY,
					ChunkZ = chunkTileZ + tileChangeZ,
					MetaData = (byte)data.MetaData.GetValue(tileChangeX, tileChangeY, tileChangeZ),
					Type = data.GetBlock(tileChangeX, tileChangeY, tileChangeZ)
				};
				foreach (var player in players) {
					player.Client.SendPacket(packet);
				}
			} else {
				foreach (var player in players) {
					player.AddChunkToSend(Position);
				}
			}

			tilesChanged = 0;
			// TODO: send changes done to the chunk
		}

		public void ProcessUpdates() {
			if (scheduledBlockUpdates.Count == 0)
				return;
			int tileX = Position.X * 16;
			int tileZ = Position.Y * 16;
			DateTime now = DateTime.Now;
			ScheduledUpdate next = scheduledBlockUpdates.GetFirst();
			// TODO: limit amount of updates per tick or time spent in here
			while (next.Time <= now) {
				scheduledBlockUpdates.RemoveFirst();
				int type = data.GetBlock(next.X, next.Y, next.Z);
				var block = Block.Blocks[type];
				if (block != null)
					block.OnUpdate(world, tileX + next.X, next.Y, tileZ + next.Z);
				if (scheduledBlockUpdates.Count == 0)
					return;
				next = scheduledBlockUpdates.GetFirst();
			}
		}

		public void ScheduleUpdate(int delay, int x, int y, int z) {
			var update = new ScheduledUpdate {
				Time = DateTime.Now.AddMilliseconds(delay),
				X = (byte)x,
				Y = (byte)y,
				Z = (byte)z
			};
			if (scheduledBlockUpdates.FirstOrDefault(u => u.Equals(update)) == null)
				scheduledBlockUpdates.Add(update);
		}
	}
}
