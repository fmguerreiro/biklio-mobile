﻿using Android.App;
using Newtonsoft.Json;
using System;
using System.Linq;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Trace;
using Trace.Droid;

[assembly: ExportRenderer(typeof(OAuthUIPage), typeof(OAuthUIPageRenderer))]
namespace Trace.Droid {

	// Use a custom page renderer to display the authentication UI on the AuthenticationPage
	public class OAuthUIPageRenderer : PageRenderer {
		bool isShown;

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e) {
			base.OnElementChanged(e);

			// Retrieve any stored account information
			var accounts = AccountStore.Create(Context).FindAccountsForService(OAuthConstants.KeystoreService);
			var account = accounts.FirstOrDefault();

			if(account == null) {
				if(!isShown) {
					isShown = true;

					// Initialize the object that communicates with the OAuth service
					var auth = new OAuth2Authenticator(
									  OAuthConstants.GoogleClientId,
									  OAuthConstants.GoogleClientSecret,
									  OAuthConstants.Scope,
									  new Uri(OAuthConstants.AuthorizeUrl),
									  new Uri(OAuthConstants.RedirectUrl),
									  new Uri(OAuthConstants.AccessTokenUrl));

					// Register an event handler for when the authentication process completes
					auth.Completed += OnAuthenticationCompleted;

					// Display the UI
					var activity = Context as Activity;
					activity.StartActivity(auth.GetUI(activity));
				}
			}
			else {
				if(!isShown) {
					User.Instance.Email = User.Instance.Username = account.Username;
					SQLiteDB.Instance.SaveItem(User.Instance);
					SignInPage.SuccessfulOAuthLoginAction.Invoke();
				}
			}
		}

		async void OnAuthenticationCompleted(object sender, AuthenticatorCompletedEventArgs e) {
			if(e.IsAuthenticated) {
				// If the user is authenticated, request their basic user data
				var request = new OAuth2Request("GET", new Uri(OAuthConstants.UserInfoUrl), null, e.Account);
				var response = await request.GetResponseAsync();
				if(response != null) {
					// Deserialize the data and store it in the account store
					// The users email address will be used to identify data in SQLite
					string userJson = response.GetResponseText();
					GoogleOAuthUser user = JsonConvert.DeserializeObject<GoogleOAuthUser>(userJson);
					e.Account.Username = user.Email;
					AccountStore.Create(Context).Save(e.Account, OAuthConstants.KeystoreService);

					// Initialize the user
					User.Instance.Username = User.Instance.Email = user.Email;
					SQLiteDB.Instance.SaveItem(User.Instance);
				}
			}
			// If the user is logged in navigate to the Home page.
			// Otherwise allow another login attempt.
			SignInPage.SuccessfulOAuthLoginAction.Invoke();
		}
	}
}