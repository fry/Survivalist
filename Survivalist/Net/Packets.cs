using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using zlib;
using System.IO;

namespace Survivalist {
	#region LoginPackets
	public class LoginPacket : Packet {
		public int Version;
		public string AccountName;
		public string Password;

		public LoginPacket() { }

		public LoginPacket(int version, string accountName, string password) {
			Version = version;
			AccountName = accountName;
			Password = password;
		}

		public override void Read(NetworkReader reader) {
			Version = reader.ReadInt32();
			AccountName = reader.ReadUTF8();
			Password = reader.ReadUTF8();
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32((Int32)Version);
			writer.WriteUTF8(AccountName);
			writer.WriteUTF8(Password);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class VerifyPacket : Packet {
		public string Contents;

		public VerifyPacket() { }

		public VerifyPacket(string contents) {
			Contents = contents;
		}

		public override void Read(NetworkReader reader) {
			Contents = reader.ReadUTF8();
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteUTF8(Contents);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}
	#endregion

	public class NoopPacket : Packet {
		public NoopPacket() {}

		public override void Read(NetworkReader reader) {
		}

		public override void Write(NetworkWriter writer) {
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class ChatPacket : Packet {
		public string Message;

		public ChatPacket() { }

		public ChatPacket(string message) {
			Message = message;
		}

		public override void Read(NetworkReader reader) {
			Message = reader.ReadUTF8();
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteUTF8(Message);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class DisconnectPacket : Packet {
		public string Reason;

		public DisconnectPacket() { }
		public DisconnectPacket(string reason) {
			Reason = reason;
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteUTF8(Reason);
		}

		public override void Read(NetworkReader reader) {
			Reason = reader.ReadUTF8();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class UpdateTimePacket : Packet {
		public long Time;

		public UpdateTimePacket() { }

		public UpdateTimePacket(long time) {
			Time = time;
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt64(Time);
		}

		public override void Read(NetworkReader reader) {
			Time = reader.ReadInt64();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	#region InventoryPackets
	public class SendInventoryPacket : Packet {
		InventoryType Type;
		InventoryItem[] Items;

		public SendInventoryPacket() {}

		public SendInventoryPacket(InventoryType type, InventoryItem[] items) {
			Type = type;
			Items = items;
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32((int)Type);
			writer.WriteInt16((short)Items.Length);
			for (int i = 0; i < Items.Length; i++) {
				var item = Items[i];
				if (item == null)
					writer.WriteInt16(-1);
				else {
					writer.WriteInt16((short)item.Id);
					writer.WriteByte((byte)item.Count);
					writer.WriteInt16((short)item.Damage);
				}
			}
		}

		public override void Read(NetworkReader reader) {
			Type = (InventoryType)reader.ReadInt32();
			int count = reader.ReadInt16();
			Items = new InventoryItem[count];
			for (int i = 0; i < count; i++) {
				int id = reader.ReadInt16();
				if (id == -1) {
					Items[i] = null;
				} else {
					int itemCount = reader.ReadByte();
					int damage = reader.ReadInt16();
					Items[i] = new InventoryItem(id, itemCount, damage);
				}
			}
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class PlayerCollectItemEntityPacket : Packet {
		public int CollectEntityId;
		public int CollectingEntityId;

		public PlayerCollectItemEntityPacket() { }

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(CollectEntityId);
			writer.WriteInt32(CollectingEntityId);
		}

		public override void Read(NetworkReader reader) {
			CollectEntityId = reader.ReadInt32();
			CollectingEntityId = reader.ReadInt32();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class PlayerAddItemPacket : Packet {
		public short TypeId;
		public byte Count;
		public short Durability;

		public PlayerAddItemPacket() { }

		public override void Write(NetworkWriter writer) {
			writer.WriteInt16(TypeId);
			writer.WriteByte(Count);
			writer.WriteInt16(Durability);
		}

		public override void Read(NetworkReader reader) {
			TypeId = reader.ReadInt16();
			Count = reader.ReadByte();
			Durability = reader.ReadInt16();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	#endregion

	#region UpdatePlayerPackets
	public class FlyingPacket : Packet {
		public bool Flying;

		public FlyingPacket() {}

		public FlyingPacket(bool flying) {
			Flying = flying;
		}

		public override void Read(NetworkReader reader) {
			Flying = reader.ReadBoolean();
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteBoolean(Flying);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class PlayerMovePacket : FlyingPacket {
		public double X, Y, Stance, Z;

		public PlayerMovePacket() { }

		public override void Write(NetworkWriter writer) {
			writer.WriteDouble(X);
			writer.WriteDouble(Y);
			writer.WriteDouble(Stance);
			writer.WriteDouble(Z);
			base.Write(writer);
		}

		public override void Read(NetworkReader reader) {
			X = reader.ReadDouble();
			Y = reader.ReadDouble();
			Stance = reader.ReadDouble();
			Z = reader.ReadDouble();
			base.Read(reader);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class PlayerLookPacket : FlyingPacket {
		public float RotationX, RotationY;

		public PlayerLookPacket() { }

		public override void Write(NetworkWriter writer) {
			writer.WriteSingle(RotationX);
			writer.WriteSingle(RotationY);
			base.Write(writer);
		}

		public override void Read(NetworkReader reader) {
			RotationX = reader.ReadSingle();
			RotationY = reader.ReadSingle();
			base.Read(reader);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class PlayerMoveLookPacket : FlyingPacket {
		public double X, Y, Stance, Z;
		public float RotationX, RotationY;

		public PlayerMoveLookPacket() { }
		public PlayerMoveLookPacket(double x, double y, double stance, double z, float rotationX, float rotationY, bool flying): base(flying) {
			X = x; Y = y; Stance = stance; Z = z;
			RotationX = rotationX; RotationY = rotationY;
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteDouble(X);
			writer.WriteDouble(Y);
			writer.WriteDouble(Stance);
			writer.WriteDouble(Z);
			writer.WriteSingle(RotationX);
			writer.WriteSingle(RotationY);
			base.Write(writer);
		}

		public override void Read(NetworkReader reader) {
			X = reader.ReadDouble();
			Y = reader.ReadDouble();
			Stance = reader.ReadDouble();
			Z = reader.ReadDouble();
			RotationX = reader.ReadSingle();
			RotationY = reader.ReadSingle();
			base.Read(reader);
		}
	}

	public class PlayerDigPacket : Packet {
		public byte State; // 0 stop 1 digging 2 finished
		public int DigX;
		public byte DigY;
		public int DigZ;
		public byte Face;

		public PlayerDigPacket() { }

		public override void Write(NetworkWriter writer) {
			throw new NotImplementedException();
		}

		public override void Read(NetworkReader reader) {
			State = reader.ReadByte();
			DigX = reader.ReadInt32();
			DigY = reader.ReadByte();
			DigZ = reader.ReadInt32();
			Face = reader.ReadByte();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class PlayerPlaceBlockPacket : Packet {
		public int Type;
		public int X;
		public byte Y;
		public int Z;
		public byte Face;

		public PlayerPlaceBlockPacket() { }

		public override void Write(NetworkWriter writer) {
			throw new NotImplementedException();
		}

		public override void Read(NetworkReader reader) {
			Type = reader.ReadInt16();
			X = reader.ReadInt32();
			Y = reader.ReadByte();
			Z = reader.ReadInt32();
			Face = reader.ReadByte();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class PlayerChangeActiveItemPacket : Packet {
		int Unknown;
		public short Slot; // ? probably

		public PlayerChangeActiveItemPacket() { }

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(Unknown);
			writer.WriteInt16(Slot);
		}

		public override void Read(NetworkReader reader) {
			Unknown = reader.ReadInt32();
			Slot = reader.ReadInt16();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	#endregion

	#region PositionPackets
	public class PositionPacket : Packet {
		public int X, Y, Z;

		public PositionPacket() { }

		public PositionPacket(int x, int y, int z) {
			X = x;
			Y = y;
			Z = z;
		}

		public override void Read(NetworkReader reader) {
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
			Z = reader.ReadInt32();
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(X);
			writer.WriteInt32(Y);
			writer.WriteInt32(Z);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}
	#endregion

	#region AddPackets
	class SpawnNamedEntityPacket : Packet {
		public int EntityId;
		public string Name;
		public int X, Y, Z;
		public float RotationX, RotationY;
		public int ActiveItemTypeId;

		public SpawnNamedEntityPacket() { }
		public SpawnNamedEntityPacket(NamedEntity entity) {
			EntityId = entity.Id;
			Name = entity.Name;
			X = (int)(entity.X * 32);
			Y = (int)(entity.Y * 32);
			Z = (int)(entity.Z * 32);
			RotationX = entity.RotationX;
			RotationY = entity.RotationY;

			var activeItem = entity.Inventory.ActiveItem;
			//if (activeItem == null)
				ActiveItemTypeId = 0;
			//else
			//	ActiveItemTypeId = activeItem.Id;
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(EntityId);
			writer.WriteUTF8(Name);
			writer.WriteInt32(X);
			writer.WriteInt32(Y);
			writer.WriteInt32(Z);
			writer.WriteAngle(RotationX);
			writer.WriteAngle(RotationY);
			writer.WriteInt16((short)ActiveItemTypeId);
		}

		public override void Read(NetworkReader reader) {
			throw new NotImplementedException();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	class SpawnMobPacket : Packet {
		public int EntityId;
		public byte TypeId;
		public int X, Y, Z;
		public byte RotationX, RotationY;

		public SpawnMobPacket() { }

		public override void Read(NetworkReader reader) {
			EntityId = reader.ReadInt32();
			TypeId = reader.ReadByte();
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
			Z = reader.ReadInt32();
			RotationX = reader.ReadByte();
			RotationY = reader.ReadByte();
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(EntityId);
			writer.WriteByte(TypeId);
			writer.WriteInt32(X);
			writer.WriteInt32(Y);
			writer.WriteInt32(Z);
			writer.WriteByte(RotationX);
			writer.WriteByte(RotationY);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class SpawnItemEntityPacket : Packet {
		public int EntityId;
		public short TypeId;
		public byte Count;
		public int X, Y, Z;
		public byte MoveX, MoveY, MoveZ;

		public SpawnItemEntityPacket() { }
		public SpawnItemEntityPacket(ItemEntity entity) {
			EntityId = entity.Id;
			TypeId = (short)entity.TypeId;
			Count = (byte)entity.Count;
			X = (int)Math.Floor(entity.X * 32);
			Y = (int)Math.Floor(entity.Y * 32);
			Z = (int)Math.Floor(entity.Z * 32);
			MoveX = (byte)(entity.MoveX * 128);
			MoveY = (byte)(entity.MoveY * 128);
			MoveZ = (byte)(entity.MoveZ * 128);
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(EntityId);
			writer.WriteInt16(TypeId);
			writer.WriteByte(Count);
			writer.WriteInt32(X);
			writer.WriteInt32(Y);
			writer.WriteInt32(Z);
			writer.WriteByte(MoveX);
			writer.WriteByte(MoveY);
			writer.WriteByte(MoveZ);
		}

		public override void Read(NetworkReader reader) {
			EntityId = reader.ReadInt32();
			TypeId = reader.ReadInt16();
			Count = reader.ReadByte();
			X = reader.ReadInt32();
			Y = reader.ReadInt32();
			Z = reader.ReadInt32();
			MoveX = reader.ReadByte();
			MoveY = reader.ReadByte();
			MoveZ = reader.ReadByte();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class DestroyEntityPacket : Packet {
		public int EntityId;

		public DestroyEntityPacket() { }
		public DestroyEntityPacket(Entity entity) {
			EntityId = entity.Id;
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(EntityId);
		}

		public override void Read(NetworkReader reader) {
			EntityId = reader.ReadInt32();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}
	#endregion

	#region ChunkPackets
	public class AddToChunkPacket : Packet {
		public int ChunkX, ChunkY;
		bool Add;

		public AddToChunkPacket() { }

		public AddToChunkPacket(int chunkX, int chunkY, bool add) {
			ChunkX = chunkX;
			ChunkY = chunkY;
			Add = add;
		}

		public override void Read(NetworkReader reader) {
			ChunkX = reader.ReadInt32();
			ChunkY = reader.ReadInt32();
			Add = reader.ReadBoolean();
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(ChunkX);
			writer.WriteInt32(ChunkY);
			writer.WriteBoolean(Add);
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class UpdateFullChunkPacket : Packet {
		public int ChunkX;
		public short ChunkY;
		public int ChunkZ;
		public int Width, Height, Depth;
		public byte[] Data;

		public UpdateFullChunkPacket() { }

		public UpdateFullChunkPacket(int chunkX, short chunkY, int chunkZ, int width, int height, int depth, ChunkData data) {
			ChunkX = chunkX;
			ChunkY = chunkY;
			ChunkZ = chunkZ;
			Width = width;
			Height = height;
			Depth = depth;

			Data = new byte[width * height * depth * 5 / 2];
			data.Write(Data, 0, 0, 0, width, height, depth, 0); // TODO
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(ChunkX);
			writer.WriteInt16(ChunkY);
			writer.WriteInt32(ChunkZ);
			writer.WriteByte((byte)(Width - 1));
			writer.WriteByte((byte)(Height - 1));
			writer.WriteByte((byte)(Depth - 1));

			var compressed = new MemoryStream(Data.Length); // TODO: use better guess here
			var zlib = new ZOutputStream(compressed, zlibConst.Z_DEFAULT_COMPRESSION);
			zlib.Write(Data, 0, Data.Length);
			zlib.Flush();
			zlib.finish();
			writer.WriteInt32((int)zlib.TotalOut);
			writer.Write(compressed.GetBuffer(), 0, (int)zlib.TotalOut);
			zlib.Close();
			compressed.Close();
		}

		public override void Read(NetworkReader reader) {
			ChunkX = reader.ReadInt32();
			ChunkY = reader.ReadInt16();
			ChunkZ = reader.ReadInt32();
			Width = reader.ReadByte() + 1;
			Height = reader.ReadByte() + 1;
			Depth = reader.ReadByte() + 1;

			byte[] data = new byte[reader.ReadInt32()];
			reader.Read(data, 0, data.Length);
			var uncompressed = new MemoryStream(data.Length); // TODO: use better guess here
			var zlib = new ZOutputStream(uncompressed);
			zlib.Write(data, 0, data.Length);
			zlib.Flush();
			zlib.finish();
			Data = new byte[zlib.TotalOut];
			uncompressed.Read(Data, 0, Data.Length);
			zlib.Close();
			uncompressed.Close();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class UpdateBlockPacket : Packet {
		public int ChunkX;
		public byte ChunkY;
		public int ChunkZ;
		public byte Type;
		public byte MetaData;

		public UpdateBlockPacket() { }
		public UpdateBlockPacket(World world, int x, int y, int z) {
			ChunkX = x;
			ChunkY = (byte)y;
			ChunkZ = z;
			Type = (byte)world.GetBlockType(x, y, z);
			MetaData = (byte)world.GetBlockData(x, y, z);
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(ChunkX);
			writer.WriteByte(ChunkY);
			writer.WriteInt32(ChunkZ);
			writer.WriteByte(Type);
			writer.WriteByte(MetaData);
		}

		public override void Read(NetworkReader reader) {
			ChunkX = reader.ReadInt32();
			ChunkY = reader.ReadByte();
			ChunkZ = reader.ReadInt32();
			Type = reader.ReadByte();
			MetaData = reader.ReadByte();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class SendTileEntityAttributesPacket : Packet {
		public int X;
		public short Y;
		public int Z;
		public byte[] Data; // in NBT

		public SendTileEntityAttributesPacket() { }

		//public SendTileEntityAttributesPacket(TileEntity e) { // TODO: encode tile entity attributes as NBT }

		public override void Read(NetworkReader reader) {
			X = reader.ReadInt32();
			Y = reader.ReadInt16();
			Z = reader.ReadInt32();
			var dataLength = reader.ReadInt16();
			Data = new byte[dataLength];
			reader.Read(Data, 0, dataLength);
		}

		public override void Write(NetworkWriter writer) {
			throw new NotImplementedException();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	#endregion

	#region EntityPackets
	public class EntityInitializePacket : Packet {
		public int EntityId;

		public EntityInitializePacket() {}

		public EntityInitializePacket(int entityId) {
			EntityId = entityId;
		}

		public override void Write(NetworkWriter writer) {
			writer.WriteInt32(EntityId);
		}

		public override void Read(NetworkReader reader) {
			EntityId = reader.ReadInt32();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	public class EntityUpdateArmPacket : EntityInitializePacket {
		public bool ForwardAnimation;

		public EntityUpdateArmPacket() { }

		public override void Write(NetworkWriter writer) {
			base.Write(writer);
			writer.WriteBoolean(ForwardAnimation);
		}

		public override void Read(NetworkReader reader) {
			base.Read(reader);
			ForwardAnimation = reader.ReadBoolean();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	class EntityUpdateMovePacket : EntityInitializePacket {
		public byte XDiff, YDiff, ZDiff;

		public EntityUpdateMovePacket() { }

		public override void Write(NetworkWriter writer) {
			base.Write(writer);
			writer.WriteByte(XDiff);
			writer.WriteByte(YDiff);
			writer.WriteByte(ZDiff);
		}

		public override void Read(NetworkReader reader) {
			base.Read(reader);
			XDiff = reader.ReadByte();
			YDiff = reader.ReadByte();
			ZDiff = reader.ReadByte();
		}
	}

	class EntityUpdateLookPacket : EntityInitializePacket {
		public byte RotationX, RotationY;

		public EntityUpdateLookPacket() { }

		public override void Write(NetworkWriter writer) {
			base.Write(writer);
			writer.WriteByte(RotationX);
			writer.WriteByte(RotationY);
		}

		public override void Read(NetworkReader reader) {
			base.Read(reader);
			RotationX = reader.ReadByte();
			RotationY = reader.ReadByte();
		}
	}

	class EntityUpdateMoveLookPacket : EntityInitializePacket {
		public byte RotationX, RotationY;
		public byte XDiff, YDiff, ZDiff;

		public EntityUpdateMoveLookPacket() { }
		public EntityUpdateMoveLookPacket(NamedEntity entity) {
			EntityId = entity.Id;

		}

		public override void Write(NetworkWriter writer) {
			base.Write(writer);
			writer.WriteByte(XDiff);
			writer.WriteByte(YDiff);
			writer.WriteByte(ZDiff);
			writer.WriteByte(RotationX);
			writer.WriteByte(RotationY);
		}

		public override void Read(NetworkReader reader) {
			base.Read(reader);
			XDiff = reader.ReadByte();
			YDiff = reader.ReadByte();
			ZDiff = reader.ReadByte();
			RotationX = reader.ReadByte();
			RotationY = reader.ReadByte();
		}
	}

	class EntityTeleportPacket : EntityInitializePacket {
		public int X, Y, Z;
		public float RotationX, RotationY;

		public EntityTeleportPacket() { }
		public EntityTeleportPacket(Entity entity) {
			EntityId = entity.Id;
			X = (int)(entity.X * 32);
			Y = (int)(entity.Y * 32);
			Z = (int)(entity.Z * 32);
			RotationX = entity.RotationX;
			RotationY = entity.RotationY;
		}

		public override void Write(NetworkWriter writer) {
			base.Write(writer);
			writer.WriteInt32(X);
			writer.WriteInt32(Y);
			writer.WriteInt32(Z);
			writer.WriteAngle(RotationX);
			writer.WriteAngle(RotationY);
		}

		public override void Read(NetworkReader reader) {
			throw new NotImplementedException();
		}

		public override void Handle(PacketHandler handler) {
			handler.Handle(this);
		}
	}

	#endregion
}
