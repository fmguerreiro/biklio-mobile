using System;
using CoreMotion;
using Foundation;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.iOS.MotionActivityManager))]
namespace Trace.iOS {

	public class MotionActivityManager : IMotionActivityManager {

		public IList<ActivityEvent> ActivityEvents { get; set; }
		public int WalkingDuration { get; set; }
		public int RunningDuration { get; set; }
		public int CyclingDuration { get; set; }
		public int DrivingDuration { get; set; }
		public bool IsInitialized { get; set; }

		CMMotionActivityManager motionActivityMgr;
		IList<CMMotionActivity> activityList;


		public void InitMotionActivity() {
			motionActivityMgr = new CMMotionActivityManager();
			ActivityEvents = new List<ActivityEvent>();
			activityList = new List<CMMotionActivity>();
			IsInitialized = true;
		}


		public void StartMotionUpdates(Action<ActivityType> handler) {
			if(User.Instance.IsBackgroundAudioEnabled || Geolocator.IsTrackingInProgress) {
				motionActivityMgr.StartActivityUpdates(NSOperationQueue.MainQueue, ((activity) => {
					//Debug.WriteLine($"{activity.DebugDescription}");
					// A CMMotionActivity can have several modes set to true. We prioritize bycicle events. 
					if(activity.Cycling) {
						if(Geolocator.IsTrackingInProgress) activityList.Add(activity);
						handler(ActivityType.Cycling);
					}
					else if(activity.Confidence != CMMotionActivityConfidence.Low) {
						if(Geolocator.IsTrackingInProgress) activityList.Add(activity);
						handler(ActivityToType(activity));
					}
				}));
			}
		}


		public void StopMotionUpdates() {
			motionActivityMgr.StopActivityUpdates();
			IsInitialized = false;
		}


		public void Reset() {
			WalkingDuration = 0;
			RunningDuration = 0;
			CyclingDuration = 0;
			DrivingDuration = 0;
			NSOperationQueue.MainQueue.Dispose();
			ActivityEvents.Clear();
			activityList.Clear();
		}


		public ActivityType GetMostCommonActivity() {
			long[] activityDurations = { WalkingDuration, RunningDuration, CyclingDuration, DrivingDuration };
			long max = activityDurations.Max();

			if(max == 0)
				return ActivityType.Stationary;
			if(max == CyclingDuration)
				return ActivityType.Cycling;
			if(max == RunningDuration)
				return ActivityType.Running;
			if(max == WalkingDuration)
				return ActivityType.Walking;
			return DrivingDuration > 0 ? ActivityType.Automative : ActivityType.Unknown;
		}


		/// <summary>
		/// Gets the motion data obtained so far.
		/// iOS' QueryActivityAsync returns old data (does not have data from the past 5 minutes).
		/// If the user is using GPS, use the activity list to get the most recent data.
		/// If the user is using GSM, query the activity manager for the data.
		/// </summary>
		/// <returns>The historical data.</returns>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public async Task QueryHistoricalData(DateTime start, DateTime end) {
			CMMotionActivity[] activities = null;
			if(activityList.Count == 0) {
				Debug.WriteLine($"QueryHistoricalData: {TimeUtil.SecondsToHHMMSS(start.DatetimeToEpochSeconds())} - {TimeUtil.SecondsToHHMMSS(end.DatetimeToEpochSeconds())}");
				activities = await motionActivityMgr.QueryActivityAsync(
				   NSDateConverter.ToNsDate(start), NSDateConverter.ToNsDate(end), NSOperationQueue.MainQueue
			   	);
			}
			else activities = activityList.ToArray();
			Debug.WriteLine($"Activity list length = {activities.Length}");
			ActivityEvents = aggregateActivitiesAsync(activities);
		}


