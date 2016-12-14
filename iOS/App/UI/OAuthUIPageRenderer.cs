using Trace;
using Trace.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Auth;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.Diagnostics;

[assembly: ExportRenderer(typeof(OAuthUIPage), typeof(OAuthUIPageRenderer))]
namespace Trace.iOS {

	public class OAuthUIPageRenderer : PageRenderer {
		bool isShown;

		public override void ViewDidAppear(bool animated) {
			base.ViewDidAppear(animated);

			// Retrieve any stored account information
			var accounts = AccountStore.Create().FindAccountsForService(OAuthConfigurationManager.KeystoreService);
			var account = accounts.FirstOrDefault();
			//if(account != null) AccountStore.Create().Delete(account, OAuthConfigurationManager.KeystoreService); var _ = SQLiteDB.Instance;

			if(account == null) {
				if(!isShown) {
					isShown = true;

					// Initialize the object that communicates with the OAuth service
					var auth = new OAuth2Authenticator(
				   					OAuthConfigurationManager.ClientId,
		   							OAuthConfigurationManager.Scope,
		   							new Uri(OAuthConfigurationManager.AuthorizeUrl),
		   							new Uri(OAuthConfigurationManager.RedirectUrl));

					// Register an event handler for when the authentication process completes
					auth.Completed += OnAuthenticationCompleted;

					// Display the UI
					PresentViewController(auth.GetUI(), true, null);
				}
			}
			else {
				if(!isShown) {
					SQLiteDB.Instance.InstantiateUser(account.Username);
					//SQLiteDB.Instance.SaveItem(User.Instance);
					SignInPage.SuccessfulOAuthLoginAction.Invoke();
				}
			}
		}

		async void OnAuthenticationCompleted(object sender, AuthenticatorCompletedEventArgs e) {
			if(e.IsAuthenticated) {
				// If the user is authenticated, request their basic user data
				var request = new OAuth2Request("GET", new Uri(OAuthConfigurationManager.UserInfoUrl), null, e.Account);

				var response = await request.GetResponseAsync();
				Debug.WriteLine("OAuth response url: " + response.ToString());
				if(response != null) {

					// Deserialize the data and store it in the account store
					// The users email address will be used to identify data in SQLite DB
					string userJson = response.GetResponseText();
					Debug.WriteLine("OAuth response body: " + userJson);
					OAuthUser user = JsonConvert.DeserializeObject<OAuthUser>(userJson);

					// Store the credentials in keychain
					User.Instance.Username = e.Account.Username = user.Id;
					AccountStore.Create().Save(e.Account, OAuthConfigurationManager.KeystoreService);

					// Store the user
					var authToken = response.ResponseUri.Query.Split(new string[] { "?access_token=" }, StringSplitOptions.RemoveEmptyEntries)[0];
					User.Instance.AuthToken = authToken;
					SQLiteDB.Instance.SaveItem(User.Instance);
					SQLiteDB.Instance.InstantiateUser(User.Instance.Username);
				}
			}
			// If the user is logged in navigate to the Home page.
			// Otherwise allow another login attempt.
			SignInPage.SuccessfulOAuthLoginAction.Invoke();
		}
	}
}
