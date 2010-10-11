using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Survivalist {
	public class NetworkWriter: BinaryWriter {
		public NetworkWriter(Stream stream) : base(stream) { }

		public void WriteUTF8(string str) {
			var length = Encoding.UTF8.GetByteCount(str);
			WriteByte((byte)((length >> 8) & 0xFF));
			WriteByte((byte)(length & 0xFF));
			Write(Encoding.UTF8.GetBytes(str));
		}

		public void WriteInt16(short num) {
			WriteByte((byte)((num >> 8) & 0xFF));
			WriteByte((byte)(num & 0xFF));
		}

		public void WriteInt32(Int32 num) {
			WriteByte((byte)((num >> 24) & 0xFF));
			WriteByte((byte)((num >> 16) & 0xFF));
			WriteByte((byte)((num >> 8) & 0xFF));
			WriteByte((byte)(num & 0xFF));
		}

		public void WriteInt64(Int64 num) {
			//base.Write((Int64)EndianUtil.SwapBitShift(num));
			WriteByte((byte)((num >> 56) & 0xFF));
			WriteByte((byte)((num >> 48) & 0xFF));
			WriteByte((byte)((num >> 40) & 0xFF));
			WriteByte((byte)((num >> 32) & 0xFF));
			WriteByte((byte)((num >> 24) & 0xFF));
			WriteByte((byte)((num >> 16) & 0xFF));
			WriteByte((byte)((num >> 8) & 0xFF));
			WriteByte((byte)(num & 0xFF));
		}

		public void WriteDouble(double val) {
			WriteInt64(BitConverter.DoubleToInt64Bits(val));
		}

		public void WriteSingle(float val) {
			WriteInt32(EndianUtil.SingleToInt32Bits(val));
		}

		public void WriteBoolean(bool val) {
			Write(val);
		}

		public void WriteByte(byte val) {
			Write(val);
		}

		public void WriteAngle(double val) {
			WriteByte((byte)(val / 360.0 * 256));
		}
	}
}
