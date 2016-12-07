using System.Linq;
using Foundation;
using Trace.iOS;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(OAuth))]
namespace Trace.iOS {
	public class OAuth : IOAuth {

		public void Logout() {
			foreach(var cookie in NSHttpCookieStorage.SharedStorage.Cookies) {
				NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
			}

			var accounts = AccountStore.Create().FindAccountsForService(OAuthConfigurationManager.KeystoreService);
			var account = accounts.FirstOrDefault();

			if(account != null) {
				AccountStore.Create().Delete(account, OAuthConfigurationManager.KeystoreService);
				User.Instance = new User();
			}
		}
	}
}
