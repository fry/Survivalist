using System.IO;
using System;
namespace Survivalist {
	public class Test {
		public static void Main(string[] argv) {
			var stream = new FileStream(@"D:\projects\Survivalist\data.dat", FileMode.Open);
			var net = new NetworkReader(stream);

			while (true) {
				var packet = Packet.Get(net);
				Console.WriteLine(packet.GetType());
				/*if (packet is PlayerMoveLookPacket) {
					var mypacket = packet as PlayerMoveLookPacket;
					Console.WriteLine("PlayerMoveLook: {0:0.00}, {1:0.00}, {2:0.00}, {3:0.00}", mypacket.X, mypacket.Y, mypacket.Stance, mypacket.Z);
					Console.WriteLine("Rot: {0:0.00}; {1:0.00}", mypacket.RotationX, mypacket.RotationY);
				}
				if (packet is PositionPacket) {
					var mypacket = packet as PositionPacket;
					Console.WriteLine("Pos: {0}, {1}, {2}", mypacket.X, mypacket.Y, mypacket.Z);
				}*/
				/*if (packet is UpdateTimePacket) {
					var mypacket = packet as UpdateTimePacket;
					Console.WriteLine("Time: " + mypacket.Time);
				}
				if (packet is UpdateBlockPacket) {
					var mypacket = packet as UpdateBlockPacket;
					Console.WriteLine("Update block: {0}, {1}, {2}, {3}, {4}", mypacket.ChunkX, mypacket.ChunkY, mypacket.ChunkZ, mypacket.Type, mypacket.MetaData);
				}
				if (packet is PlayerDigPacket) {
					var mypacket = packet as PlayerDigPacket;
					Console.WriteLine("Dig: {0}, {1}, {2}", mypacket.DigX, mypacket.DigY, mypacket.DigZ);
				}*/
			}

			/*var stream = new MemoryStream();
			var writer = new NetworkWriter(stream);
			double val = 1000.12345678;
			Console.WriteLine(val);
			writer.WriteDouble(val);
			stream.Seek(0, SeekOrigin.Begin);
			var reader = new NetworkReader(stream);
			Console.WriteLine(reader.ReadDouble());*/
		}
	}
}