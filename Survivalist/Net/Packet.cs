using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Survivalist {
	public abstract class Packet {
		static Dictionary<int, Type> IdTypes = new Dictionary<int, Type>();
		static Dictionary<Type, int> TypeIds = new Dictionary<Type, int>();

		static Packet() {
			AddPacketType(0x00, typeof(NoopPacket));
			AddPacketType(0x01, typeof(LoginPacket));
			AddPacketType(0x02, typeof(VerifyPacket));
			AddPacketType(0x03, typeof(ChatPacket));
			AddPacketType(0x04, typeof(UpdateTimePacket));
			AddPacketType(0x05, typeof(SendInventoryPacket));
			AddPacketType(0x06, typeof(PositionPacket));

			AddPacketType(0x0A, typeof(FlyingPacket));
			AddPacketType(0x0B, typeof(PlayerMovePacket));
			AddPacketType(0x0C, typeof(PlayerLookPacket));
			AddPacketType(0x0D, typeof(PlayerMoveLookPacket));
			AddPacketType(0x0E, typeof(PlayerDigPacket));
			AddPacketType(0x0F, typeof(PlayerPlaceBlockPacket));

			AddPacketType(0x10, typeof(PlayerChangeActiveItemPacket));

			AddPacketType(0x12, typeof(EntityUpdateArmPacket));

			AddPacketType(0x14, typeof(SpawnNamedEntityPacket));
			AddPacketType(0x15, typeof(SpawnItemEntityPacket));

			AddPacketType(0x1D, typeof(DestroyEntityPacket));
			AddPacketType(0x1E, typeof(EntityInitializePacket));
			AddPacketType(0x1F, typeof(EntityUpdateMovePacket));
			AddPacketType(0x20, typeof(EntityUpdateLookPacket));
			AddPacketType(0x21, typeof(EntityUpdateMoveLookPacket));
			AddPacketType(0x22, typeof(EntityTeleportPacket));

			AddPacketType(0x18, typeof(SpawnMobPacket));

			AddPacketType(0x32, typeof(AddToChunkPacket));
			AddPacketType(0x33, typeof(UpdateFullChunkPacket));
			AddPacketType(0x35, typeof(UpdateBlockPacket));

			AddPacketType(0x3B, typeof(SendTileEntityAttributesPacket));

			AddPacketType(0xFF, typeof(DisconnectPacket));
		}

		public Packet() {}

		public static void AddPacketType(int id, Type type) {
			IdTypes.Add(id, type);
			TypeIds.Add(type, id);
		}

		public static Packet Get(int id) {
			Type type;
			if (!IdTypes.TryGetValue(id, out type)) {
				Console.WriteLine("Unknown packet: 0x" + id.ToString("x"));
				return null;
			}

			//Console.WriteLine("0x"+id.ToString("x") + ", " + id);
			return Activator.CreateInstance(type) as Packet;
		}

		public static Packet Get(NetworkReader reader) {
			var packetId = reader.ReadByte();
			var packet = Get(packetId);
			if (packet == null)
				return null;
			packet.Read(reader);
			return packet;
		}

		public static void Put(Packet packet, NetworkWriter writer) {
			writer.Write((byte)packet.Id);
			packet.Write(writer);
			writer.Flush();
		}

		public int? Id {
			get {
				int id;
				if (!TypeIds.TryGetValue(GetType(), out id))
					return null;
				return id;
			}
		}

		public abstract void Handle(PacketHandler handler);
		public abstract void Write(NetworkWriter writer);
		public abstract void Read(NetworkReader reader);
	}
}
