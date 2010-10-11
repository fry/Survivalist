using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class ItemEntity: Entity {
		public short TypeId;
		public byte Count;
		public short Durability;

		public override void OnPlayerTouched(Player player) {
			Console.WriteLine("touched item");
			player.World.EntityHandler.RemoveEntity(this);
			player.CollectItem(this);
		}
	}
}
