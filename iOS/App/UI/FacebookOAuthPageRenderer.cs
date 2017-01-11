using Trace;
using Trace.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Auth;
using System.Linq;
using System;
using Newtonsoft.Json;
using System.Diagnostics;


[assembly: ExportRenderer(typeof(FacebookOAuthUIPage), typeof(FacebookOAuthPageRenderer))]
namespace Trace.iOS {

	public class FacebookOAuthPageRenderer : PageRenderer {
		bool isShown;

		public override void ViewDidAppear(bool animated) {
			base.ViewDidAppear(animated);

			// Retrieve any stored account information
			var accounts = AccountStore.Create().FindAccountsForService(OAuthConfigurationManager.KeystoreService);
			var account = accounts.FirstOrDefault();
			//if(account != null) { var _ = SQLiteDB.Instance; AccountStore.Create().Delete(account, OAuthConfigurationManager.KeystoreService); account = AccountStore.Create().FindAccountsForService(OAuthConfigurationManager.KeystoreService).FirstOrDefault(); }

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
					var ui = auth.GetUI();
					PresentViewController(ui, true, null);
				}
			}
			else {
				if(!isShown) {
					Debug.WriteLine("Offline FB login with account: " + account.Username);
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
				Debug.WriteLine("OAuth response url: " + response);
				if(response != null) {
					SQLiteDB.Instance.InstantiateUser(User.Instance.Username);

					// Deserialize the data and store it in the account store
					// The users email address will be used to identify data in SQLite DB
					string userJson = response.GetResponseText();
					Debug.WriteLine("OAuth response body: " + userJson);
					OAuthUser user = JsonConvert.DeserializeObject<OAuthUser>(userJson);

					// Store the credentials in keychain
					User.Instance.Username = e.Account.Username = user.Id;
					AccountStore.Create().Save(e.Account, OAuthConfigurationManager.KeystoreService);

					// Check if the token was received.
					var authToken = response.ResponseUri.Query.Split(new string[] { "?access_token=" }, StringSplitOptions.RemoveEmptyEntries)[0];
					if(string.IsNullOrWhiteSpace(authToken)) {
						SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
						return;
					}

					// Finally, store the user.
					User.Instance.IDToken = authToken;
					SQLiteDB.Instance.SaveUser(User.Instance);

					// If the user is logged in navigate to the Home page.
					// Otherwise allow another login attempt.
					SignInPage.SuccessfulOAuthLoginAction.Invoke();
					return;
				}
				SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
			}
		}
	}
}
