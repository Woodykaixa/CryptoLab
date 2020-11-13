using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace CryptoLab.Component {
	/// <summary>
	/// StrHexInputBox.xaml 的交互逻辑
	/// </summary>
	public partial class StrHexInputBox {
		private bool _enableTA;

		private byte[] _actualValue;

		public StrHexInputBox() {
			InitializeComponent();
			_actualValue = new byte[8];
			_enableTA = true;
		}


		private void Update() {
			TextArea.Text = Encoding.UTF8.GetString(_actualValue);
			HexArea.Text =
				_actualValue.Aggregate("", (current, ch) => current + ch.ToString("X2")+' ');
		}

		public byte[] StrBytes {
			get => _actualValue;
			set {
				_actualValue = value;
				_enableTA = false;
				Update();
				_enableTA = true;
			}
		}

		public string Label {
			get => NameLabel.Content.ToString();
			set => NameLabel.Content = value;
		}

		private void TextArea_OnTextChanged(object sender, TextChangedEventArgs e) {
			if (!_enableTA)
				return;
			StrBytes = Encoding.UTF8.GetBytes(TextArea.Text);
		}
	}
}