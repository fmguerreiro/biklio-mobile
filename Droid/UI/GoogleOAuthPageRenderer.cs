using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Gms.Common.Apis;
using Android.Support.V7.App;
using Android.Gms.Common;
using Android.Util;
using Trace;
using Trace.Droid;
using Xamarin.Forms;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Auth.Api;
using Xamarin.Forms.Platform.Android;
using System.Threading.Tasks;

[assembly: ExportRenderer(typeof(GoogleOAuthUIPage), typeof(GoogleOAuthPageRenderer))]
namespace Trace.Droid {

	public class GoogleOAuthPageRenderer : PageRenderer,
	GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener {

		global::Android.Views.View view;

		GoogleApiClient mGoogleApiClient;

		readonly int RC_SIGN_IN = 23;


		protected override void OnElementChanged(ElementChangedEventArgs<Page> e) {
			base.OnElementChanged(e);

			if(e.OldElement != null || Element == null)
				return;

			// Configure sign-in to request the user's ID, email address, and basic profile. ID and
			// basic profile are included in DEFAULT_SIGN_IN.
			GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
				.RequestEmail()
				.Build();

			System.Diagnostics.Debug.WriteLine(gso.ToString());

			var activity = this.Context as Activity;
			//activity.LayoutInflater.Inflate(, this, false);
			//FragmentManager fm = activity.FragmentManager;
			//FragmentTransaction fragmentTransaction = fm.BeginTransaction();

			var fragment = new Android.Support.V4.App.FragmentActivity();
			//fragmentTransaction.Add(fragment, "GOOGLE_SIGNIN");

			mGoogleApiClient = new GoogleApiClient.Builder(activity)
				.EnableAutoManage(fragment, this)
				.AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
				.Build();

			//Task.Run(() => {
			mGoogleApiClient.BlockingConnect();
			System.Diagnostics.Debug.WriteLine(mGoogleApiClient.IsConnected);
			//});

			//Intent signInIntent = Auth.GoogleSignInApi.GetSignInIntent(mGoogleApiClient);

			//StartActivityForResult(signInIntent, RC_SIGN_IN);

			//AddView(view);
		}


		//public void onActivityResult(int requestCode, int resultCode, Intent data) {

		//	OnActivityResult(requestCode, (Result) resultCode, data);

		//	string name;
		//	string email;
		//	string token;
		//	// Result returned from launching the Intent from
		//	//   GoogleSignInApi.getSignInIntent(...);
		//	if(requestCode == RC_SIGN_IN) {
		//		GoogleSignInResult result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
		//		if(result.IsSuccess) {
		//			GoogleSignInAccount acct = result.SignInAccount;
		//			// Get account information
		//			name = acct.DisplayName;
		//			email = acct.Email;
		//			token = acct.IdToken;
		//			System.Diagnostics.Debug.WriteLine("Got user info: " + name + " " + email + " " + token);
		//		}
		//	}
		//}

		//protected override void OnStart() {
		//	base.OnStart();
		//	mGoogleApiClient.Connect();
		//}

		//protected override void OnStop() {
		//	base.OnStop();
		//	mGoogleApiClient.Disconnect();
		//}

		//protected override void OnSaveInstanceState(Bundle outState) {
		//	base.OnSaveInstanceState(outState);
		//}

		public void OnConnected(Bundle connectionHint) {
			System.Diagnostics.Debug.WriteLine("OnConnected: " + connectionHint);
		}

		public void OnConnectionSuspended(int cause) {
			System.Diagnostics.Debug.WriteLine("OnConnectionSuspended: " + cause);
		}

		public void OnConnectionFailed(ConnectionResult result) {
			System.Diagnostics.Debug.WriteLine("OnConnectionFailed: " + result);
		}

		class DialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener {
			public Action<IDialogInterface> OnCancelImpl { get; set; }

			public void OnCancel(IDialogInterface dialog) {
				OnCancelImpl(dialog);
			}
		}

	}

}