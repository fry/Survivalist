﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Survivalist {
	public class LoginHandler: PacketHandler {
		public bool IsFinished { get; protected set; }

		PacketPipe pipe;
		Server server;

		public LoginHandler(Server server, TcpClient client) {
			this.server = server;
			pipe = new PacketPipe(client, this);
		}

		public void SendPacket(Packet packet) {
			pipe.SendPacket(packet);
		}

		public void HandlePackets() {
			pipe.HandlePackets();
		}

		public override void Handle(LoginPacket packet) {
			Console.WriteLine("Connected: {0}|{1}|{2}", packet.AccountName, packet.Password, packet.Version);

			if (packet.Version != Packet.Version) {
				pipe.Close(String.Format("Invalid protocol version {0} != {1}", packet.Version, Packet.Version));
				return;
			}

			pipe.SendPacket(new LoginPacket(0, "", "", (long)server.World.Time, -1));
			pipe.SendPacket(new SetSpawnPacket(0, 64, 0));

			var player = server.World.EntityHandler.NewPlayer(pipe, packet.AccountName, packet.Password);
			var ingame = new PlayerHandler(server, pipe, player);
			server.ConnectionHandler.AddPlayer(ingame);
			server.World.EntityHandler.AddPlayer(player);
			IsFinished = true;
		}

		public override void Handle(VerifyPacket packet) {
			// TODO: add client verification here
			Console.WriteLine("Verify account: " + packet.Contents);
			pipe.SendPacket(new VerifyPacket("-"));
		}
	}
}
