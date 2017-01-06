using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Plugin.Geolocator;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public static class LoginManager {

		public static bool IsOfflineLoggedIn { get; set; }
		public static bool IsLoginVerified { get; set; }
		public static bool IsRememberMe { get; set; }

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
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		public static async void OnConnectivityChanged(object isCrendentialsLogin, ConnectivityChangedEventArgs e) {
			Debug.WriteLine("OnConnectivityChanged() - IsLoginVerified: " + IsLoginVerified + " and isOfflineLoggedIn: " + IsOfflineLoggedIn);
			if(e.IsConnected) {
				var isWSReachable = await CrossConnectivity.Current.IsRemoteReachable(WebServerConstants.HOST, WebServerConstants.PORT, 5000);
				if(isWSReachable) {
					// Check login info.
					if(!IsLoginVerified && IsOfflineLoggedIn) {
						Debug.WriteLine("LoginManager: Verifying login.");
						bool result = false;
						if((bool) isCrendentialsLogin)
							result = await Task.Run(() => LoginWithCredentials());
						else
							result = await Task.Run(() => LoginWithToken());

						if(result) {
							IsLoginVerified = true;
							// Upon successful login, send available KPIs to the server.
							new WebServerClient().SendKPIs(User.Instance.GetFinishedKPIs()).DoNotAwait();
						}
						else {
							// If user credentials were wrong, send the user to sign in screen and show error message.
							Device.BeginInvokeOnMainThread(async () => {
								await Application.Current.MainPage.DisplayAlert(Language.Error, Language.OnlineLoginError, Language.Ok);
								DependencyService.Get<DeviceKeychainInterface>().DeleteCredentials(User.Instance.Username);
								await PrepareLogout();
								Application.Current.MainPage = new NavigationPage(new SignInPage());
							});
							return;
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


		public static async Task<bool> LoginWithCredentials() {

			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.LoginWithCredentials(User.Instance.Username, User.Instance.Password));

			if(result.success) {
				if(IsRememberMe)
					DependencyService.Get<DeviceKeychainInterface>().SaveCredentials(User.Instance.Username, User.Instance.Password);
				else
					DependencyService.Get<DeviceKeychainInterface>().DeleteCredentials(User.Instance.Username);

				User.Instance.SessionToken = result.payload.token;
				SQLiteDB.Instance.SaveUser(User.Instance);
			}
			return result.success;
		}


		public static async Task<bool> LoginWithToken() {

			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.LoginWithToken(User.Instance.IDToken));

			if(result.success) {
				User.Instance.Name = result.payload.name;
				User.Instance.Email = result.payload.email;
				User.Instance.PictureURL = result.payload.picture;
				User.Instance.SessionToken = result.payload.token;
				SQLiteDB.Instance.SaveUser(User.Instance);
			}
			return result.success;
		}


		public async static Task PrepareLogout() {
			User.Instance = null;
			RewardEligibilityManager.Instance = null;
			LoginManager.IsLoginVerified = LoginManager.IsOfflineLoggedIn = false;
			await CrossGeolocator.Current.StopListeningAsync();
			DependencyService.Get<IMotionActivityManager>().StopMotionUpdates();
		}
	}
}
