using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Survivalist {
	public class NetworkReader: BinaryReader {
		public NetworkReader(Stream stream) : base(stream) { }

		public string ReadUTF8() {
			int length = ReadInt16();
			return Encoding.UTF8.GetString(ReadBytes(length));
		}

		public override short ReadInt16() {
			var bytes = ReadBytes(2);
			return (short)((bytes[0] << 8) |
							bytes[1]);
		}

		public override Int32 ReadInt32() {
			var bytes = ReadBytes(4);
			return (bytes[0] << 24) |
				   (bytes[1] << 16) |
				   (bytes[2] << 8) |
				    bytes[3];
		}

		public override long ReadInt64() {
			return EndianUtil.SwapBitShift(base.ReadInt64());
			/*var bytes = ReadBytes(8);
			return (bytes[0] << 56) |
				   (bytes[1] << 48) |
				   (bytes[2] << 40) |
				   (bytes[3] << 32) |
				   (bytes[4] << 24) |
				   (bytes[5] << 16) |
				   (bytes[6] << 8) |
					bytes[7];*/
		}

		public override double ReadDouble() {
			return BitConverter.Int64BitsToDouble(ReadInt64());
		}

		public override float ReadSingle() {
			return EndianUtil.Int32BitsToSingle(ReadInt32());
		}

		public double ReadAngle() {
			var val = ReadByte();
			return val / 256.0 * 360.0;
		}
	}
}
