using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Survivalist {
	public class ConnectionHandler {
		Server server;
		int port;
		Thread listenThread;

		List<LoginHandler> loginClients = new List<LoginHandler>();
		List<PlayerHandler> gameClients = new List<PlayerHandler>();

		public ConnectionHandler(Server server, int port) {
			this.server = server;
			this.port = port;

			listenThread = new Thread(new ThreadStart(StartListen));
			listenThread.Start();
		}

		void StartListen() {
			var listener = new TcpListener(IPAddress.Any, port);
			listener.Start();

			Console.WriteLine("Accepting connections");

			while (true) {
				var client = listener.AcceptTcpClient();
				Console.WriteLine("Connection");
				var handler = new LoginHandler(server, client);
				loginClients.Add(handler);
			}
		}

		public void AddPlayer(PlayerHandler player) {
			gameClients.Add(player);
		}

		// handle packets for all clients
		public void HandlePackets() {
			for(int i=0; i< loginClients.Count; i++) {
				var login = loginClients[i];
				login.HandlePackets();
				if (login.IsFinished) {
					loginClients.Remove(login);
					i--;
				}
			}

			for (int i = 0; i < gameClients.Count; i++) {
				var game = gameClients[i];
				game.HandlePackets();
				if (game.IsFinished) {
					gameClients.Remove(game);
					i--;
				}
			}
		}
	}
}
