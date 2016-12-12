using System;
using Xamarin.Forms;

namespace Trace {
	public partial class FirstPage : ContentPage {
		public FirstPage() {
			InitializeComponent();
		}

		async void OnRegister(object sender, EventArgs e) {
			await Navigation.PushAsync(new RegistrationPage());
		}

		async void OnLogin(object sender, EventArgs e) {
			await Navigation.PushAsync(new SignInPage());
		}

		async void OnSkip(object sender, EventArgs e) {
			User.Instance.Username = "test";
			SQLiteDB.Instance.InstantiateUser(User.Instance.Username, "test");
			await Navigation.PushAsync(new HomePage());
		}
	}
}
