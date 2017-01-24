using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Auth;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// Responsible for logging the user in using the most recent login info used.
	/// </summary>
	public class AutoLoginManager {

		public static LoginType MostRecentLoginType {
			get {
				if(Application.Current.Properties.ContainsKey("login_type")) {
					var _ = Application.Current.Properties["login_type"];
					return (LoginType) _;
				}
				else {
					Application.Current.Properties["login_type"] = 0;
					return (LoginType) Application.Current.Properties["login_type"];
				}
			}
			// Note: the enum is cast to an 'int' because the 'properties' dict will fail silently with non-standard types.
			set {
				Application.Current.Properties["login_type"] = (int) value;
				Task.Run(async () => {
					await Application.Current.SavePropertiesAsync();
				});
			}
		}


		public static Page GetAppFirstPage() {
			switch(MostRecentLoginType) {
				case LoginType.None: { return SignInPage.CreateSignInPage(); }
				case LoginType.Normal: {
						var username = DependencyService.Get<ICredentialsStore>().Username;
						var password = DependencyService.Get<ICredentialsStore>().GetPassword(username);
						SQLiteDB.Instance.InstantiateUser(username);
						User.Instance.Password = password;
						WebServerLoginManager.TryLogin(isCredentialsLogin: true).DoNotAwait();
						User.Instance.GetCurrentKPI().AddLoginEvent(TimeUtil.CurrentEpochTimeSeconds());
						return new MainPage();
					}
				case LoginType.GoogleOAuth: {
						OAuthConfigurationManager.SetConfig(new GoogleOAuthConfig());
						return doOfflineOAuthLogin();
					}
				case LoginType.FacebookOAuth: {
						OAuthConfigurationManager.SetConfig(new FacebookOAuthConfig());
						return doOfflineOAuthLogin();
					}
				default: {
						return SignInPage.CreateSignInPage();
					}
			}
		}

		static Page doOfflineOAuthLogin() {
			var account = AccountStore.Create().FindAccountsForService(OAuthConfigurationManager.KeystoreService).FirstOrDefault();
			if(account != null) {
				SQLiteDB.Instance.InstantiateUser(account.Username);
				WebServerLoginManager.TryLogin(isCredentialsLogin: false).DoNotAwait();
				User.Instance.GetCurrentKPI().AddLoginEvent(TimeUtil.CurrentEpochTimeSeconds());
				return new MainPage();
			}
			else {
				MostRecentLoginType = LoginType.None;
				return SignInPage.CreateSignInPage();
			}
		}
	}

	public enum LoginType {
		None = 0,
		Normal = 1,
		GoogleOAuth = 2,
		FacebookOAuth = 3
	}
}
