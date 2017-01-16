using System.Diagnostics;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.iOS.KeyChain))]
namespace Trace.iOS {

	public class KeyChain : ICredentialsStore {

		public string Username {
			get {
				var iterator = AccountStore.Create().FindAccountsForService(App.AppName).GetEnumerator();
				return iterator.MoveNext() ? iterator.Current.Username : null;
			}
		}

		public string Password {
			get {
				var iterator = AccountStore.Create().FindAccountsForService(App.AppName).GetEnumerator();
				return iterator.MoveNext() ? iterator.Current.Properties["Password"] : null;
			}
		}

		public string GetPassword(string username) {
			var accounts = AccountStore.Create().FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				Debug.WriteLine("GetPassword(): " + username);
				if(a.Username.Equals(username))
					return a.Properties["Password"];
			}
			return null;
		}

		public bool Exists(string username) {
			var accounts = AccountStore.Create().FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				if(a.Username.Equals(username))
					return true;
			}
			foreach(var a in AccountStore.Create().FindAccountsForService(new GoogleOAuthConfig().KeystoreService)) {
				if(a.Username.Equals(username))
					return true;
			}
			foreach(var a in AccountStore.Create().FindAccountsForService(new FacebookOAuthConfig().KeystoreService)) {
				if(a.Username.Equals(username))
					return true;
			}
			return false;
		}

		public bool OAuthExists() {
			foreach(var a in AccountStore.Create().FindAccountsForService(new GoogleOAuthConfig().KeystoreService)) {
				if(a != null)
					return true;
			}
			foreach(var a in AccountStore.Create().FindAccountsForService(new FacebookOAuthConfig().KeystoreService)) {
				if(a != null)
					return true;
			}
			return false;
		}

		public void SaveCredentials(string username, string password) {
			if(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)) {
				var account = new Account {
					Username = username
				};
				account.Properties.Add("Password", password);
				AccountStore.Create().Save(account, App.AppName);
			}
		}

		public void DeleteAllCredentials() {
			foreach(var a in AccountStore.Create().FindAccountsForService(App.AppName)) {
				Debug.WriteLine("DeleteAllCredentials() native-login: " + a.Username);
				AccountStore.Create().Delete(a, App.AppName);
			}
			foreach(var a in AccountStore.Create().FindAccountsForService(new GoogleOAuthConfig().KeystoreService)) {
				Debug.WriteLine("DeleteAllCredentials() google-login: " + a.Username);
				AccountStore.Create().Delete(a, new GoogleOAuthConfig().KeystoreService);
			}
			foreach(var a in AccountStore.Create().FindAccountsForService(new FacebookOAuthConfig().KeystoreService)) {
				Debug.WriteLine("DeleteAllCredentials() facebook-login: " + a.Username);
				AccountStore.Create().Delete(a, new FacebookOAuthConfig().KeystoreService);
			}
		}

		public void DeleteCredentials(string username) {
			var accounts = AccountStore.Create().FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				if(a.Username.Equals(username)) {
					AccountStore.Create().Delete(a, App.AppName);
				}
			}
			foreach(Account a in AccountStore.Create().FindAccountsForService(new GoogleOAuthConfig().KeystoreService)) {
				if(a.Username.Equals(username)) {
					AccountStore.Create().Delete(a, new GoogleOAuthConfig().KeystoreService);
					return;
				}
			}
			foreach(Account a in AccountStore.Create().FindAccountsForService(new FacebookOAuthConfig().KeystoreService)) {
				if(a.Username.Equals(username)) {
					AccountStore.Create().Delete(a, new FacebookOAuthConfig().KeystoreService);
					return;
				}
			}

		}
	}
}
