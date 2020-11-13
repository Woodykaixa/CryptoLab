// File: AutoPtr.cs - CryptoLab
// 
// Create Date: 2020-10-23 21:20
// Author Name: 初雨墨 [18074104]
// Copyright (c) 2020 Woodykaixa. All rights reserved.

using System;
using System.Runtime.InteropServices;

namespace CryptoLab.Util {
	public class AutoPtr : IDisposable {
		public IntPtr Ptr { get; private set; }

		public AutoPtr(int size) {
			Ptr = size > 0 ? MarshalHelper.Allocate(size) : IntPtr.Zero;
		}

		~AutoPtr() {
			if (Ptr == IntPtr.Zero)
				return;
			Marshal.FreeHGlobal(Ptr);
			Ptr = IntPtr.Zero;
		}

		public void Dispose() {
			Marshal.FreeHGlobal(Ptr);
			Ptr = IntPtr.Zero;
			GC.SuppressFinalize(this);
		}

		public static implicit operator IntPtr(AutoPtr p) {
			return p.Ptr;
		}
	}
}