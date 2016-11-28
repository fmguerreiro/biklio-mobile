using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// Page used for checking user credentials and entering the application.
	/// </summary>
	public partial class SignInPage : ContentPage {

		private bool isRememberMe;

		public SignInPage() {
			InitializeComponent();
			isRememberMe = true;
			usernameText.Text = DependencyService.Get<DeviceKeychainInterface>().Username;
			passwordText.Text = DependencyService.Get<DeviceKeychainInterface>().Password;
		}

		async void OnLogin(object sender, EventArgs e) {
			var username = usernameText.Text;
			var password = passwordText.Text;

			if(username == null || password == null) {
				await DisplayAlert("Error", "Please fill every field.", "Ok");
				return;
			}

			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.loginWithCredentials(username, password));

			if(result.success) {

				// Remember me => Store credentials in keychain.
				if(isRememberMe)
					DependencyService.Get<DeviceKeychainInterface>().SaveCredentials(username, password);
				else
					DependencyService.Get<DeviceKeychainInterface>().DeleteCredentials();

				// Fetch user information from the database.
				SQLiteDB.Instance.InstantiateUser(username);
				await DisplayAlert("DEBUG: User", User.Instance.toString(), "Ok");

				await Navigation.PushAsync(new HomePage());
			}
			else
				await DisplayAlert("Error", result.error, "Ok");
		}

		void OnRememberMe(object sender, EventArgs e) {
			isRememberMe = !isRememberMe;
		}
	}
}