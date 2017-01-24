using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Plugin.Geolocator;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// This class handles online login with the WebServer in the background after the user has passed offline login.
	/// </summary>
	public static class WebServerLoginManager {

		public static bool IsOfflineLoggedIn { get; set; }
		public static bool IsLoginVerified { get; set; }

		public static async Task TryLogin(bool isCredentialsLogin) {
			IsOfflineLoggedIn = true;
			if(CrossConnectivity.Current.IsConnected) {
				await Task.Run(() => OnConnectivityChanged(isCredentialsLogin, new ConnectivityChangedEventArgs() { IsConnected = true }));
			}
		}

		/// <summary>
		/// Event handler that performs operations when the user gets Internet connection.
		/// Performs credentials verification and sends KPIs if they were not done so previously.
		/// </summary>
		public static async void OnConnectivityChanged(object isCrendentialsLogin, ConnectivityChangedEventArgs e) {
			Debug.WriteLine("OnConnectivityChanged() - IsLoginVerified: " + IsLoginVerified + " and isOfflineLoggedIn: " + IsOfflineLoggedIn);
			if(e.IsConnected) {
				var isWSReachable = await CrossConnectivity.Current.IsRemoteReachable(WebServerConstants.HOST, WebServerConstants.PORT, 5000);
				if(isWSReachable) {
					// Check login info.
					if(!IsLoginVerified && IsOfflineLoggedIn) {
						Debug.WriteLine("LoginManager: Verifying login.");
						bool? result = false;
						if((bool) isCrendentialsLogin)
							result = await Task.Run(() => LoginWithCredentials());
						else
							result = await Task.Run(() => LoginWithToken());

						if(result ?? false) {
							IsLoginVerified = true;
							// Upon successful login, send available KPIs to the server.
							new WebServerClient().SendKPIs(User.Instance.GetFinishedKPIs()).DoNotAwait();
						}
						else if(!result ?? false) {
							// If user credentials were wrong, send the user to sign in screen and show error message.
							Device.BeginInvokeOnMainThread(async () => {
								await Application.Current.MainPage.DisplayAlert(Language.Error, Language.OnlineLoginError, Language.Ok);
								PrepareLogout();

								Application.Current.MainPage = SignInPage.CreateSignInPage();
							});
							return;
						}
						else if(result == null) {
							// Could not communicate with the server. Maybe it's down, allow user to proceed offline.
							IsLoginVerified = true;
						}
					}
					else {
						// If user was already logged in, send KPIs to the server.
						new WebServerClient().SendKPIs(User.Instance.GetFinishedKPIs()).DoNotAwait();
					}
				}
			}
			//Debug.WriteLine(string.Format("Connectivity Changed - IsConnected: {0}", e.IsConnected));
		}


		/// <summary>
		/// Login with the Web Server using the username and password credentials.
		/// </summary>
		/// <returns>The user credentials.</returns>
		public static async Task<bool?> LoginWithCredentials() {

			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.LoginWithCredentials(User.Instance.Username, User.Instance.Password));

			if(result.success) {
				DependencyService.Get<ICredentialsStore>().SaveCredentials(User.Instance.Username, User.Instance.Password);

				User.Instance.SessionToken = result.payload.token;
				SQLiteDB.Instance.SaveUser(User.Instance);
			}
			else if(result.error.StartsWith("404", StringComparison.Ordinal)) {
				return null;
			}
			Debug.WriteLine($"1 -> {result.success}");
			return result.success;
		}


		/// <summary>
		/// Login with the Web Server using the access token obtained from the OAuth providers.
		/// </summary>
		/// <returns>The oauth token.</returns>
		public static async Task<bool?> LoginWithToken() {

			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.LoginWithToken(User.Instance.IDToken));

			if(result.success) {
				User.Instance.Email = result.payload.email;
				User.Instance.SessionToken = result.payload.token;
				// Facebook login obtains personal user information from the WS.
				if(OAuthConfigurationManager.Type.Equals("facebook")) {
					User.Instance.Name = result.payload.name;
					User.Instance.PictureURL = result.payload.picture;
				}
				SQLiteDB.Instance.SaveUser(User.Instance);
			}
			// If the problem lies with the WS, return null to let the user continue using the app.
			else if(result.error.StartsWith("404", StringComparison.Ordinal)) {
				return null;
			}
			// Google replaces its token periodically, so the webserver login will fail unless we ask for a new one.
			else if(OAuthConfigurationManager.Type.Equals("google")) {
				try {
					// Fetch the user's current navigation page to push the google login page onto the stack.
					var mainPage = ((MainPage) Application.Current.MainPage).Detail as NavigationPage;
					mainPage = mainPage ?? Application.Current.MainPage as NavigationPage;
					//Debug.WriteLine($"LoginManager.LoginWithToken() changing main page from {Application.Current.MainPage} to {((MainPage) Application.Current.MainPage).Detail}");
					if(mainPage != null) {
						DependencyService.Get<ICredentialsStore>().DeleteCredentials(User.Instance.Username);
						Device.BeginInvokeOnMainThread(async () => {
							await mainPage.PushAsync(new GoogleOAuthUIPage());
						});
						return null;
					}
				}
				catch(Exception e) { Debug.WriteLine(e); }
			}
			return result.success;
		}



		public static void PrepareLogout() {
			User.Instance = null;
			RewardEligibilityManager.Instance = null;
			IsLoginVerified = IsOfflineLoggedIn = false;
			CrossGeolocator.Current.StopListeningAsync().DoNotAwait();
			DependencyService.Get<IMotionActivityManager>().StopMotionUpdates();
			DependencyService.Get<ICredentialsStore>().DeleteAllCredentials();
			AutoLoginManager.MostRecentLoginType = LoginType.None;
		}
	}
}
