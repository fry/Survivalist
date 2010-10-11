using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	/* Keeps track of which other players an entitiy has been added to and
	 * ensures position updates are sent to those. */
	public class EntityTracker {
		public static int ChunkRange = 10;
		// Whether this entity as moved a signification distance last tick
		public bool HasMoved { get; protected set; }
		public Entity Origin { get; protected set; }

		World world;
		int trackRange;
		int interval;
		int currentInterval;
		HashSet<Player> subscribedEntities = new HashSet<Player>();
		bool firstRun = true;

		// The last position from which we checked for new subscribers
		double lastUpdateX;
		double lastUpdateY;
		double lastUpdateZ;

		// The last position we sent to our subscribers
		int lastPosSentX;
		int lastPosSentY;
		int lastPosSentZ;
		byte lastRotSentX;
		byte lastRotSentY;

		public EntityTracker(World world, Entity entity, int trackRange, int interval) {
			this.world = world;
			Origin = entity;
			this.trackRange = trackRange;
			this.interval = interval;

			lastPosSentX = Entity.EncodePosition(Origin.X);
			lastPosSentY = Entity.EncodePosition(Origin.Y);
			lastPosSentZ = Entity.EncodePosition(Origin.Z);

			lastRotSentX = Entity.EncodeAngle(Origin.RotationX);
			lastRotSentY = Entity.EncodeAngle(Origin.RotationY);
		}

		public override int GetHashCode() {
			return Origin.Id;
		}

		public override bool Equals(object obj) {
			if (obj is Entity)
				return Origin.Equals(obj as Entity);
			return base.Equals(obj);
		}

		bool IsChunkInRange(int x1, int z1, int x2, int z2) {
			int dx = Math.Abs(x1 - x2);
			int dz = Math.Abs(z1 - z2);
			return dx <= ChunkRange && dz <= ChunkRange;
		}

		public void UpdatePosition(IEnumerable<Player> players) {
			HasMoved = false;
			// Run range check on the first run or if we moved a certain distance
			if (firstRun || Origin.Distance(lastUpdateX, lastUpdateY, lastUpdateZ) > 4*4) {
				HasMoved = true;
				UpdateSubscriber(players);

				// Update chunks for the player
				if (Origin is Player) {
					int oldChunkX = (int)(lastUpdateX / 16);
					int oldChunkZ = (int)(lastUpdateZ / 16);
					int newChunkX = (int)(Origin.X / 16);
					int newChunkZ = (int)(Origin.Z / 16);
					if (firstRun || oldChunkX != newChunkX || oldChunkZ != newChunkZ) {
						var player = Origin as Player;
						int diffX = newChunkX - oldChunkX;
						int diffZ = newChunkZ - oldChunkZ;
						// Go through chunks in a radius of ChunkRange(= 10)
						for (int x = newChunkX - ChunkRange; x < newChunkX + ChunkRange; x++) {
							for (int z = newChunkZ - ChunkRange; z < newChunkZ + ChunkRange; z++) {
								// Add new chunks
								if (IsChunkInRange(newChunkX, newChunkZ, x, z)) {
									var chunk = world.ChunkPool.GetChunk(x, z, true);
									if (chunk != null)
										chunk.AddPlayer(player);
								}
								// Delete old chunks
								if (!IsChunkInRange(newChunkX, newChunkZ, x - diffX, z - diffZ)) {
									var chunk = world.ChunkPool.GetChunk(x - diffX, z - diffZ, false);
									if (chunk != null)
										chunk.RemovePlayer(player);
								}
							}
						}
					}
				}

				lastUpdateX = Origin.X;
				lastUpdateY = Origin.Y;
				lastUpdateZ = Origin.Z;
				firstRun = false;
			}

			// Only broadcast position updates in the specified interval
			if (currentInterval++ % interval == 0) {
				int newPosX = Entity.EncodePosition(Origin.X);
				int newPosY = Entity.EncodePosition(Origin.Y);
				int newPosZ = Entity.EncodePosition(Origin.Z);

				byte newRotX = Entity.EncodeAngle(Origin.RotationX);
				byte newRotY = Entity.EncodeAngle(Origin.RotationY);

				bool positionChanged = newPosX != lastPosSentX || newPosY != lastPosSentY || newPosZ != lastPosSentZ;
				bool rotationChanged = newRotX != lastRotSentX || newRotY != lastRotSentY;

				int xDiff = newPosX - lastPosSentX;
				int yDiff = newPosY - lastPosSentY;
				int zDiff = newPosZ - lastPosSentZ;

				Packet packet;
				// Position changed for more than four tiles, we need to teleport
				int fourTiles = 32 * 4;
				if (Math.Abs(xDiff) > fourTiles || Math.Abs(yDiff) > fourTiles || Math.Abs(zDiff) > fourTiles) {
					packet = new EntityTeleportPacket {
						EntityId = Origin.Id,
						X = newPosX,
						Z = newPosZ,
						Y = newPosY,
						RotationX = newRotX,
						RotationY = newRotY
					};
				} else if (positionChanged && rotationChanged) {
					packet = new EntityUpdateMoveLookPacket {
						EntityId = Origin.Id,
						XDiff = (byte)xDiff,
						YDiff = (byte)yDiff,
						ZDiff = (byte)zDiff,
						RotationX = newRotX,
						RotationY = newRotY
					};
				} else if (positionChanged) {
					packet = new EntityUpdateMovePacket {
						EntityId = Origin.Id,
						XDiff = (byte)xDiff,
						YDiff = (byte)yDiff,
						ZDiff = (byte)zDiff
					};
				} else if (rotationChanged) {
					packet = new EntityUpdateLookPacket {
						EntityId = Origin.Id,
						RotationX = newRotX,
						RotationY = newRotY
					};
				} else {
					packet = new EntityInitializePacket(Origin.Id);
				}

				// Send updates to tracked player
				Broadcast(packet);

				lastPosSentX = newPosX;
				lastPosSentY = newPosY;
				lastPosSentZ = newPosZ;
				lastRotSentX = newRotX;
				lastRotSentY = newRotY;
			}
		}

		public void Broadcast(Packet packet) {
			foreach (var player in subscribedEntities) {
				player.Client.SendPacket(packet);
			}
		}

		// Check if a specific player is in range and subscribe/unsubscribe it depending on that
		public void Update(Player player) {
			if (Origin == player)
				return;

			var distanceX = Math.Abs(player.X - Origin.X);
			var distanceZ = Math.Abs(player.Z - Origin.Z);
			// Add or remove entity depending on range
			if (distanceX <= trackRange && distanceZ <= trackRange) {
				if (!subscribedEntities.Contains(player)) {
					subscribedEntities.Add(player);
					player.Client.SendPacket(CreateAddPacket());
				}
			} else if (subscribedEntities.Contains(player)) {
				// The player went out of range, remove him
				subscribedEntities.Remove(player);
				player.Client.SendPacket(new DestroyEntityPacket(Origin));
			}
		}

		// Check each player's subscriber status
		protected void UpdateSubscriber(IEnumerable<Player> players) {
			foreach (var player in players)
				Update(player);
		}

		// Remove ourself from our children
		public void RemoveSelf() {
			Broadcast(new DestroyEntityPacket(Origin));

			// Remove this player from chunks he is in
			if (Origin is Player) {
				int chunkX = (int)(lastUpdateX / 16);
				int chunkZ = (int)(lastUpdateZ / 16);
				for (int x = chunkX - ChunkRange; x < chunkX + ChunkRange; x++) {
					for (int y = chunkZ - ChunkRange; y < chunkZ + ChunkRange; y++) {
						var chunk = world.ChunkPool.GetChunk(x, y, false);
						if (chunk != null)
							chunk.RemovePlayer(Origin as Player);
					}
				}
			}
		}

		protected Packet CreateAddPacket() {
			if (Origin is Player) {
				var packet = new SpawnNamedEntityPacket(Origin as Player);
				// Send the last sent position so any following relative position packets are correct
				packet.X = lastPosSentX;
				packet.Y = lastPosSentY;
				packet.Z = lastPosSentZ;
				packet.RotationX = lastRotSentX;
				packet.RotationY = lastRotSentY;

				return packet;
			} else if (Origin is ItemEntity) {
				var packet = new SpawnItemEntityPacket(Origin as ItemEntity);
				packet.X = lastPosSentX;
				packet.Y = lastPosSentY;
				packet.Z = lastPosSentZ;

				return packet;
			}

			throw new ArgumentException("Can't add object of type " + Origin.GetType());
		}
	}
}
