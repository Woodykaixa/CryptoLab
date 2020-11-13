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
	internal class CaesarVm : INotifyPropertyChanged {
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

		public CaesarVm() {
			_cipher = "";
			PlainTxt = "";
			Key = "0";
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged(
			[CallerMemberName] string propertyName = null) {
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	/// <summary>
	/// Caesar.xaml 的交互逻辑
	/// </summary>
	public partial class Caesar {
		private readonly CaesarVm _vm;
		private static string _alphabet = "abcdefghijklmnopqrstuvwxyz";
		private static char[] _pChars = _alphabet.ToCharArray();
		private static char[] _cChars = _alphabet.ToUpper().ToCharArray();

		public Caesar() {
			InitializeComponent();
			_vm = DataContext as CaesarVm;
			MakeTable(0);
		}

		[DllImport("./Core/Core.Caesar.dll", EntryPoint = "CaesarEncrypt")]
		private static extern IntPtr CaesarEncrypt(string plain, int textLen,
			IntPtr cipher, int key);

		[DllImport("Core.Caesar.dll", EntryPoint = "CaesarDecrypt")]
		private static extern IntPtr CaesarDecrypt(string cipher, int cipherLen,
			IntPtr plain, int key);

		private void EncryptButtonClick(object sender, RoutedEventArgs e) {
			var p = _vm.PlainTxt ?? "";
			using var cipherTextBuffer = new AutoPtr(p.Length * 2);
			var ptr = CaesarEncrypt(p, p.Length, cipherTextBuffer.Ptr,
				int.Parse(KeyValue.Text) % 26);
			_vm.Cipher = Marshal.PtrToStringAnsi(ptr);
		}

		private void DecryptButtonClick(object sender, RoutedEventArgs e) {
			var c = _vm.Cipher ?? "";
			using var plainTextBuffer = new AutoPtr(c.Length * 2);
			var ptr = CaesarDecrypt(c, c.Length, plainTextBuffer.Ptr,
				int.Parse(KeyValue.Text) % 26);
			_vm.PlainTxt = Marshal.PtrToStringAnsi(ptr);
		}

		private void MakeTable(int key) {
			var newTable = new StackPanel[26];
			for (var i = 0; i < 26; i++) {
				var m = new Label() {
					Content = _pChars[i]
				};
				var c = new Label() {
					Content = _cChars[(i + key + 26) % 26]
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

		private void IncreaseKeyValue(object sender, RoutedEventArgs e) {
			var key = int.Parse(_vm.Key);
			MakeTable(++key);
			_vm.Key = key.ToString();
		}

		private void DecreaseKeyValue(object sender, RoutedEventArgs e) {
			var key = int.Parse(_vm.Key);
			MakeTable(--key);
			_vm.Key = key.ToString();
		}
	}
}