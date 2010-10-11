using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Survivalist {
	public class EndianUtil {
		public static Int64 SwapBitShift(Int64 value) {
			UInt64 uvalue = (UInt64)value;
			UInt64 swapped = ((0x00000000000000FF) & (uvalue >> 56) |
							  (0x000000000000FF00) & (uvalue >> 40) |
							  (0x0000000000FF0000) & (uvalue >> 24) |
							  (0x00000000FF000000) & (uvalue >> 8)  |
							  (0x000000FF00000000) & (uvalue << 8)  |
							  (0x0000FF0000000000) & (uvalue << 24) |
							  (0x00FF000000000000) & (uvalue << 40) |
							  (0xFF00000000000000) & (uvalue << 56));
			return (Int64)swapped;
		}

		public static Int32 SwapBitShift(Int32 value) {
			UInt32 uvalue = (UInt32)value;
			UInt32 swapped = ((0x000000FF) & (uvalue >> 24) |
							  (0x0000FF00) & (uvalue >> 8) |
							  (0x00FF0000) & (uvalue << 8) |
							  (0xFF000000) & (uvalue << 24));
			return (Int32)swapped;
		}

		public static Int32 SingleToInt32Bits(float value) {
			return new Int32SingleUnion(value).AsInt32;
		}

		public static float Int32BitsToSingle(Int32 value) {
			return new Int32SingleUnion(value).AsSingle;
		}

		[StructLayout(LayoutKind.Explicit)]
		struct Int32SingleUnion {
			[FieldOffset(0)]
			int i;
			[FieldOffset(0)]
			float f;

			internal Int32SingleUnion(int i) {
				this.f = 0; // Just to keep the compiler happy
				this.i = i;
			}

			internal Int32SingleUnion(float f) {
				this.i = 0; // Just to keep the compiler happy
				this.f = f;
			}

			internal int AsInt32 {
				get { return i; }
			}

			internal float AsSingle {
				get { return f; }
			}
		}
	}
}
