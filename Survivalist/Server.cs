using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Survivalist {
	public class Server {
		public ConnectionHandler ConnectionHandler;
		public World World;

		public Server() {
			World = new World();
			ConnectionHandler = new ConnectionHandler(this, 12345);
		}

		public void Start() {
			var lastUpdate = DateTime.Now;
			long excessTime = 0;
			while (true) {
				var timeDiff = (long)(DateTime.Now - lastUpdate).TotalMilliseconds;
				lastUpdate = DateTime.Now;
				excessTime += timeDiff;

				while (excessTime >= 50) {
					excessTime -= 50;
					Tick();
				}

				Thread.Sleep(1);
			}
		}

		void Tick() {
			World.Tick();
			ConnectionHandler.HandlePackets();
		}
	}
}
