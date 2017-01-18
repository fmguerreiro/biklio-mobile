using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using System.Linq;
using Android.Support.V4.Content;
using System;

namespace Trace.Droid {
	/// <summary>
	/// Service that wakes up every time a detected activity data intent is created.
	/// </summary>
	public class DetectedActivitiesIntentService : IntentService {

		protected const string TAG = "activity-detection-intent-service";

		public static Action<ActivityType> Handler { get; set; }

		public static IList<DetectedActivity> ActivitiesObtained { get; set; }


		public DetectedActivitiesIntentService()
			: base(TAG) {
		}

		protected override void OnHandleIntent(Intent intent) {
			var result = ActivityRecognitionResult.ExtractResult(intent);
			var localIntent = new Intent(App.AppName + ".BROADCAST_ACTION");

			if(ActivitiesObtained == null) { ActivitiesObtained = new List<DetectedActivity>(); }

			var now = DateTime.UtcNow;
			IList<DetectedActivity> detectedActivities = new List<DetectedActivity>();
			foreach(var da in result.ProbableActivities)
				detectedActivities.Add(new DetectedActivity(da.Type, da.Confidence, now));

			System.Diagnostics.Debug.WriteLine(TAG, "activities detected");
			foreach(DetectedActivity da in detectedActivities) {
				System.Diagnostics.Debug.WriteLine($"{da.Type} {da.Confidence}");
				// Calls the handler function which will send the data to the state machine.
				Handler(MotionActivityManager.ActivityToType(da));
				// Store significant motion data, which will be later used to calculate activity events and calories.
				if(da.Confidence > 35) {
					ActivitiesObtained.Add(da);
				}
			}

			localIntent.PutExtra(App.AppName + ".ACTIVITY_EXTRA", detectedActivities.ToArray());
			LocalBroadcastManager.GetInstance(this).SendBroadcast(localIntent);
		}
	}
}
