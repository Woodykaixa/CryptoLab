using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CryptoLab.Annotations;
using CryptoLab.Component;
using CryptoLab.Util;
using Microsoft.Win32;

namespace CryptoLab.CryptoComponent {
	/// <summary>
	/// DES.xaml 的交互逻辑
	/// </summary>
	public partial class DES {
		private bool isCbc;
		private bool is3Des;

		public DES() {
			InitializeComponent();
			isCbc = false;
			is3Des = false;
		}

		[DllImport("./Core/Core.DES.dll", EntryPoint = "DesEncrypt")]
		private static extern IntPtr DesEncrypt(byte[] plain, string key, IntPtr cipher);

		[DllImport("./Core/Core.DES.dll", EntryPoint = "DesDecrypt")]
		private static extern IntPtr DesDecrypt(byte[] cipher, string key, IntPtr plain);


		private bool CheckKey() {
			var key = KeyText.Text;
			var expectedLength = is3Des ? 16 : 8;
			if (key.Length != expectedLength) {
				MessageBox.Show($"密钥长度应当为{expectedLength}位，实际长度为{key.Length}", "密钥长度错误");
				return false;
			}

			foreach (var t in key.Where(t => t > 255)) {
				MessageBox.Show($"密钥仅支持ASCII字符。检测到非ASCII字符: {t}", "密钥编码错误");
				return false;
			}

			return true;
		}

		private bool CheckIv() {
			var iv = IVText.Text;
			if (iv.Length != 8) {
				MessageBox.Show($"IV长度应当为8位，实际长度为{iv.Length}", "IV长度错误");
				return false;
			}

			foreach (var t in iv.Where(t => t > 255)) {
				MessageBox.Show($"IV仅支持ASCII字符。检测到非ASCII字符: {t}", "IV编码错误");
				return false;
			}

			return true;
		}

		private bool CheckKeyAndIv() {
			if (!CheckKey()) {
				return false;
			}

			return !isCbc || CheckIv();
		}

		private static byte[] DesCallDllFunctionEcb(byte[] p1, string key,
			Func<byte[], string, IntPtr, IntPtr> dllFunc) {
			var obLen = p1.Length;
			if (obLen % 8 != 0) {
				obLen = p1.Length - p1.Length % 8 + 8;
			}

			var outBytes = new byte[obLen];
			var len = p1.Length;
			var bytes = new byte[8];
			var obIndex = 0;
			using var buffer = new AutoPtr(8);
			for (var i = 0; i < len; i += 8) {
				if (len - i < 8) {
					for (var j = 0; j < 8; j++) {
						bytes[j] = 0;
					}

					for (var j = i; j < len; j++) {
						bytes[j - i] = p1[j];
					}
				} else {
					for (var j = 0; j < 8; j++) {
						bytes[j] = p1[i + j];
					}
				}

				var ptr = dllFunc(bytes, key, buffer.Ptr);
				var tmp = new byte[8];
				Marshal.Copy(ptr, tmp, 0, 8);
				for (var j = 0; j < 8; j++) {
					outBytes[obIndex + j] = tmp[j];
				}

				obIndex += 8;
			}

			return outBytes;
		}

