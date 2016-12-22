using System;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// First page of the app, showing the logo and the logic for the user to sign into the application.
	/// </summary>
	public partial class StartPage : ContentPage {
		public StartPage() {
			InitializeComponent();
		}

		async void OnRegister(object sender, EventArgs e) {
			await Navigation.PushAsync(new RegistrationPage());
		}

		async void OnLogin(object sender, EventArgs e) {
			await Navigation.PushAsync(new SignInPage());
		}

		//async void OnSkip(object sender, EventArgs e) {
		//	User.Instance.Username = "test";
		//	SQLiteDB.Instance.InstantiateUser(User.Instance.Username, "test");
		//	await Navigation.PushAsync(new HomePage());
		//}
	}
}
