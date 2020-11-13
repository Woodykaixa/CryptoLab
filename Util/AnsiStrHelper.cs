// File: AnsiStrHelper.cs - CryptoLab
// 
// Create Date: 2020-11-10 10:15
// Author Name: 初雨墨 [18074104]
// Copyright (c) 2020 Woodykaixa. All rights reserved.

using System;

namespace CryptoLab.Util {
	public static class AnsiStrHelper {
		public static string ToAnsiString(string raw) {
			var s = "";
			foreach (var t in raw) {
				if (t < 256) {
					s += t;
				} else {
					var twoChar = Convert.ToInt32(t);
					s += Convert.ToString((twoChar >> 8) & 0xff);
					s += Convert.ToString(twoChar & 0xff);
				}
			}

			return s;
		}
	}
}