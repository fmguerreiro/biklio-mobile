using System;
using Xamarin.Auth;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.Droid.StoreCredentials))]
namespace Trace.Droid {

	public class StoreCredentials : StoreCredentialsInterface {

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

		public void SaveCredentials(string username, string password) {
			if(!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password)) {
				var account = new Account {
					Username = username
				};
				account.Properties.Add("Password", password);
				AccountStore.Create(Forms.Context).Save(account, App.AppName);
			}
		}

		public void DeleteCredentials() {
			var iterator = AccountStore.Create(Forms.Context).FindAccountsForService(App.AppName).GetEnumerator();
			if(iterator.MoveNext() == true) {
				AccountStore.Create(Forms.Context).Delete(iterator.Current, App.AppName);
			}
		}
	}
}