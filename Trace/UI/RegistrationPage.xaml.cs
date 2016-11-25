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
			WSResult result = await Task.Run(() => client.register(username, password, email));

			if(result.error == null) {
				var database = new SQLiteDB();
				database.CreateNewUser(username, email, result.token);

				await DisplayAlert("Result", "Registration successful.", "Ok");
				await Navigation.PushAsync(new SignInPage());
			}
			else
				await DisplayAlert("Result", result.error, "Ok");
		}
	}
}