		private static byte[] DesCallDllFunctionCbc(byte[] p1, string key, string iv,
			Func<byte[], string, IntPtr, IntPtr> dllFunc, bool enc) {
			var ivBytes = Encoding.ASCII.GetBytes(iv);
			var obLen = p1.Length;
			if (obLen % 8 != 0) {
				obLen = p1.Length - p1.Length % 8 + 8;
			}

			var outBytes = new byte[obLen];
			var len = p1.Length;
			var inBytes = new byte[8];
			var obIndex = 0;
			using var buffer = new AutoPtr(8);
			for (var i = 0; i < len; i += 8) {
				if (len - i < 8) {
					for (var j = 0; j < 8; j++) {
						inBytes[j] = 0;
					}

					for (var j = i; j < len; j++) {
						inBytes[j - i] = p1[j];
					}
				} else {
					for (var j = 0; j < 8; j++) {
						inBytes[j] = p1[i + j];
					}
				}

				var back4dec = new byte[8];
				for (int j = 0; j < 8; j++) {
					back4dec[j] = inBytes[j];
				}

				if (enc) {
					for (var j = 0; j < 8; j++) {
						inBytes[j] ^= ivBytes[j];
					}
				}

				var ptr = dllFunc(inBytes, key, buffer.Ptr);
				var dllOutBytes = new byte[8];
				Marshal.Copy(ptr, dllOutBytes, 0, 8);
				if (!enc) {
					for (var j = 0; j < 8; j++) {
						dllOutBytes[j] ^= ivBytes[j];
					}
				}

				for (var j = 0; j < 8; j++) {
					outBytes[obIndex + j] = dllOutBytes[j];
					ivBytes[j] = enc
						? dllOutBytes[j]
						: back4dec[j];
				}

				obIndex += 8;
			}

			return outBytes;
		}

		private static byte[] DesCallDllFunction3Des(byte[] p1, string key,
			Func<byte[], string, IntPtr, IntPtr> dllFunc) {
			var key1 = key.Substring(0, 8);
			var key2 = key.Substring(8, 8);
			var obLen = p1.Length;
			if (obLen % 8 != 0) {
				obLen = p1.Length - p1.Length % 8 + 8;
			}

			var outBytes = new byte[obLen];
			var len = p1.Length;
			var bytes = new byte[8];
			var obIndex = 0;
			using var buffer = new AutoPtr(8);
			for (var i = 0; i < len; i += 8) {
				if (len - i < 8) {
					for (var j = 0; j < 8; j++) {
						bytes[j] = 0;
					}

					for (var j = i; j < len; j++) {
						bytes[j - i] = p1[j];
					}
				} else {
					for (var j = 0; j < 8; j++) {
						bytes[j] = p1[i + j];
					}
				}

				var ptr = dllFunc(bytes, key1, buffer.Ptr);
				var tmp = new byte[8];
				Marshal.Copy(ptr, tmp, 0, 8);
				ptr = dllFunc(tmp, key2, buffer.Ptr);
				Marshal.Copy(ptr, tmp, 0, 8);
				ptr = dllFunc(tmp, key1, buffer.Ptr);
				Marshal.Copy(ptr, tmp, 0, 8);

				for (var j = 0; j < 8; j++) {
					outBytes[obIndex + j] = tmp[j];
				}

				obIndex += 8;
			}

			return outBytes;
		}

		private void DesEncryptClick(object sender, RoutedEventArgs e) {
			if (!CheckKeyAndIv()) {
				return;
			}

			var plain = PlainInput.StrBytes;
			using var cipherTextBuffer = new AutoPtr(8);
			var begin = DateTime.Now;
			byte[] cipherOutBytes;
			if (isCbc) {
				cipherOutBytes = DesCallDllFunctionCbc(plain, KeyText.Text, IVText.Text,
					DesEncrypt, true);
			} else if (is3Des) {
				cipherOutBytes = DesCallDllFunction3Des(plain, KeyText.Text, DesEncrypt);
			} else {
				cipherOutBytes = DesCallDllFunctionEcb(plain, KeyText.Text, DesEncrypt);
			}
			var end = DateTime.Now;
			CipherInput.StrBytes = cipherOutBytes;
			MessageBox.Show($"加密用时: {end - begin}");
		}

