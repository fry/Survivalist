using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class EntityHandler {
		World world;
		List<Entity> entities = new List<Entity>();
		List<Player> players = new List<Player>();
		List<Entity> removeEntities = new List<Entity>();

		HashSet<EntityTracker> trackedEntities = new HashSet<EntityTracker>();

		public EntityHandler(World world) {
			this.world = world;
		}

		public IEnumerable<Player> Players {
			get {
				return players;
			}
		}

		public void TrackEntity(Entity entity, int range, int interval) {
			var tracker = new EntityTracker(world, entity, range, interval);
			tracker.UpdatePosition(players);
			trackedEntities.Add(tracker);
		}

		public void AddEntity(Entity entity) {
			if (entity is Player) {
				// Update trackers with the new player
				foreach (var tracker in trackedEntities) {
					tracker.Update(entity as Player);
				}
				TrackEntity(entity, 512, 2);
			} else if (entity is ItemEntity)
				TrackEntity(entity, 64, 20);

			entities.Add(entity);
		}

		public void RemoveEntity(Entity entity) {
			entity.Deleted = true;
			removeEntities.Add(entity);
		}

		public void AddPlayer(Player player) {
			players.Add(player);
			AddEntity(player);
		}

		public void RemovePlayer(Player player) {
			players.Remove(player);
			RemoveEntity(player);
		}

		public Player NewPlayer(LoginHandler client, string accountName, string serverPassword) {
			var player = new Player(client, world, accountName);
			player.X = 0;
			player.Y = 64;
			player.Z = 0;
			Console.WriteLine("Player #{0} connected: {1}", player.Id, accountName);
			return player;
		}

		public void Tick() {
			foreach (var remove in removeEntities) {
				// Remove tracker and player from tracked entities
				var tracker = trackedEntities.FirstOrDefault(e => e.Equals(remove));
				if (tracker != null) {
					trackedEntities.Remove(tracker);
					tracker.RemoveSelf();
				}
				entities.Remove(remove);
			}
			removeEntities.Clear();

			foreach (var tracker in trackedEntities) {
				tracker.UpdatePosition(players);
				if (tracker.HasMoved && tracker.Origin is Player) {
					// If this tracker's player has moved, update all other trackers with it
					foreach (var tracker2 in trackedEntities) {
						tracker2.Update(tracker.Origin as Player);
					}
				}
			}

			foreach (var player in players) {
				player.SendChunks();
			}
		}
	}
}
