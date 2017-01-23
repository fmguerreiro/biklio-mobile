using System;
using System.Diagnostics;
using System.Linq;
using SQLite;
using Xamarin.Auth;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// Responsible for logging the user in using the most recent login info used.
	/// </summary>
	public class AutoLoginManager : DatabaseEntityBase {

		static AutoLoginManager instance;
		[Ignore]
		public static AutoLoginManager Instance {
			get {
				if(instance == null) { instance = new AutoLoginManager(); }
				return instance;
			}
			set { instance = value; }
		}

		public static LoginType MostRecentLoginType { get; set; }


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
				SQLiteDB.Instance.SaveAutoLoginConfig();
				return SignInPage.CreateSignInPage();
			}
		}
	}

	public enum LoginType {
		None,
		Normal,
		GoogleOAuth,
		FacebookOAuth
	}
}
