using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Auth;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// Page used for checking user credentials and entering the application.
	/// </summary>
	public partial class SignInPage : ContentPage {

		private bool isRememberMe;
		static INavigation navigation;


		public SignInPage() {
			InitializeComponent();
			navigation = Navigation;
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
			WSResult result = await Task.Run(() => client.LoginWithCredentials(username, password));

			if(result.success) {

				// Remember me => Store credentials in keychain.
				if(isRememberMe)
					DependencyService.Get<DeviceKeychainInterface>().SaveCredentials(username, password);
				else
					DependencyService.Get<DeviceKeychainInterface>().DeleteCredentials();

				// Fetch user information from the database.
				SQLiteDB.Instance.InstantiateUser(username);

				await Navigation.PushAsync(new HomePage());
			}
			else
				await DisplayAlert("Error", result.error, "Ok");
		}


		void OnGoogleLogin(object sender, EventArgs e) {
			// Use a custom renderer to display the Google auth UI
			OAuthConfigurationManager.SetConfig(new GoogleOAuthConfig());
			Navigation.PushModalAsync(new OAuthUIPage());
		}


		void OnFacebookLogin(object sender, EventArgs e) {
			// Use a custom renderer to display the Facebook auth UI
			OAuthConfigurationManager.SetConfig(new FacebookOAuthConfig());
			Navigation.PushModalAsync(new OAuthUIPage());
		}


		public static Action SuccessfulOAuthLoginAction {
			get {
				return new Action(() => {
					navigation.PopModalAsync();
					// Is user logged in?
					if(!string.IsNullOrEmpty(User.Instance.Email)) {
						navigation.InsertPageBefore(new HomePage(), navigation.NavigationStack.First());
						navigation.PopToRootAsync();
					}
				});
			}
		}


		void OnRememberMe(object sender, EventArgs e) {
			isRememberMe = !isRememberMe;
		}
	}
}