using System.Diagnostics;
using System.Linq;
using Foundation;
using Google.SignIn;
using Newtonsoft.Json.Linq;
using Trace;
using Trace.iOS;
using UIKit;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(GoogleOAuthUIPage), typeof(GoogleOAuthPageRenderer))]
namespace Trace.iOS {

	public class GoogleOAuthPageRenderer : PageRenderer, ISignInUIDelegate, ISignInDelegate {

		private bool didTryLoginOnce = false;

		public void DidSignIn(SignIn signIn, GoogleUser gUser, NSError error) {
			Device.BeginInvokeOnMainThread(() => {
				//var alert = new UIAlertView("Login", "In DidSignIn", null, "OK", null);
				//alert.Show();
				//Debug.WriteLine("DidSignIn");
				if(gUser == null) {
					Debug.WriteLine("GoogleOAuthPageRenderer.DidSignIn(): Failure Google User == null", "Login");
					if(!didTryLoginOnce)
						SignIn.SharedInstance.SignInUser();
					didTryLoginOnce = true;
					//SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
					return;
				}

				if(error != null) {
					Debug.WriteLine("GoogleOAuthPageRenderer.DidSignIn(): Failure Google Error: " + error.Description, "Login");
					SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
					return;
				}

				if(gUser != null) {
					var accessToken = gUser.Authentication.AccessToken;
					var idToken = gUser.Authentication.IdToken;
					var fullname = gUser.Profile.Name;
					var pictureURL = gUser.Profile.GetImageUrl(48).AbsoluteString;
					var email = gUser.Profile.Email;

					var jToken = JObject.FromObject(new {
						access_token = accessToken,
						server_auth_code = gUser.ServerAuthCode,
						id_token = idToken,
						profile = JObject.FromObject(new {
							name = fullname,
							img = pictureURL,
							public_email = email,
							app_id = gUser.Authentication.ClientId,
							description = gUser.Authentication.Description
							// ***REMOVED***
						})
					});
					Debug.WriteLine("Google OAuth token: " + jToken);

					// Store token in keychain for later offline login.
					User.Instance.Username = gUser.UserID;
					var account = new Account(User.Instance.Username);
					AccountStore.Create().Save(account, OAuthConfigurationManager.KeystoreService);

					// Store information in SQLite. 
					User.Instance.AuthToken = idToken;
					User.Instance.Name = fullname;
					User.Instance.Email = email;
					User.Instance.PictureURL = pictureURL;
					SQLiteDB.Instance.SaveItem(User.Instance);
					SQLiteDB.Instance.InstantiateUser(User.Instance.Username);

					SignInPage.SuccessfulOAuthLoginAction.Invoke();
				}
			});
		}

		public override void ViewDidLoad() {
			base.ViewDidLoad();
			Debug.WriteLine("GoogleOAuthPageRenderer.ViewDidLoad()");
			//View.GestureRecognizers.OfType<UITapGestureRecognizer>().First().CancelsTouchesInView = false; //fix mentioned by AdamKemp here for the button TouchUpInside event not firing: https://forums.xamarin.com/discussion/comment/171084/#Comment_171084

			// If the account is already on the device keychain, no need for another request.
			var account = AccountStore.Create().FindAccountsForService(OAuthConfigurationManager.KeystoreService).FirstOrDefault();
			if(account != null) {
				Debug.WriteLine("GoogleOAuthPageRenderer.ViewDidLoad(): User already in keychain, skipping google oauth.");
				SQLiteDB.Instance.InstantiateUser(account.Username);
				SignInPage.SuccessfulOAuthLoginAction.Invoke();
				return;
			}
			Debug.WriteLine("GoogleOAuthPageRenderer.ViewDidLoad(): New user, using google oauth.");

			// Assign the SignIn Delegates to receive callbacks
			SignIn.SharedInstance.UIDelegate = this;
			SignIn.SharedInstance.Delegate = this;

			// Sign the user in automatically
			//SignIn.SharedInstance.SignInUser();
			SignIn.SharedInstance.SignInUserSilently();
		}
	}
}