		private void DesDecryptClick(object sender, RoutedEventArgs e) {
			if (!CheckKeyAndIv()) {
				return;
			}

			using var plainTextBuffer = new AutoPtr(8);
			var cipher = CipherInput.StrBytes;
			var begin = DateTime.Now;
			byte[] plainOutBytes;
			if (isCbc) {
				plainOutBytes = DesCallDllFunctionCbc(cipher, KeyText.Text, IVText.Text,
					DesDecrypt, false);
			} else if (is3Des) {
				plainOutBytes = DesCallDllFunction3Des(cipher, KeyText.Text, DesDecrypt);
			} else {
				plainOutBytes = DesCallDllFunctionEcb(cipher, KeyText.Text, DesDecrypt);
			}

			var end = DateTime.Now;
			PlainInput.StrBytes = plainOutBytes;
			MessageBox.Show($"解密用时: {end - begin}");
		}

		private void UseEcb(object sender, RoutedEventArgs e) {
			EcbButton.IsChecked = true;
			CbcButton.IsChecked = false;
			Des3Button.IsChecked = false;
			IVText.IsEnabled = false;
			isCbc = false;
			is3Des = false;
		}

		private void UseCbc(object sender, RoutedEventArgs e) {
			EcbButton.IsChecked = false;
			CbcButton.IsChecked = true;
			Des3Button.IsChecked = false;
			IVText.IsEnabled = true;
			isCbc = true;
			is3Des = false;
		}


		private void Use3Des(object sender, RoutedEventArgs e) {
			EcbButton.IsChecked = false;
			CbcButton.IsChecked = false;
			Des3Button.IsChecked = true;
			IVText.IsEnabled = false;
			isCbc = false;
			is3Des = true;
		}

		private void EncryptFile(object sender, RoutedEventArgs e) {
			if (!CheckKeyAndIv()) {
				return;
			}

			var dialog = new OpenFileDialog {
				Title = "选择待加密文件",
				Filter = "所有文件(*.*)|*.*"
			};
			var result = dialog.ShowDialog();
			if (!(result ?? false))
				return;
			var filename = dialog.FileName;

			using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));
			using var writer =
				new BinaryWriter(new FileStream(filename + ".des", FileMode.Create));
			var begin = DateTime.Now;
			var inBytes = reader.ReadBytes(8);
			while (inBytes.Length != 0) {
				byte[] outBytes;
				if (isCbc) {
					outBytes = DesCallDllFunctionCbc(inBytes, KeyText.Text, IVText.Text,
						DesEncrypt, true);
				} else if (is3Des) {
					outBytes = DesCallDllFunction3Des(inBytes, KeyText.Text, DesEncrypt);
				} else {
					outBytes = DesCallDllFunctionEcb(inBytes, KeyText.Text, DesEncrypt);
				}

				writer.Write(outBytes);
				inBytes = reader.ReadBytes(8);
			}

			var end = DateTime.Now;
			MessageBox.Show($"解密用时: {end - begin}");
		}

		private void DecryptFile(object sender, RoutedEventArgs e) {
			if (!CheckKeyAndIv()) {
				return;
			}

			var dialog = new OpenFileDialog {
				Title = "选择待解密文件",
				Filter = "已加密文件(*.*.des)|*.des"
			};
			var result = dialog.ShowDialog();
			if (!(result ?? false))
				return;
			var filename = dialog.FileName;

			using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));
			using var writer =
				new BinaryWriter(new FileStream(
					filename.Substring(0, filename.Length - 4) + ".dcpt",
					FileMode.Create));
			var begin = DateTime.Now;
			var inBytes = reader.ReadBytes(8);
			while (inBytes.Length != 0) {
				byte[] outBytes;
				if (isCbc) {
					outBytes = DesCallDllFunctionCbc(inBytes, KeyText.Text, IVText.Text,
						DesEncrypt, false);
				} else if (is3Des) {
					outBytes = DesCallDllFunction3Des(inBytes, KeyText.Text, DesDecrypt);
				} else {
					outBytes = DesCallDllFunctionEcb(inBytes, KeyText.Text, DesDecrypt);
				}

				writer.Write(outBytes);
				inBytes = reader.ReadBytes(8);
			}

			var end = DateTime.Now;
			MessageBox.Show($"解密用时: {end - begin}");
		}
	}
}