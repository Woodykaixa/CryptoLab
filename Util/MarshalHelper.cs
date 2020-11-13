using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CryptoLab.Util {
	internal static class MarshalHelper {
		public static IntPtr Allocate(int size) {
			var ptr = Marshal.AllocHGlobal(size);
			var init = new byte[size];
			for (var i = 0; i < size; i++) {
				init[i] = 0;
			}

			Marshal.Copy(init, 0, ptr, size);
			return ptr;
		}

		public static string ParseStringFromPtr(IntPtr ptr, int bitLength, Encoding encoding) {
			var tmp = new byte[bitLength];
			Marshal.Copy(ptr,tmp,0,bitLength);
			return encoding.GetString(tmp);
		}
	}
}