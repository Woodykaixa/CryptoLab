using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using CryptoLab.Util;

namespace CryptoLab.CryptoComponent {
	internal class MonoVm : INotifyPropertyChanged {
		private string _cipher;
		private string _plain;
		private string _key;

		public string Cipher {
			get => _cipher;
			set {
				_cipher = value;
				OnPropertyChanged();
			}
		}

		public string PlainTxt {
			get => _plain;
			set {
				_plain = value;
				OnPropertyChanged();
			}
		}

		public string Key {
			get => _key;
			set {
				_key = value;
				OnPropertyChanged();
			}
		}

		public MonoVm() {
			Cipher = "";
			PlainTxt = "";
			Key = "";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(
			[CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	/// <summary>
	/// Monoalphabetic.xaml 的交互逻辑
	/// </summary>
	public partial class Monoalphabetic {
		private readonly MonoVm _vm;

		public Monoalphabetic() {
			InitializeComponent();
			_vm = DataContext as MonoVm;
			MakeTable(_vm.Key);
		}

		private static readonly char[] Alphabet = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

		[DllImport("./Core/Core.Monoalphabetic.dll", EntryPoint = "UseKeyword")]
		private static extern IntPtr UseKeyword(string keyword, int len, IntPtr outKey);

		[DllImport("./Core/Core.Monoalphabetic.dll",
			EntryPoint = "MonoalphabeticEncrypt")]
		private static extern IntPtr MonoalphabeticEncrypt(string plain, int pLen,
			string keyword, int kLen, IntPtr cipher);

		[DllImport("./Core/Core.Monoalphabetic.dll",
			EntryPoint = "MonoalphabeticDecrypt")]
		private static extern IntPtr MonoalphabeticDecrypt(string cipher, int pLen,
			string keyword, int kLen, IntPtr plain);

		private void EncryptClick(object sender, RoutedEventArgs e) {
			var plain = _vm.PlainTxt ?? "";
			using var cipherBuffer = new AutoPtr(plain.Length);
			var ptr = MonoalphabeticEncrypt(plain, plain.Length, _vm.Key,
				_vm.Key.Length, cipherBuffer);
			_vm.Cipher =
				MarshalHelper.ParseStringFromPtr(ptr, plain.Length, Encoding.ASCII);
		}

		private void DecryptClick(object sender, RoutedEventArgs e) {
			var cipher = _vm.Cipher ?? "";
			using var plainBuffer = new AutoPtr(cipher.Length);
			var ptr = MonoalphabeticDecrypt(cipher, cipher.Length, _vm.Key,
				_vm.Key.Length, plainBuffer);
			_vm.PlainTxt =
				MarshalHelper.ParseStringFromPtr(ptr, cipher.Length, Encoding.ASCII);
		}

		private void MakeTable(string key) {
			using var keyPtr = new AutoPtr(26);
			var ptr = UseKeyword(key, key.Length, keyPtr.Ptr);
			var keyStr = Marshal.PtrToStringAnsi(ptr);
			var newTable = new StackPanel[26];
			for (var i = 0; i < 26; i++) {
				var m = new Label() {
					Content = Alphabet[i]
				};
				var c = new Label() {
					Content = keyStr[i]
				};
				newTable[i] = new StackPanel();
				newTable[i].Children.Add(m);
				newTable[i].Children.Add(c);
			}

			Table.Children.Clear();
			foreach (var t in newTable) {
				Table.Children.Add(t);
			}
		}

		private void OnKeyValueChange(object sender, TextChangedEventArgs e) {
			if (sender is TextBox tb) {
				_vm.Key = tb.Text;
			}

			MakeTable(_vm.Key);
		}
	}
}