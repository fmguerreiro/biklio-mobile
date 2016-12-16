using System.Diagnostics;
using System.Linq;
using Foundation;
using Google.SignIn;
using Newtonsoft.Json.Linq;
using Trace;
using Trace.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(GoogleOAuthUIPage), typeof(GoogleOAuthPageRenderer))]
namespace Trace.iOS {

	public class GoogleOAuthPageRenderer : PageRenderer, ISignInUIDelegate, ISignInDelegate {

		public void DidSignIn(SignIn signIn, GoogleUser gUser, NSError error) {
			Device.BeginInvokeOnMainThread(() => {

				//var alert = new UIAlertView("Login", "In DidSignIn", null, "OK", null);
				//alert.Show();
				//Debug.WriteLine("DidSignIn");
				if(gUser == null) {
					Debug.WriteLine("In DidSignIn: Failure Google User == null", "Login");
					SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
					return;
				}

				if(error != null) {
					Debug.WriteLine("In DidSignIn: Failure Google Error: " + error.Description, "Login");
					SignInPage.UnsuccessfulOAuthLoginAction.Invoke();
					return;
				}

				if(gUser != null) {
					var jToken = JObject.FromObject(new {
						access_token = SignIn.SharedInstance.CurrentUser.Authentication.AccessToken,
						authorization_code = SignIn.SharedInstance.CurrentUser.ServerAuthCode,
						id_token = SignIn.SharedInstance.CurrentUser.Authentication.IdToken,
					});
					Debug.WriteLine("Google OAuth token: " + jToken);
					// TODO store in keychain.
					User.Instance.AuthToken = SignIn.SharedInstance.CurrentUser.Authentication.AccessToken;
					User.Instance.Username = SignIn.SharedInstance.CurrentUser.Authentication.IdToken;
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

			// TODO check if it is in keychain already.

			// Assign the SignIn Delegates to receive callbacks
			SignIn.SharedInstance.UIDelegate = this;
			SignIn.SharedInstance.Delegate = this;

			// Sign the user in automatically
			//SignIn.SharedInstance.SignInUser();
			SignIn.SharedInstance.SignInUserSilently();
		}
	}
}