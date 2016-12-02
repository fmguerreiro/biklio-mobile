using System.Linq;
using Android.Webkit;
using Trace.Droid;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(OAuth))]
namespace Trace.Droid {
	public class OAuth : IOAuth {

		public void Logout() {
			CookieManager.Instance.RemoveAllCookie();

			var accounts = AccountStore.Create(Android.App.Application.Context).FindAccountsForService(OAuthConstants.KeystoreService);
			var account = accounts.FirstOrDefault();

			if(account != null) {
				AccountStore.Create(Android.App.Application.Context).Delete(account, OAuthConstants.KeystoreService);
				User.Instance = new User();
			}
		}
	}
}
