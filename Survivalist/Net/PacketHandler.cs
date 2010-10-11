using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class PacketHandler {
		public virtual void Handle(Packet packet) {
		}

		public virtual void Handle(LoginPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(VerifyPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(PlayerMovePacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(PlayerLookPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(PlayerMoveLookPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(ChatPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(DisconnectPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(PlayerDigPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void Handle(PlayerPlaceBlockPacket packet) {
			Handle(packet as Packet);
		}

		public virtual void OnError(string reason) {
		}
	}
}
