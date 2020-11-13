using System;
using System.Collections.Generic;
using System.IO;
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
using CryptoLab.Util;
using Microsoft.Win32;

namespace CryptoLab.CryptoComponent {
	/// <summary>
	/// RSA.xaml 的交互逻辑
	/// </summary>
	public partial class RSA {
		private uint[] _keys;

		public RSA() {
			InitializeComponent();
			_keys = new uint[] {0, 0, 0};
		}

		[DllImport("./Core/Core.RSA.dll", EntryPoint = "RsaKeyGen")]
		private static extern IntPtr KeyGen(IntPtr keyArray);

		[DllImport("./Core/Core.RSA.dll", EntryPoint = "RsaEncrypt")]
		private static extern IntPtr RsaEncrypt(byte[] plain, int len, uint e, uint n,
			IntPtr cipher);

		[DllImport("./Core/Core.RSA.dll", EntryPoint = "RsaDecrypt")]
		private static extern IntPtr RsaDecrypt(byte[] cipher, int len, uint d, uint n,
			IntPtr plain);

		private void ImportKey(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog() {
				Title = "选择导入密钥",
				Filter = "公钥文件(*.pubkey)|*.pubkey|私钥文件(*.privkey)|*.privkey"
			};
			if (!(dialog.ShowDialog() ?? false)) {
				return;
			}

			var filename = dialog.FileName;
			using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));
			if (filename.EndsWith(".pubkey")) {
				_keys[0] = reader.ReadUInt32();
				_keys[1] = reader.ReadUInt32();
				PubKeyText.Text = _keys[0].ToString("X8") + _keys[1].ToString("X8");
			} else {
				_keys[0] = reader.ReadUInt32();
				_keys[2] = reader.ReadUInt32();
				PrivKeyText.Text = _keys[0].ToString("X8") + _keys[2].ToString("X8");
			}
		}


		private void ExportKeyToFile(string title, string filter, uint a, uint b) {
			var dialog = new SaveFileDialog() {Title = title, Filter = filter};
			if (!(dialog.ShowDialog() ?? false)) {
				return;
			}

			var filename = dialog.FileName;
			using var writer =
				new BinaryWriter(new FileStream(filename, FileMode.OpenOrCreate));
			writer.Write(a);
			writer.Write(b);
		}

		private void ExportKey(object sender, RoutedEventArgs e) {
			var noPub = _keys[1] == 0;
			var noPriv = _keys[2] == 0;

			if (_keys[0] == 0 || (noPriv && noPub)) {
				MessageBox.Show("未生成或导入密钥");
				return;
			}

			if (!noPub) {
				ExportKeyToFile("导出公钥", "公钥文件(*.pubkey)|*.pubkey", _keys[0], _keys[1]);
			}

			if (!noPriv) {
				ExportKeyToFile("导出私钥", "私钥文件(*.privkey)|*.privkey", _keys[0], _keys[2]);
			}
		}

		private void GenerateRsaKey(object sender, RoutedEventArgs e) {
			using var keyPtr = new AutoPtr(12);
			var keyArray = KeyGen(keyPtr.Ptr);
			for (var i = 0; i < 3; i++) {
				_keys[i] = (uint) Marshal.ReadInt32(keyArray, i * 4);
			}

			PubKeyText.Text = _keys[0].ToString("X8") + _keys[1].ToString("X8");

			PrivKeyText.Text = _keys[0].ToString("X8") + _keys[2].ToString("X8");
		}

		private bool CheckKey(int index) {
			return _keys[0] != 0 && _keys[index] != 0;
		}

		private byte[] EncryptByCallingDllFunc(byte[] plain, uint e, uint n) {
			var bufferLen = plain.Length;
			if (bufferLen % 3 != 0) {
				bufferLen += 3 - (bufferLen % 3);
			}

			bufferLen = bufferLen * 4 / 3;

			using var cipherBuffer = new AutoPtr(bufferLen);
			var ptr = RsaEncrypt(plain, plain.Length, e, n, cipherBuffer.Ptr);
			var cipherBytes = new byte[bufferLen];
			Marshal.Copy(ptr, cipherBytes, 0, bufferLen);
			return cipherBytes;
		}

		private byte[] DecryptByCallingDllFunc(byte[] cipher, uint d, uint n) {
			var bufferLen = cipher.Length * 3 / 4;

			using var plainBuffer = new AutoPtr(bufferLen);
			var ptr = RsaDecrypt(cipher, cipher.Length, d, n, plainBuffer.Ptr);
			var plainBytes = new byte[bufferLen];
			Marshal.Copy(ptr, plainBytes, 0, bufferLen);
			return plainBytes;
		}

		private void EncryptText(object sender, RoutedEventArgs e) {
			if (!CheckKey(1)) {
				MessageBox.Show("未生成或导入公钥，加密结束");
				return;
			}

			var begin = DateTime.Now;
			CipherInput.StrBytes =
				EncryptByCallingDllFunc(PlainInput.StrBytes, _keys[1], _keys[0]);
			var end = DateTime.Now;

			MessageBox.Show($"加密完成，用时: {end - begin}");
		}

		private void DecryptText(object sender, RoutedEventArgs e) {
			if (!CheckKey(2)) {
				MessageBox.Show("未生成或导入私钥，解密结束");
				return;
			}

			var begin = DateTime.Now;
			PlainInput.StrBytes =
				DecryptByCallingDllFunc(CipherInput.StrBytes, _keys[2], _keys[0]);
			var end = DateTime.Now;
			MessageBox.Show($"解密完成，用时: {end - begin}");
		}

		private void EncryptFile(object sender, RoutedEventArgs e) {
			if (!CheckKey(1)) {
				MessageBox.Show("未生成或导入公钥，加密结束");
				return;
			}

			var dialog = new OpenFileDialog() {
				Title = "选择文件",
				Filter = "所有文件(*.*)|*.*"
			};
			if (!(dialog.ShowDialog() ?? false)) {
				return;
			}

			var filename = dialog.FileName;
			using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));
			using var writer =
				new BinaryWriter(new FileStream(filename + ".rsaenc",
					FileMode.OpenOrCreate));
			var begin = DateTime.Now;
			var bytes = new byte[300];
			while (0 < reader.Read(bytes, 0, 300)) {
				var encryptedBytes = EncryptByCallingDllFunc(bytes, _keys[1], _keys[0]);
				writer.Write(encryptedBytes);
			}

			var end = DateTime.Now;
			MessageBox.Show($"加密完成，用时: {end - begin}");
		}

		private void DecryptFile(object sender, RoutedEventArgs e) {
			if (!CheckKey(2)) {
				MessageBox.Show("未生成或导入私钥，加密结束");
				return;
			}

			var dialog = new OpenFileDialog() {
				Title = "选择文件",
				Filter = "已加密文件(*.rsaenc)|*.rsaenc"
			};
			if (!(dialog.ShowDialog() ?? false)) {
				return;
			}

			var filename = dialog.FileName;
			using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));
			using var writer = new BinaryWriter(new FileStream(
				filename.Substring(0, filename.Length - 7) + ".rsadec",
				FileMode.OpenOrCreate));
			var begin = DateTime.Now;
			var bytes = new byte[400];
			while (0 < reader.Read(bytes, 0, 400)) {
				var decryptedBytes = DecryptByCallingDllFunc(bytes, _keys[2], _keys[0]);
				writer.Write(decryptedBytes);
			}

			var end = DateTime.Now;
			MessageBox.Show($"解密完成，用时: {end - begin}");
		}
	}
}