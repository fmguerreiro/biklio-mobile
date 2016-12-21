using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public static class LoginManager {

		public static bool IsOfflineLoggedIn { get; set; }
		public static bool IsLoginVerified { get; set; }
		public static bool IsRememberMe { get; set; }


		public static async Task TryLogin() {
			IsOfflineLoggedIn = true;
			if(CrossConnectivity.Current.IsConnected) {
				await Task.Run(() => OnConnectivityChanged(null, new ConnectivityChangedEventArgs() { IsConnected = true }));
			}
		}

		/// <summary>
		/// Event handler that performs operations when the user gets Internet connection.
		/// Performs credentials verification and sends KPIs if they were not done so previously.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		public static async void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e) {
			Debug.WriteLine("OnConnectivityChanged() - IsLoginVerified: " + IsLoginVerified + " and isOfflineLoggedIn: " + IsOfflineLoggedIn);
			if(e.IsConnected) {
				var isWSReachable = await CrossConnectivity.Current.IsRemoteReachable(WebServerConstants.HOST, WebServerConstants.PORT, 5000);
				if(isWSReachable) {
					// Check login info.
					if(!IsLoginVerified && IsOfflineLoggedIn) {
						Debug.WriteLine("Verifying login.");
						bool result = false;
						if(string.IsNullOrEmpty(User.Instance.AuthToken))
							result = await Task.Run(() => LoginWithCredentials());
						else
							result = await Task.Run(() => LoginWithToken());

						// TODO Send KPIs
						if(result) {
							IsLoginVerified = true;
						}
						else {
							// If user credentials were wrong, boot the user out of the application.
							Device.BeginInvokeOnMainThread(async () => {
								await Application.Current.MainPage.DisplayAlert(Language.Error, Language.OnlineLoginError, Language.Ok);
								DependencyService.Get<DeviceKeychainInterface>().DeleteCredentials(User.Instance.Username);
								IsOfflineLoggedIn = false;
								Application.Current.MainPage = new NavigationPage(new StartPage());
							});
							return;
						}
					}
					else {
						// TODO Send KPIs.

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

				// TODO check if on login there is new information that should be updated.

			}
			return result.success;
		}


		public static async Task<bool> LoginWithToken() {

			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.LoginWithToken(User.Instance.AuthToken));

			if(result.success) {
				User.Instance.Name = result.payload.name;
				User.Instance.Email = result.payload.email;
				User.Instance.PictureURL = result.payload.picture;
				SQLiteDB.Instance.SaveItem(User.Instance);
			}
			return result.success;
		}
	}
}
