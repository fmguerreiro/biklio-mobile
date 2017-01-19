using System;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.OS;
using Android.Support.V4.Content;
using Xamarin.Forms;

namespace Trace.Droid {
	/// <summary>
	/// Class that implements the required interface for connecting to Google api client,
	/// which is required for fetching motion data.
	/// </summary>
	public class GoogleApiHandler :
			Java.Lang.Object,
			GoogleApiClient.IConnectionCallbacks,
			GoogleApiClient.IOnConnectionFailedListener {

		private const int MOTION_DATA_UPDATE_PERIOD = 5 * 1000; // ms

		public static GoogleApiClient GApiClient;

		public async void OnConnected(Bundle connectionHint) {
			System.Diagnostics.Debug.WriteLine("------- GMS.OnConnected() -> requested activity updates -------");
			await ActivityRecognition.ActivityRecognitionApi.RequestActivityUpdatesAsync(
					GApiClient,
					MOTION_DATA_UPDATE_PERIOD,
					MotionActivityManager.ActivityDetectionPendingIntent
				);
			LocalBroadcastManager.GetInstance(Forms.Context).RegisterReceiver(
				MotionActivityManager.BroadcastReceiver,
				new IntentFilter(App.AppName + ".BROADCAST_ACTION"));
		}

		public void OnConnectionSuspended(int cause) {
			System.Diagnostics.Debug.WriteLine("------- GMS.OnConnectionSuspended() -------");
			//mApiClient.Connect();
		}

		public void OnConnectionFailed(ConnectionResult result) {
			System.Diagnostics.Debug.WriteLine("------- GMS.OnConnectionFailed() -------");
		}
	}
}
