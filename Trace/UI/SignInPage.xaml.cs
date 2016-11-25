using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;

namespace Trace {
	public partial class SignInPage : ContentPage {

		private bool isRememberMe;

		public SignInPage() {
			InitializeComponent();
			isRememberMe = false;
			usernameText.Text = DependencyService.Get<StoreCredentialsInterface>().Username;
			passwordText.Text = DependencyService.Get<StoreCredentialsInterface>().Password;
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

			if(result.error == null) {

				if(isRememberMe)
					storeCredentials(username, password);
				else
					removeCredentials();

				var database = new SQLiteDB();
				database.InstantiateUser(username);

				await Navigation.PushAsync(new HomePage());
			}
			else
				await DisplayAlert("Error", result.error, "Ok");
		}

		void OnRememberMe(object sender, EventArgs e) {
			isRememberMe = !isRememberMe;
		}

		void storeCredentials(string username, string password) {
			DependencyService.Get<StoreCredentialsInterface>().SaveCredentials(username, password);
		}

		void removeCredentials() {
			DependencyService.Get<StoreCredentialsInterface>().DeleteCredentials();
		}
	}
}