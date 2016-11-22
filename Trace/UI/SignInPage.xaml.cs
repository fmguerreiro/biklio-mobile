using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Trace {
	public partial class SignInPage : ContentPage {

		public SignInPage() {
			InitializeComponent();
		}

		async void OnLogin(object sender, EventArgs e) {
			var username = usernameText.Text;
			var password = passwordText.Text;

			if(username == null || password == null) {
				await DisplayAlert("Error", "Please fill every field.", "Ok");
				return;
			}

			var client = new WebServerClient();
			WSSuccess result = await Task.Run(() => client.loginWithCredentials(username, password));

			if(result.error == null) {
				User.AuthToken = result.token;
				User.Username = username;
				User.Password = password;
				await Navigation.PushAsync(new HomePage());
			}
			else
				await DisplayAlert("Result", result.error, "Ok");
		}
	}
}