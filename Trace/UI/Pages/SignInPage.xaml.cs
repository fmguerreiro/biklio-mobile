using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Trace.Localization;

namespace Trace {
	/// <summary>
	/// Page used for checking user credentials and entering the application.
	/// </summary>
	public partial class SignInPage : ContentPage {

		public SignInPage() {
			InitializeComponent();
			string username = null;
			usernameText.Text = username = DependencyService.Get<ICredentialsStore>().Username;
			passwordText.Text = DependencyService.Get<ICredentialsStore>().GetPassword(usernameText.Text);
			if(username != null) {
				tosSwitch.IsToggled = true;
			}

			// Add tap event handlers to the labels so the user can click on them.
			var registerGR = new TapGestureRecognizer();
			registerGR.Tapped += onRegister;
			registerLabel.GestureRecognizers.Add(registerGR);

			var checkToSGR = new TapGestureRecognizer();
			checkToSGR.Tapped += onCheckTos;
			tosWarningLabel.GestureRecognizers.Add(checkToSGR);
		}


		void onUsernameInput(object sender, EventArgs e) {
			passwordText.Text = DependencyService.Get<ICredentialsStore>().GetPassword(((Entry) sender).Text);
		}


		async void onLogin(object sender, EventArgs e) {
			var username = usernameText.Text;
			var password = passwordText.Text;

			if(!tosSwitch.IsToggled) {
				await DisplayAlert(Language.Error, Language.ToSUncheckedError, Language.Ok);
				return;
			}

			if(username == null || password == null) {
				await DisplayAlert(Language.Error, Language.FillEveryField, Language.Ok);
				return;
			}

			// Perform credentials validation against the local keystore/keychain.
			bool doesUsernameExist = DependencyService.Get<ICredentialsStore>().Exists(username);
			string storedPassword = DependencyService.Get<ICredentialsStore>().GetPassword(username);
			bool doesPasswordMatch = storedPassword != null && storedPassword.Equals(password);
			if(!doesUsernameExist || !doesPasswordMatch) {
				await DisplayAlert(Language.Warning, Language.LocalIncorrectCredentialsWarning, Language.Yes, Language.No);
				await LoginManager.TryLogin(isCredentialsLogin: true);
				if(!LoginManager.IsLoginVerified) {
					await DisplayAlert(Language.Error, Language.LoginError, Language.Ok);
					return;
				}
			}

			// Remember me => Store credentials in keychain.
			DependencyService.Get<ICredentialsStore>().SaveCredentials(username, password);

			// Fetch user information from the database.
			SQLiteDB.Instance.InstantiateUser(username);
			//SQLiteDB.Instance.SaveItem(User.Instance);

			// Used later for background login when user has internet connection.
			User.Instance.Password = password;

			LoginManager.TryLogin(isCredentialsLogin: true).DoNotAwait();

			// Record login event.
			User.Instance.GetCurrentKPI().AddLoginEvent(TimeUtil.CurrentEpochTimeSeconds());

			//await Task.Delay(1000);
			Application.Current.MainPage = new MainPage();
		}


		// Use a custom renderer to display the Google auth UI
		async void onGoogleLogin(object sender, EventArgs e) {
			if(!tosSwitch.IsToggled) {
				await DisplayAlert(Language.Error, Language.ToSUncheckedError, Language.Ok);
				return;
			}

			OAuthConfigurationManager.SetConfig(new GoogleOAuthConfig());
			await Navigation.PushAsync(new GoogleOAuthUIPage());
		}


		// Use a custom renderer to display the Facebook auth UI
		async void onFacebookLogin(object sender, EventArgs e) {
			if(!tosSwitch.IsToggled) {
				await DisplayAlert(Language.Error, Language.ToSUncheckedError, Language.Ok);
				return;
			}

			OAuthConfigurationManager.SetConfig(new FacebookOAuthConfig());
			await Navigation.PushAsync(new FacebookOAuthUIPage());
		}


		void onRegister(object sender, EventArgs e) {
			Navigation.PushModalAsync(new RegistrationPage());
		}


		/// <summary>
		/// If the OAuthLogin is successful, finish the login by sending the token to Web Server.
		/// </summary>
		public static Action SuccessfulOAuthLoginAction {
			get {
				return new Action(async () => {

					// Leave OAuth provider page.
					//NavPage.PopModalAsync();

					//Application.Current.MainPage = new MainPage();
					//Debug.WriteLine("ModalPage after OAuth: " + NavPage.ModalStack.FirstOrDefault().GetType().Name);
					//Debug.WriteLine("Page after OAuth: " + NavPage.NavigationStack.FirstOrDefault().GetType().Name);

					// Set root page as MainPage, so user can't use Back button to leave app.
					//NavPage.InsertPageBefore(new MainPage(), NavPage.NavigationStack.First());

					// Remove all pages from the navigation stack but the MainPage.
					//NavPage.PopToRootAsync();

					// HACK Spent 4 hours trying to get this to work. I gave up. 
					// If you remove this, the Android app crashes at this point. Seems to be an issue with Xamarin.Auth.GetUI().
					await Task.Delay(1000);
					// Record login event.
					Device.BeginInvokeOnMainThread(() => {
						Application.Current.MainPage = new MainPage();
						LoginManager.TryLogin(isCredentialsLogin: false).DoNotAwait();
						User.Instance.GetCurrentKPI().AddLoginEvent(TimeUtil.CurrentEpochTimeSeconds());
					});
				});
			}
		}


		/// <summary>
		/// If the OAuthLogin is unsuccessful, simply come back to sign-in page.
		/// </summary>
		public static Action UnsuccessfulOAuthLoginAction {
			get {
				return new Action(async () => {
					await Task.Delay(1000);
					await LoginManager.PrepareLogout();
					await Application.Current.MainPage.DisplayAlert(Language.Error, Language.OAuthError, Language.Ok);

					var signInPage = new NavigationPage(new SignInPage());
					signInPage.BarBackgroundColor = (Color) App.Current.Resources["PrimaryColor"];
					signInPage.BarTextColor = (Color) App.Current.Resources["PrimaryTextColor"];
					Application.Current.MainPage = signInPage;
				});
			}
		}


		// TODO use ToS page instead of Privacy policy!!
		void onCheckTos(object sender, EventArgs e) {
			Navigation.PushModalAsync(new PrivacyPolicyPage());
		}


		void onAgreeToToS(object sender, EventArgs e) {
			//doesUserAgreeToS = !doesUserAgreeToS;
		}

	}
}