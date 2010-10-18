using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class PlayerHandler: PacketHandler {
		static int seed = new Random().Next();

		public bool IsFinished { get; protected set; }

		PacketPipe pipe;
		Server server;
		Player player;
		uint packetSentCount = 0;

		public PlayerHandler(Server server, PacketPipe pipe, Player player) {
			this.server = server;
			this.pipe = pipe;
			this.player = player;

			pipe.SetHandler(this);
			//server.World.ChunkPool.AddPlayer(player);
			//pipe.SendPacket(new PositionPacket(0, 4, 0));
			SetPosition(player.X, player.Y, player.Z, 0, 0);

			var mainInv = new InventoryItem[36];
			mainInv[0] = new InventoryItem((int)BlockType.Stone, -1);
			mainInv[1] = new InventoryItem((int)BlockType.Dirt, -1);
			mainInv[2] = new InventoryItem((int)BlockType.Cobblestone, -1);
			mainInv[3] = new InventoryItem((int)BlockType.Planks, -1);
			mainInv[4] = new InventoryItem((int)BlockType.Sand, -1);
			mainInv[5] = new InventoryItem((int)BlockType.StillLava, -1);
			mainInv[6] = new InventoryItem((int)BlockType.StillWater, -1);
			mainInv[7] = new InventoryItem((int)BlockType.Sponge, -1);
			mainInv[8] = new InventoryItem(277, 1);
			pipe.SendPacket(new SendInventoryPacket(InventoryType.Main, mainInv));
			pipe.SendPacket(new SendInventoryPacket(InventoryType.Crafting, new InventoryItem[4]));
			pipe.SendPacket(new SendInventoryPacket(InventoryType.Armor, new InventoryItem[4]));
			pipe.SendPacket(new UpdateTimePacket((int)server.World.Time));
			Broadcast(new ChatPacket("Player connected: " + player.Name));
		}

		public void HandlePackets() {
			pipe.HandlePackets();

			if (packetSentCount++ % 20 == 0) {
				pipe.SendPacket(new NoopPacket());
			}

			if (pipe.IsClosed)
				IsFinished = true;
		}

		public void Broadcast(Packet packet) {
			foreach (var player in server.World.EntityHandler.Players) {
				player.Client.SendPacket(packet);
			}
		}

		public void Disconnect(string reason) {
			pipe.SendPacket(new DisconnectPacket(reason));
			server.World.EntityHandler.RemovePlayer(player);
			IsFinished = true;
			Console.WriteLine("Player {0} disconnected: {1}", player.Name, reason);
			Broadcast(new ChatPacket("Player disconnected: " + player.Name));
		}

		public void SetPosition(double x, double y, double z, float rotX, float rotY) {
			var stance = y + 1.6;
			player.SetPosition(x, y, stance, z, rotX, rotY);
			pipe.SendPacket(new PlayerMoveLookPacket(x, y, stance, z, rotX, rotY, false));

			//player.Client.SendPacket(new PlayerMoveLookPacket(player.X, player.Y, player.Y + 1.6, player.Z, player.RotationX, player.RotationY, false));
		}

		public override void Handle(PlayerMovePacket packet) {
			//Console.WriteLine("Move: {0:0.00}|{1:0.00}|{2:0.00}", packet.X, packet.Y, packet.Z);
			player.X = packet.X;
			player.Y = packet.Y;
			player.Z = packet.Z;
		}

		public override void Handle(PlayerLookPacket packet) {
			//Console.WriteLine("Look: {0:0.00}|{1:0.00}", packet.RotationX, packet.RotationY);
			player.RotationX = packet.RotationX;
			player.RotationY = packet.RotationY;
		}

		public override void Handle(PlayerMoveLookPacket packet) {
			//Console.WriteLine("MoveLook");
			player.X = packet.X;
			player.Y = packet.Y;
			player.Z = packet.Z;
			player.RotationX = packet.RotationX;
			player.RotationY = packet.RotationY;
		}

		public override void Handle(ChatPacket packet) {
			string rawmsg = packet.Message;
			if (rawmsg.StartsWith("/")) {
				var p = rawmsg.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
				if (p[0] == "/tp") {
					int x = (int)player.X;
					int.TryParse(p[1], out x);
					int y;
					if (p.Length > 2 && int.TryParse(p[2], out y)) {
						int z = (int)player.Z;
						if (p.Length > 3)
							int.TryParse(p[3], out z);
						SetPosition(x, y, z, 0, 0);
					} else {
						var toPlayer = server.World.EntityHandler.Players.FirstOrDefault(e => e.Name == p[1]);
						if (toPlayer != null) {
							SetPosition(toPlayer.X, toPlayer.Y + 2, toPlayer.Z, 0, 0);
						} else {
							pipe.SendPacket(new ChatPacket("Couldn't find player " + p[1]));
						}
					}
				} else if (p[0] == "/save") {
					server.World.ChunkPool.SaveAll();
				} else if (p[0] == "/time") {
					if (p.Length == 2) {
						int time;
						if (int.TryParse(p[1], out time))
							server.World.UpdateTime(time);
					}
				} else if (p[0] == "/item") {
					if (p.Length == 2) {
						int id;
						if (int.TryParse(p[1], out id))
							pipe.SendPacket(new PlayerAddItemPacket {
								Count = 255,
								TypeId = (short)id
							});
					}
				} else if (p[0] == "/reltp") {
					int x, y, z;
					if (p.Length == 4 && int.TryParse(p[1], out x) && int.TryParse(p[2], out y) && int.TryParse(p[3], out z))
						SetPosition(player.X + x, player.Y + y, player.Z + z, player.RotationX, player.RotationY);
				}
				return;
			}
			string message = String.Format("<{0}> {1}", player.Name, packet.Message);
			Broadcast(new ChatPacket(message));
		}

		public override void Handle(DisconnectPacket packet) {
			Disconnect("Player Disconnected");
		}

		public override void Handle(PlayerDigPacket packet) {
			//Console.WriteLine("Dig: {0}, {1}, {2}, {3}, {4}", packet.State, packet.DigX, packet.DigY, packet.DigZ, packet.Face);
			if (packet.State == 3) {
				server.World.SetBlockType(packet.DigX, packet.DigY, packet.DigZ, (int)BlockType.Air);
			}
		}

		public override void Handle(PlayerPlaceBlockPacket packet) {
			//pipe.SendPacket(new ChatPacket(String.Format("{0}, {1}, {2}", packet.X, packet.Y, packet.Z)));
			int destX = packet.X;
			int destY = packet.Y;
			int destZ = packet.Z;

			if (packet.Face == 0)
				destY--;
			else if (packet.Face == 1)
				destY++;
			else if (packet.Face == 2)
				destZ--;
			else if (packet.Face == 3)
				destZ++;
			else if (packet.Face == 4)
				destX--;
			else if (packet.Face == 5)
				destX++;

			// TODO: deal with using items (ids >= 256) here
			int typeId = (int)BlockType.Dirt;
			if (packet.Type != -1 && packet.Type < 256)
				typeId = packet.Type;

			server.World.SetBlockType(destX, destY, destZ, typeId);
			pipe.SendPacket(new UpdateBlockPacket(player.World, destX, destY, destZ));
		}

		public override void OnError(string reason) {
			Disconnect("Error: " + reason);
		}
	}
}
