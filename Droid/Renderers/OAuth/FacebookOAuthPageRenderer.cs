using Android.App;
using Newtonsoft.Json;
using System;
using System.Linq;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Trace;
using Trace.Droid;

[assembly: ExportRenderer(typeof(FacebookOAuthUIPage), typeof(FacebookOAuthPageRenderer))]
namespace Trace.Droid {

	public class FacebookOAuthPageRenderer : PageRenderer {
		bool isShown;

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e) {
			base.OnElementChanged(e);

			// Retrieve any stored account information
			var accounts = AccountStore.Create(Context).FindAccountsForService(OAuthConfigurationManager.KeystoreService);
			var account = accounts.FirstOrDefault();

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
					var activity = Context as Activity;
					activity.StartActivity(auth.GetUI(activity));
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
				System.Diagnostics.Debug.WriteLine("OAuth response url: " + response.ToString());
				if(response != null) {

					// Deserialize the data and store it in the account store
					// The users email address will be used to identify data in SQLite DB
					string userJson = response.GetResponseText();
					System.Diagnostics.Debug.WriteLine("OAuth response body: " + userJson);
					OAuthUser user = JsonConvert.DeserializeObject<OAuthUser>(userJson);

					// Store the credentials in keychain
					User.Instance.Username = e.Account.Username = user.Id;
					AccountStore.Create(Context).Save(e.Account, OAuthConfigurationManager.KeystoreService);

					// Check if the token was received.
					var authToken = response.ResponseUri.Query.Split(new string[] { "?access_token=" }, StringSplitOptions.RemoveEmptyEntries)[0];
					if(string.IsNullOrWhiteSpace(authToken)) {
						SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
						return;
					}

					// Finally, store the user.
					User.Instance.IDToken = authToken;
					SQLiteDB.Instance.SaveUser(User.Instance);
					SQLiteDB.Instance.InstantiateUser(User.Instance.Username);
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