using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Gms.Common.Apis;
using Android.Gms.Location;
using Android.App;
using Android.Content;
using System.Linq;
using Xamarin.Forms;

[assembly: Dependency(typeof(Trace.Droid.MotionActivityManager))]
namespace Trace.Droid {
	public class MotionActivityManager : IMotionActivityManager {

		public IList<ActivityEvent> ActivityEvents { get; set; }
		public int WalkingDuration { get; set; }
		public int RunningDuration { get; set; }
		public int CyclingDuration { get; set; }
		public int DrivingDuration { get; set; }
		public bool IsInitialized { get; set; }
		public double CurrentAvgSpeed { get; set; }


		// Handles connections, disconnections from google api (for accessing motion data).
		static internal GoogleApiHandler gApiHandler = new GoogleApiHandler();

		// TODO likely to be removed -- not needed
		//static public ActivityDetectionBroadcastReceiver BroadcastReceiver;

		/// <summary>
		/// Intent message sent to DetectedActivitiesIntentService whenever motion detection data is obtained.
		/// </summary>
		static PendingIntent activityDetectionPendingIntent;
		public static PendingIntent ActivityDetectionPendingIntent {
			get {
				if(activityDetectionPendingIntent != null) {
					return activityDetectionPendingIntent;
				}
				var intent = new Intent(Forms.Context, typeof(DetectedActivitiesService));
				return activityDetectionPendingIntent = PendingIntent.GetService(Forms.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
			}
		}


		/// <summary>
		/// Functions that implement the IMotionActivityManager from the shared code project.
		/// </summary>
		public void InitMotionActivity() {

			//if(!DetectedActivitiesService.IsStarted) {
			//	Forms.Context.StartService(new Intent(Forms.Context, typeof(DetectedActivitiesService)));
			//}
			DetectedActivitiesService.UserId = User.Instance.Id;

			if(GoogleApiHandler.GApiClient == null || !GoogleApiHandler.GApiClient.IsConnected) {
				GoogleApiHandler.GApiClient = new GoogleApiClient.Builder(Forms.Context)
								.AddApi(ActivityRecognition.API)
								.AddConnectionCallbacks(gApiHandler)
								.AddOnConnectionFailedListener(gApiHandler)
								.Build();

				GoogleApiHandler.GApiClient.RegisterConnectionCallbacks(gApiHandler);

				//if(BroadcastReceiver == null) {
				//	BroadcastReceiver = new ActivityDetectionBroadcastReceiver();
				//	BroadcastReceiver.OnReceiveImpl = (context, intent) => {
				//		System.Diagnostics.Debug.WriteLine("------- BROADCAST_RECEIVED -------");
				//		//var updatedActivity = (Android.Gms.Location.DetectedActivity) intent.GetParcelableExtra(App.AppName + ".ACTIVITY_EXTRA");
				//		//handlerCallback(ActivityToType(updatedActivity));
				//	};
				//}
			}
			else { System.Diagnostics.Debug.WriteLine("-------- InitMotionActivity() -> Context not initialized --------"); }
		}


		public void StartMotionUpdates(Action<ActivityType> handler) {
			System.Diagnostics.Debug.WriteLine("-------- Connecting to GMS Client --------");
			if(!GoogleApiHandler.GApiClient.IsConnected)
				GoogleApiHandler.GApiClient.Connect();

			// Update the handler callback that feeds the motion data into the state machine.
			DetectedActivitiesService.UserId = User.Instance.Id;
			DetectedActivitiesService.HandlerCallback = handler;
			IsInitialized = true;
		}


		public void StopMotionUpdates() {
			if(GoogleApiHandler.GApiClient.IsConnected) {
				GoogleApiHandler.GApiClient.Disconnect();
				IsInitialized = false;
			}
			if(!WebServerLoginManager.IsOfflineLoggedIn) {
				//DetectedActivitiesService.UserId = 0;
				//// Unregister service.
				//Forms.Context.StopService(new Intent(Forms.Context, typeof(DetectedActivitiesService)));
				//DetectedActivitiesService.IsStarted = false;
			}
		}


		public void Reset() {
			WalkingDuration = 0;
			RunningDuration = 0;
			CyclingDuration = 0;
			DrivingDuration = 0;
			// TODO clear db
			ActivityEvents?.Clear();
		}


		public ActivityType GetMostCommonActivity() {
			long[] activityDurations = { WalkingDuration, RunningDuration, CyclingDuration, DrivingDuration };
			long max = activityDurations.Max();

			if(max == 0)
				return ActivityType.Unknown;
			if(max == CyclingDuration)
				return ActivityType.Cycling;
			if(max == RunningDuration)
				return ActivityType.Running;
			if(max == WalkingDuration)
				return ActivityType.Walking;
			return DrivingDuration > 0 ? ActivityType.Automative : ActivityType.Unknown;
		}

		/// <summary>
		/// Process the motion data that occurs between 'start' and 'end', calculating significant events
		/// and durations of each mode of transport used.
		/// </summary>
		/// <returns>The historical data.</returns>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public async Task QueryHistoricalData(DateTime start, DateTime end) {
			// First filter the activities that happened between 'start' and 'end'.
			var filteredActivities = new List<ActivityData>();
			var startInSeconds = start.DatetimeToEpochSeconds();
			var endInSeconds = end.DatetimeToEpochSeconds();

			// Deserialize activities stored until now.
			var activitiesObtained = SQLiteDB.Instance.GetItems<ActivityData>();
			foreach(var activity in activitiesObtained) {
				var timeInSeconds = activity.Timestamp;
				if(TimeUtil.IsWithinPeriod(time: timeInSeconds, start: startInSeconds, end: endInSeconds)) {
					filteredActivities.Add(activity);
				}
			}
			// Then process the data.
			ActivityEvents = await Task.Run(() => aggregateActivitiesAsync(filteredActivities.ToArray()));
		}


