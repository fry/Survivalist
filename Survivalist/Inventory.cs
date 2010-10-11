using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class Inventory {
		public InventoryItem[] Main = new InventoryItem[37];
		public InventoryItem[] Armor = new InventoryItem[4];
		public InventoryItem[] Crafting = new InventoryItem[4];
		int activeItem = 0;

		public InventoryItem ActiveItem {
			get {
				return Main[activeItem];
			}
		}

		public void SetActiveItem(int index) {
			activeItem = index % Main.Length;
		}
	}
}
