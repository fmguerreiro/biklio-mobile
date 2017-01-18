using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Gms.Common.Apis;
using Android.OS;
using Android.Gms.Common;
using Android.Gms.Location;
using Android.App;
using Android.Content;
using System.Linq;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.Droid.MotionActivityManager))]
namespace Trace.Droid {
	public class MotionActivityManager : IMotionActivityManager {

		public IList<ActivityEvent> ActivityEvents { get; set; }
		public int WalkingDuration { get; set; }
		public int RunningDuration { get; set; }
		public int CyclingDuration { get; set; }
		public int DrivingDuration { get; set; }

		internal GoogleApiClient mApiClient;

		/// <summary>
		/// Intent message sent to DetectedActivitiesIntentService whenever motion detection data is obtained.
		/// </summary>
		PendingIntent activityDetectionPendingIntent;
		PendingIntent ActivityDetectionPendingIntent {
			get {
				if(activityDetectionPendingIntent != null) {
					return activityDetectionPendingIntent;
				}
				var intent = new Intent(Xamarin.Forms.Forms.Context, typeof(DetectedActivitiesIntentService));
				return activityDetectionPendingIntent = PendingIntent.GetService(Xamarin.Forms.Forms.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
			}
		}


		/// <summary>
		/// Internal class that implements the required interface for connecting to Google api client.
		/// Implementing these classes requires IntPtr and Dispose methods to be implemented as well to be visible to Mono.
		/// Having a class inherit from Java.Lang.Object fixes this (the father class could not do this because it already extends IMotionActivityManager).
		/// </summary>
		private MotionActivityManagerHandler handler = new MotionActivityManagerHandler();
		private class MotionActivityManagerHandler : Java.Lang.Object,
					GoogleApiClient.IConnectionCallbacks,
					GoogleApiClient.IOnConnectionFailedListener {

			public void OnConnected(Bundle connectionHint) {
				System.Diagnostics.Debug.WriteLine("MotionActivityManager.OnConnected()");
			}

			public void OnConnectionSuspended(int cause) {
				System.Diagnostics.Debug.WriteLine("MotionActivityManager.OnConnectionSuspended()");
				//mApiClient.Connect();
			}

			public void OnConnectionFailed(ConnectionResult result) {
				System.Diagnostics.Debug.WriteLine("MotionActivityManager.OnConnectionFailed()");
			}
		}


		public void InitMotionActivity() {
			ActivityEvents = new List<ActivityEvent>();
			WalkingDuration = 0;
			RunningDuration = 0;
			CyclingDuration = 0;
			DrivingDuration = 0;

			if(Xamarin.Forms.Forms.IsInitialized) {
				mApiClient = new GoogleApiClient.Builder(Xamarin.Forms.Forms.Context, handler, handler)
								.AddApi(ActivityRecognition.API)
								.AddConnectionCallbacks(handler)
								.AddOnConnectionFailedListener(handler)
								.Build();

				mApiClient.Connect();
				mApiClient.RegisterConnectionCallbacks(handler);
			}
			else { System.Diagnostics.Debug.WriteLine("InitMotionActivity() -> Context not initialized."); }
		}

		private void reset() {
			WalkingDuration = 0;
			RunningDuration = 0;
			CyclingDuration = 0;
			DrivingDuration = 0;
			ActivityEvents.Clear();
		}

		public async void StartMotionUpdates(Action<ActivityType> handler) {
			// HACK I do this because StartMotionUpdates is called right after InitMotionUpdates in shared code.
			if(mApiClient.IsConnecting) {
				await Task.Delay(2000);
			}
			if(mApiClient.IsConnected) {
				await ActivityRecognition.ActivityRecognitionApi.RequestActivityUpdatesAsync(
						mApiClient,
						15 * 1000,
						ActivityDetectionPendingIntent
					);

				// Update the handler function of the service that will process this request.
				DetectedActivitiesIntentService.Handler = handler;
			}
			else { System.Diagnostics.Debug.WriteLine("StartMotionUpdates() -> GoogleApiClient is not connected yet!"); }
		}

		public void StopMotionUpdates() {
			if(mApiClient.IsConnected)
				mApiClient.Disconnect();
		}

		public void Reset() {
			if(mApiClient.IsConnected) {
				mApiClient.Disconnect();
				if(Xamarin.Forms.Forms.IsInitialized) {
					mApiClient = new GoogleApiClient.Builder(Xamarin.Forms.Forms.Context, handler, handler)
									.AddApi(ActivityRecognition.API)
									.AddConnectionCallbacks(handler)
									.AddOnConnectionFailedListener(handler)
									.Build();

					mApiClient.Connect();
				}
			}
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
		/// Process the motion data that happen between 'start' and 'end', calculating significant events
		/// and durations of each mode of transport used.
		/// </summary>
		/// <returns>The historical data.</returns>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public async Task QueryHistoricalData(DateTime start, DateTime end) {
			// First filter the activities that happened between 'start' and 'end'.
			var filteredActivities = new List<DetectedActivity>();
			var startInSeconds = start.DatetimeToEpochSeconds();
			var endInSeconds = end.DatetimeToEpochSeconds();
			foreach(var activity in DetectedActivitiesIntentService.ActivitiesObtained) {
				var timeInSeconds = activity.Timestamp.DatetimeToEpochSeconds();
				if(TimeUtil.IsWithinPeriod(time: timeInSeconds, start: startInSeconds, end: endInSeconds)) {
					filteredActivities.Add(activity);
				}
			}
			// Then process the data.
			ActivityEvents = await Task.Run(() => aggregateActivitiesAsync(filteredActivities.ToArray()));
		}


		List<ActivityEvent> aggregateActivitiesAsync(DetectedActivity[] activities) {
			var filteredActivities = new List<DetectedActivity>();

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

				var duration = nextActivity.Timestamp.DatetimeToEpochSeconds() - activity.Timestamp.DatetimeToEpochSeconds();
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
					activity.Timestamp,
					nextActivity.Timestamp);

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

		public static string ActivityTypeToString(ActivityType type) {
			switch(type) {
				case ActivityType.Cycling:
					return "Cycling";
				case ActivityType.Running:
					return "Running";
				case ActivityType.Walking:
					return "Walking";
				case ActivityType.Automative:
					return "Automotive";
				case ActivityType.Stationary:
					return "Stationary";
				default:
					return "Unknown";
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
