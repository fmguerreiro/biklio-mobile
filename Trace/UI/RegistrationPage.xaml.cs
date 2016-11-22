using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Trace {
	public partial class RegistrationPage : ContentPage {
		public RegistrationPage() {
			InitializeComponent();
		}

		async void OnRegister(object sender, EventArgs e) {
			var username = usernameText.Text;
			var email = emailText.Text;
			var password = passwordText.Text;
			var confirmPassword = confirmPasswordText.Text;

			if(username == null || email == null || password == null || confirmPassword == null) {
				await DisplayAlert("Error", "Please fill every field.", "Ok");
				return;
			}

			var client = new WebServerClient();
			WSSuccess result = await Task.Run(() => client.register(username: username, password: password, email: email));

			if(result.error == null) {
				User.AuthToken = result.token;
				User.Username = username;
				User.Password = password;
				User.Email = email;
				await DisplayAlert("Result", "Registration successful.", "Ok");
				await Navigation.PushAsync(new SignInPage());
			}
			else
				await DisplayAlert("Result", result.error, "Ok");
		}
	}
}
