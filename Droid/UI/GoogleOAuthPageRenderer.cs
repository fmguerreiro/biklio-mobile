using System.Diagnostics;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Support.V7.App;
using Newtonsoft.Json.Linq;
using Trace;
using Trace.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(GoogleOAuthUIPage), typeof(GoogleOAuthPageRenderer))]
namespace Trace.Droid {
	// TODO implement!
	public class GoogleOAuthPageRenderer
//:  AppCompatActivity, View.IOnClickListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener 
{

		GoogleApiClient mGoogleApiClient;

		//protected override void OnCreate(Bundle savedInstanceState) {
		//	base.OnCreate(savedInstanceState);

		//	mGoogleApiClient = new GoogleApiClient.Builder(this)
		//		.AddConnectionCallbacks(this)
		//		.AddOnConnectionFailedListener(this)
		//		.AddApi(PlusClass.API)
		//		.AddScope(new Scope(Scopes.Profile))
		//		.Build();
		//}

		//protected override void OnStart ()
		//{
		//	base.OnStart ();
		//	mGoogleApiClient.Connect ();
		//}

		//protected override void OnStop ()
		//{
		//	base.OnStop ();
		//	mGoogleApiClient.Disconnect ();
		//}

		//protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		//{
		//	base.OnActivityResult (requestCode, resultCode, data);
		//	Log.Debug (TAG, "onActivityResult:" + requestCode + ":" + resultCode + ":" + data);

		//	if (requestCode == RC_SIGN_IN) {
		//		if (resultCode != Result.Ok) {
		//			mShouldResolve = false;
		//		}

		//		mIsResolving = false;
		//		mGoogleApiClient.Connect ();
		//	}
		//}

		//public void OnConnected (Bundle connectionHint)
		//{
		//	Log.Debug (TAG, "onConnected:" + connectionHint);

		//	UpdateUI (true);
		//}

		//public void OnConnectionSuspended (int cause)
		//{
		//	Log.Warn (TAG, "onConnectionSuspended:" + cause);
		//}

		//public void OnConnectionFailed (ConnectionResult result)
		//{
		//	Log.Debug (TAG, "onConnectionFailed:" + result);

		//	if (!mIsResolving && mShouldResolve) {
		//		if (result.HasResolution) {
		//			try {
		//				result.StartResolutionForResult (this, RC_SIGN_IN);
		//				mIsResolving = true;
		//			} catch (IntentSender.SendIntentException e) {
		//				Log.Error (TAG, "Could not resolve ConnectionResult.", e);
		//				mIsResolving = false;
		//				mGoogleApiClient.Connect ();
		//			}
		//		} else {
		//			ShowErrorDialog (result);
		//		}
		//	} else {
		//		UpdateUI (false);
		//	}
		//}

		//class DialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
		//{
		//	public Action<IDialogInterface> OnCancelImpl { get; set; }

		//	public void OnCancel (IDialogInterface dialog)
		//	{
		//		OnCancelImpl (dialog);
		//	}
		//}

		//void ShowErrorDialog (ConnectionResult connectionResult)
		//{
		//	int errorCode = connectionResult.ErrorCode;

		//	if (GooglePlayServicesUtil.IsUserRecoverableError (errorCode)) {
		//		var listener = new DialogInterfaceOnCancelListener ();
		//		listener.OnCancelImpl = (dialog) => {
		//			mShouldResolve = false;
		//			UpdateUI (false);
		//		};
		//		GooglePlayServicesUtil.GetErrorDialog (errorCode, this, RC_SIGN_IN, listener).Show ();
		//	} else {
		//		var errorstring = string.Format(GetString (Resource.String.play_services_error_fmt), errorCode);
		//		Toast.MakeText (this, errorstring, ToastLength.Short).Show ();

		//		mShouldResolve = false;
		//		UpdateUI (false);
		//	}
		//}
	}
}