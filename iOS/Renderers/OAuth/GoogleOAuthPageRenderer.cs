using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Google.SignIn;
using Newtonsoft.Json.Linq;
using Trace;
using Trace.iOS;
using Xamarin.Auth;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(GoogleOAuthUIPage), typeof(GoogleOAuthPageRenderer))]
namespace Trace.iOS {

	public class GoogleOAuthPageRenderer : PageRenderer, ISignInUIDelegate, ISignInDelegate {

		//private bool didTryLoginOnce = false;

		public void DidSignIn(SignIn signIn, GoogleUser gUser, NSError error) {
			//Device.BeginInvokeOnMainThread(() => {
			//var alert = new UIAlertView("Login", "In DidSignIn", null, "OK", null);
			//alert.Show();
			Debug.WriteLine("DidSignIn() called");
			if(gUser == null) {
				Debug.WriteLine("GoogleOAuthPageRenderer.DidSignIn(): Failure Google User == null", "Login");
				//if(!didTryLoginOnce)
				//	SignIn.SharedInstance.SignInUser();
				//didTryLoginOnce = true;
				SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
				return;
			}

			if(error != null) {
				Debug.WriteLine("GoogleOAuthPageRenderer.DidSignIn(): Failure Google Error: " + error.Description, "Login");
				SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
				return;
			}

			if(gUser != null) {
				var googleId = gUser.UserID;
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
						// 111912839668-ur9ffohu7qbfj07hammvgfld53nt91vd.apps.googleusercontent.com
					})
				});
				Debug.WriteLine("Google OAuth token: " + jToken);

				SQLiteDB.Instance.InstantiateUser(googleId);

				// Store token in keychain for later offline login.
				//User.Instance.Username = googleId;
				var account = new Account(User.Instance.Username);
				AccountStore.Create().Save(account, OAuthConfigurationManager.KeystoreService);

				// Store information in SQLite. 
				User.Instance.IDToken = idToken;
				User.Instance.Name = fullname;
				User.Instance.Email = email;
				User.Instance.PictureURL = pictureURL;

				Debug.WriteLine("google token is: " + idToken);
				Debug.WriteLine("user token is  : " + User.Instance.IDToken);
				SQLiteDB.Instance.SaveUser(User.Instance);

				SignInPage.SuccessfulOAuthLoginAction.Invoke();
			}
			//});
		}

		public override async void ViewDidLoad() {
			base.ViewDidLoad();

			// Assign the SignIn Delegates to receive callbacks
			SignIn.SharedInstance.UIDelegate = this;
			SignIn.SharedInstance.Delegate = this;

			//Debug.WriteLine("GoogleOAuthPageRenderer.ViewDidLoad()");
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

			// HACK not sure why this works, but thank the lord.
			await Task.Delay(1000);

			SignIn.SharedInstance.SignInUser();
			//SignIn.SharedInstance.SignInUserSilently();
		}
	}
}