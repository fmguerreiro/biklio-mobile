using System;
using Xamarin.Auth;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.Droid.StoreCredentials))]
namespace Trace.Droid {

	public class StoreCredentials : DeviceKeychainInterface {

		public string Username {
			get {
				var iterator = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).GetEnumerator();
				return iterator.MoveNext() ? iterator.Current.Username : null;
			}
		}

		public string Password {
			get {
				var iterator = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).GetEnumerator();
				return iterator.MoveNext() ? iterator.Current.Properties["Password"] : null;
			}
		}

		public string GetPassword(string username) {
			var accounts = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				if(a.Username.Equals(username))
					return a.Properties["Password"];
			}
			return null;
		}

		public bool Exists(string username) {
			var accounts = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName);
			foreach(Account a in accounts) {
				if(a.Username.Equals(username))
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
				AccountStore.Create(Forms.Context).Save(account, App.AppName);
			}
		}

		public void DeleteAllCredentials() {
			var iterator = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).GetEnumerator();
			while(iterator.MoveNext()) {
				AccountStore.Create(Forms.Context).Delete(iterator.Current, App.AppName);
			}
		}

		public void DeleteCredentials(string username) {
			var iterator = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).GetEnumerator();
			while(iterator.MoveNext()) {
				if(iterator.Current.Username.Equals(username))
					AccountStore.Create(Forms.Context).Delete(iterator.Current, App.AppName);
			}
		}
	}
}