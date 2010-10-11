using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace Survivalist {
	public class PacketPipe {
		public static readonly object Lock = new object();

		public bool IsClosed { get; protected set; }
		public string CloseReason;

		PacketHandler handler;
		TcpClient client;
		NetworkReader reader;
		NetworkWriter writer;
		Thread readThread;
		Thread writeThread;

		Queue<Packet> sendQueue = new Queue<Packet>();
		Queue<Packet> recieveQueue = new Queue<Packet>();

		int timeout = 0;

		public PacketPipe(TcpClient client, PacketHandler handler) {
			this.client = client;
			this.handler = handler;

			var stream = client.GetStream();
			reader = new NetworkReader(stream);
			writer = new NetworkWriter(stream);

			readThread = new Thread(new ThreadStart(ReadLoop));
			writeThread = new Thread(new ThreadStart(WriteLoop));
			readThread.Start();
			writeThread.Start();
		}

		public void SetHandler(PacketHandler handler) {
			this.handler = handler;
		}

		public void SendPacket(Packet packet) {
			lock (sendQueue) {
				sendQueue.Enqueue(packet);
			}
		}

		// Invoke packet handler on each packet in the queue
		public void HandlePackets() {
			if (recieveQueue.Count == 0) {
				if (timeout++ > 60000 / 50) {
					Close("Timeout");
				}
			} else {
				timeout = 0;
			}

			// TODO: limit amount of packets handled
			while (recieveQueue.Count > 0) {
				Packet packet;
				lock (recieveQueue) {
					packet = recieveQueue.Dequeue();
				}
				packet.Handle(handler);
			}

			if (IsClosed) {
				handler.OnError(CloseReason);
			}
		}

		protected void ReadLoop() {
			while (client.Connected) {
				try {
					var packet = Packet.Get(reader);
					if (packet != null) {
						lock (recieveQueue) {
							recieveQueue.Enqueue(packet);
						}
						//Console.WriteLine("Recieve: " + packet.GetType());
					} else {
						Console.WriteLine("Unknown packet");
					}
				} catch (Exception e) {
					Close("Read Error: " + e.Message);
					return;
				}
			}
		}

		protected void WriteLoop() {
			while (client.Connected) {
				if (sendQueue.Count > 0) {
					try {
						Packet packet;
						lock (this) {
							packet = sendQueue.Dequeue();
						}
						//Console.WriteLine("Send " + packet.GetType());
						Packet.Put(packet, writer);
						writer.Flush();
					} catch (Exception e) {
						Close("Write Error: " + e.Message);
						return;
					}
				} else {
					Thread.Sleep(10);
				}
			}
		}

		public void Close(string reason) {
			Console.WriteLine("Try close: " + reason);
			if (IsClosed)
				return;
			IsClosed = true;
			CloseReason = reason;
			readThread.Abort();
			writeThread.Abort();
			client.Close();
		}
	}
}
