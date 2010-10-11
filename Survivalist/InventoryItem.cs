using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public enum InventoryType {
		Main = -1,
		Armor = -2, // TODO: not sure
		Crafting = -3 // TODO: not sure
	}

	public class InventoryItem {
		public int Id;
		public int Count = 0;
		public int Damage = 0;

		public InventoryItem(int id, int count = 1, int damage = 0) {
			Id = id;
			Count = count;
			Damage = damage;
		}
	}
}
