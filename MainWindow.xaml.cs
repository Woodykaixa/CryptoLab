using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CryptoLab.CryptoComponent;

namespace CryptoLab {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
		}

		private void SetTitle(string title) {
			Title = "CryptoLab - " + title;
		}

		private void ShowCaesar(object sender, RoutedEventArgs e) {
			CryptoModule.Child = new Caesar();
			SetTitle("Caesar");
		}

		private void ShowDES(object sender, RoutedEventArgs e) {
			CryptoModule.Child = new DES();
			SetTitle("DES");
		}

		private void ShowMonoalphabetic(object sender, RoutedEventArgs e) {
			CryptoModule.Child = new Monoalphabetic();
			SetTitle("Monoalphabetic");
		}

		private void ShowRSA(object sender, RoutedEventArgs e) {
			CryptoModule.Child = new RSA();
			SetTitle("RSA");
		}
	}
}