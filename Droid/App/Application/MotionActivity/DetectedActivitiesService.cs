using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Gms.Location;
using System.Linq;
using Android.Support.V4.Content;
using System;

namespace Trace.Droid {
	/// <summary>
	/// Service that receives detected activity data broadcasts sent from the system.
	/// </summary>
	[Service(Exported = false)]
	public class DetectedActivitiesService : IntentService {

		protected const string TAG = "activity-detection-intent-service";

		public static long UserId;

		// Handler function that is passed from the shared application code.
		// The Input() function of the RewardEligibilityMonitor.
		public static Action<ActivityType> HandlerCallback { get; set; }

		public DetectedActivitiesService()
			: base(TAG) {
		}

		protected override void OnHandleIntent(Intent intent) {
			var result = ActivityRecognitionResult.ExtractResult(intent);
			//var localIntent = new Intent(App.AppName + ".BROADCAST_ACTION");

			// Prioritize cycling activities (i.e, if cycling probability >= 30 %)
			Android.Gms.Location.DetectedActivity res = result.MostProbableActivity;
			foreach(var activity in result.ProbableActivities) {
				if(activity.Type == DetectedActivity.OnBicycle && activity.Confidence > 29) {
					res = activity; break;
				}
			}
			System.Diagnostics.Debug.WriteLine($"Activity received -> Type: {MotionActivityManager.ActivityTypeToString(res.Type)}, Confidence: {res.Confidence}");

			// Send data to eligibility state machine.
			HandlerCallback(MotionActivityManager.ActivityToType(res));

			if(UserId != 0) {
				SQLiteDB.Instance.SaveItem(new ActivityData {
					UserId = UserId,
					Type = res.Type,
					Confidence = res.Confidence,
					Timestamp = TimeUtil.CurrentEpochTimeSeconds()
				});
			}
			//localIntent.PutExtra(App.AppName + ".ACTIVITY_EXTRA", res);
			//LocalBroadcastManager.GetInstance(this).SendBroadcast(localIntent);
		}
	}
}
