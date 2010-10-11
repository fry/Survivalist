using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class NamedEntity: Entity {
		public string Name = "<UNKNOWN>";

		public double Stance;
		public Inventory Inventory = new Inventory();

		public NamedEntity(string name) {
			Name = name;
		}

		public void SetPosition(double x, double y, double stance, double z, float rotX, float rotY) {
			X = x;
			Y = y;
			Stance = stance;
			Z = z;
			RotationX = rotX;
			RotationY = rotY;
		}
	}
}
