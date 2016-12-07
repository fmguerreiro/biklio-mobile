using System;
using Xamarin.Auth;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.iOS.StoreCredentials))]
namespace Trace.iOS {

	public class StoreCredentials : DeviceKeychainInterface {

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

		public void SaveCredentials(string username, string password) {
			if(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)) {
				var account = new Account {
					Username = username
				};
				account.Properties.Add("Password", password);
				AccountStore.Create().Save(account, App.AppName);
			}
		}

		public void DeleteCredentials() {
			var iterator = AccountStore.Create().FindAccountsForService(App.AppName).GetEnumerator();
			if(iterator.MoveNext()) {
				AccountStore.Create().Delete(iterator.Current, App.AppName);
			}
		}
	}
}
