using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Survivalist {
	public class Entity {
		public static int IdCounter = 0;
		public int Id = ++IdCounter;
		public double X, Y, Z;
		public float RotationX, RotationY;
		public double MoveX, MoveY, MoveZ;

		public bool Deleted = false;

		public static int EncodePosition(double pos) {
			return (int)Math.Floor(pos * 32);
		}

		public static byte EncodeAngle(double angle) {
			return (byte)Math.Floor(angle / 360.0 * 256);
		}

		public override int  GetHashCode() {
			return Id;
		}

		public override bool Equals(object obj) {
			if (obj is Entity)
				return Id == (obj as Entity).Id;

			return false;
		}

		public void Set(double x, double y, double z, float rotx, float roty) {
			X = x;
			Y = y;
			Z = z;
			RotationX = rotx;
			RotationY = roty;

		}

		public double Distance(double x, double y, double z) {
			double dX = X - x;
			double dY = Y - y;
			double dZ = Z - z;
			return dX * dX + dY * dY + dZ * dZ;
		}

		public virtual void OnPlayerTouched(Player player) {
			Console.WriteLine("touched " + Id);
		}
	}
}
