using System;
using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// Page that handles new user registration.
	/// Requires Internet connection to verify the information with the Web Server.
	/// Stores the information on the device upon success for offline login later.
	/// </summary>
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
				await DisplayAlert(Language.Error, Language.FillEveryField, Language.Ok);
				return;
			}

			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.Register(username, password, email));

			if(result.success) {
				SQLiteDB.Instance.SaveUser(User.Instance = new User {
					Username = username,
					Email = email,
					// TODO verify if its needed -> AuthToken = result.token
				});
				DependencyService.Get<DeviceKeychainInterface>().SaveCredentials(username, password);
				await DisplayAlert(Language.Result, Language.RegistrationSuccessful, Language.Ok);
				await Navigation.PushAsync(new SignInPage());
			}
			else
				await DisplayAlert(Language.Result, result.error, Language.Ok);
		}
	}
}
