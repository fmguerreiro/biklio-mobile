using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
				User.AuthToken = null;
				User.Username = username;
				User.Password = password;
				if(isRememberMe) storeCredentials(); else removeCredentials();
				await Navigation.PushAsync(new HomePage());
			}
			else
				await DisplayAlert("Error", result.error, "Ok");
		}

		void OnRememberMe(object sender, EventArgs e) {
			isRememberMe = !isRememberMe;
		}

		void storeCredentials() {
			DependencyService.Get<StoreCredentialsInterface>().SaveCredentials(User.Username, User.Password);
		}

		void removeCredentials() {
			DependencyService.Get<StoreCredentialsInterface>().DeleteCredentials();
		}
	}
}