		List<ActivityEvent> aggregateActivitiesAsync(ActivityData[] activities) {
			var filteredActivities = new List<ActivityData>();

			// Skip all contiguous unclassified and stationary activities so that only one remains.
			for(int i = 0; i < activities.Length; ++i) {
				var activity = activities[i];
				filteredActivities.Add(activity);

				if(activity.Type == DetectedActivity.Unknown || activity.Type == DetectedActivity.Still) {
					while(++i < activities.Length) {
						var skipActiv = activities[i];
						if(skipActiv.Type != DetectedActivity.Unknown && skipActiv.Type != DetectedActivity.Still) {
							i = i - 1;
							break;
						}
					}
				}
			}

			// Skip all unclassified and stationary activities if their duration is smaller than
			// some threshold.  This has the effect of coalescing the remaining med + high
			// confidence activities together.
			for(int i = 0; i < filteredActivities.Count - 1;) {
				var activity = filteredActivities[i];
				var nextActivity = filteredActivities[i + 1];

				var duration = nextActivity.Timestamp - activity.Timestamp;
				const int THERESHOLD = 60 * 3;
				if(duration < THERESHOLD && (activity.Type == DetectedActivity.Still || activity.Type == DetectedActivity.Unknown)) {
					filteredActivities.RemoveAt(i);
				}
				else {
					++i;
				}
			}

			// Coalesce activities where they differ only in confidence.
			for(int i = 1; i < filteredActivities.Count;) {
				var prevActivity = filteredActivities[i - 1];
				var activity = filteredActivities[i];

				if((prevActivity.Type == DetectedActivity.Walking && activity.Type == DetectedActivity.Walking) ||
					(prevActivity.Type == DetectedActivity.Running && activity.Type == DetectedActivity.Running) ||
					(prevActivity.Type == DetectedActivity.OnBicycle && activity.Type == DetectedActivity.OnBicycle) ||
					(prevActivity.Type == DetectedActivity.InVehicle && activity.Type == DetectedActivity.InVehicle)) {
					filteredActivities.RemoveAt(i);
				}
				else {
					++i;
				}
			}

			// Finally transform into ActivityEvent and increment duration
			var activityEvents = new List<ActivityEvent>();

			for(int i = 0; i < filteredActivities.Count - 1; i++) {
				var activity = filteredActivities[i];
				var nextActivity = filteredActivities[i + 1];

				if(activity.Type == DetectedActivity.Unknown || activity.Type == DetectedActivity.Still)
					continue;

				var activityEvent = new ActivityEvent(ActivityToType(activity),
					TimeUtil.EpochSecondsToDatetime(activity.Timestamp),
					TimeUtil.EpochSecondsToDatetime(nextActivity.Timestamp));

				activityEvents.Add(activityEvent);
				ActivityToDuration(activityEvent.ActivityType, activityEvent.ActivityDurationInSeconds());
			}
			return activityEvents;
		}


		#region Utility
		public static ActivityType ActivityToType(DetectedActivity activity) {
			switch(activity.Type) {
				case DetectedActivity.OnBicycle: return ActivityType.Cycling;
				case DetectedActivity.Running: return ActivityType.Running;
				case DetectedActivity.Walking: return ActivityType.Walking;
				case DetectedActivity.InVehicle: return ActivityType.Automative;
				case DetectedActivity.Still: return ActivityType.Stationary;
				default: return ActivityType.Unknown;
			}
		}

		public static ActivityType ActivityToType(ActivityData activity) {
			switch(activity.Type) {
				case DetectedActivity.OnBicycle: return ActivityType.Cycling;
				case DetectedActivity.Running: return ActivityType.Running;
				case DetectedActivity.Walking: return ActivityType.Walking;
				case DetectedActivity.InVehicle: return ActivityType.Automative;
				case DetectedActivity.Still: return ActivityType.Stationary;
				default: return ActivityType.Unknown;
			}
		}

		public static string ActivityTypeToString(int type) {
			switch(type) {
				case DetectedActivity.OnBicycle: return "Cycling";
				case DetectedActivity.Running: return "Running";
				case DetectedActivity.Walking: return "Walking";
				case DetectedActivity.InVehicle: return "Automative";
				case DetectedActivity.Still: return "Stationary";
				default: return "Unknown";
			}
		}

		public void ActivityToDuration(ActivityType type, long duration) {
			switch(type) {
				case ActivityType.Walking:
					WalkingDuration += (int) duration;
					break;
				case ActivityType.Running:
					RunningDuration += (int) duration;
					break;
				case ActivityType.Cycling:
					CyclingDuration += (int) duration;
					break;
				case ActivityType.Automative:
					DrivingDuration += (int) duration;
					break;
			}
		}
		#endregion
	}
}