		/// <summary>
		/// Parses the output of the CMMotionActivityManager or activityList, removing low confidence and 
		/// unknown activities and agreggating the remaining into ActivityEvents.
		/// </summary>
		/// <returns>The list of distinct activity periods.</returns>
		/// <param name="activities">Activities.</param>
		List<ActivityEvent> aggregateActivitiesAsync(CMMotionActivity[] activities) {
			var filteredActivities = new List<CMMotionActivity>();

			Debug.WriteLine($"1 {activities.Length}");
			// Skip all contiguous unclassified and stationary activities so that only one remains.
			for(int i = 0; i < activities.Length; ++i) {
				CMMotionActivity activity = activities[i];
				filteredActivities.Add(activity);

				if(activity.Unknown || activity.Stationary) {
					while(++i < activities.Length) {
						CMMotionActivity skipActiv = activities[i];
						if(!skipActiv.Unknown && !skipActiv.Stationary) {
							i = i - 1;
							break;
						}
					}
				}
			}
			Debug.WriteLine($"2 {filteredActivities.Count}");
			// Ignore all low confidence activities.
			for(int i = 0; i < filteredActivities.Count;) {
				CMMotionActivity activity = filteredActivities[i];
				if(activity.Confidence == CMMotionActivityConfidence.Low) {
					filteredActivities.RemoveAt(i);
				}
				else {
					++i;
				}
			}
			Debug.WriteLine($"3 {filteredActivities.Count}");
			// Skip all unclassified and stationary activities if their duration is smaller than
			// some threshold.  This has the effect of coalescing the remaining med + high
			// confidence activities together.
			for(int i = 0; i < filteredActivities.Count - 1;) {
				CMMotionActivity activity = filteredActivities[i];
				CMMotionActivity nextActivity = filteredActivities[i + 1];

				var duration = nextActivity.StartDate.SecondsSinceReferenceDate - activity.StartDate.SecondsSinceReferenceDate;
				const int THERESHOLD = 60 * 3;
				if(duration < THERESHOLD && (activity.Stationary || activity.Unknown)) {
					filteredActivities.RemoveAt(i);
				}
				else {
					++i;
				}
			}
			Debug.WriteLine($"4 {filteredActivities.Count}");
			// Coalesce activities where they differ only in confidence.
			for(int i = 1; i < filteredActivities.Count;) {
				CMMotionActivity prevActivity = filteredActivities[i - 1];
				CMMotionActivity activity = filteredActivities[i];

				if((prevActivity.Walking && activity.Walking) ||
					(prevActivity.Running && activity.Running) ||
					(prevActivity.Cycling && activity.Cycling) ||
					(prevActivity.Automotive && activity.Automotive)) {
					filteredActivities.RemoveAt(i);
				}
				else {
					++i;
				}
			}
			Debug.WriteLine($"5 {filteredActivities.Count}");
			// Finally, create ActivityEvents, which are periods where users used activity A from time X to Y.
			var activityEvents = new List<ActivityEvent>();

			for(int i = 0; i < filteredActivities.Count - 1; i++) {
				CMMotionActivity activity = filteredActivities[i];
				CMMotionActivity nextActivity = filteredActivities[i + 1];

				Debug.WriteLine($"{activity.DebugDescription} - {nextActivity.DebugDescription}");
				var activityType = ActivityToType(activity);

				if(activityType == ActivityType.Unknown || activityType == ActivityType.Stationary)
					continue;

				var activityEvent = new ActivityEvent(activityType,
					NSDateConverter.ToDateTime(activity.StartDate),
					NSDateConverter.ToDateTime(nextActivity.StartDate));
				Debug.WriteLine(activityEvent.ToString());

				activityEvents.Add(activityEvent);
				ActivityToDuration(activityEvent.ActivityType, activityEvent.ActivityDurationInSeconds());
			}
			activityList.Clear();
			return activityEvents;
		}


		#region Utility
		public static ActivityType ActivityToType(CMMotionActivity activity) {
			if(activity.Cycling)
				return ActivityType.Cycling;
			if(activity.Running)
				return ActivityType.Running;
			if(activity.Walking)
				return ActivityType.Walking;
			if(activity.Automotive)
				return ActivityType.Automative;
			if(activity.Stationary)
				return ActivityType.Stationary;

			return ActivityType.Unknown;
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
