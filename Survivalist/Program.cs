using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace Survivalist {
	class Program {
		static void Main(string[] args) {
			Server server = new Server();
			server.Start();
		}
	}
}
