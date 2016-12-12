using System;
using System.Threading.Tasks;
using System.Collections.Generic;

// TODO NOT YET IMPLEMENTED!!!
[assembly: Xamarin.Forms.Dependency(typeof(Trace.Droid.MotionActivityManager))]
namespace Trace.Droid {
	public class MotionActivityManager : IMotionActivityManager {
		//private CMMotionActivityManager motionActivityMgr;

		public override void InitMotionActivity() {
			//motionActivityMgr = new CMMotionActivityManager();
			ActivityEvents = new List<ActivityEvent>();
			WalkingDuration = 0;
			RunningDuration = 0;
			CyclingDuration = 0;
			AutomativeDuration = 0;
		}

		private void reset() {
			WalkingDuration = 0;
			RunningDuration = 0;
			CyclingDuration = 0;
			AutomativeDuration = 0;
			ActivityEvents.Clear();
		}

		public override void StartMotionUpdates(Action<ActivityType> handler) {
			//motionActivityMgr.StartActivityUpdates(NSOperationQueue.MainQueue, ((activity) => {
			//	handler(ActivityToType(activity));
			//}));
		}

		public override void StopMotionUpdates() {
			//motionActivityMgr.StopActivityUpdates();
		}

		public override void Reset() {
			//throw new NotImplementedException();
		}

		public override ActivityType GetMostCommonActivity() {
			return ActivityType.Unknown;
		}

		public override async Task QueryHistoricalData(DateTime start, DateTime end) {
			//await queryHistoricalDataAsync(NSDateConverter.ToNSDate(start), NSDateConverter.ToNSDate(end));
		}

		//private async Task queryHistoricalDataAsync(NSDate startDate, NSDate endDate) {
		//	var activities = await motionActivityMgr.QueryActivityAsync(startDate, endDate, NSOperationQueue.MainQueue);
		//	ActivityEvents = aggregateActivitiesAsync(activities);
		//}

		/// <summary>
		/// Parses the output of the CMMotionActivityManager, removing low confidence and 
		/// unknown activities and agreggating the remaining into ActivityEvents.
		/// </summary>
		/// <returns>The list of distinct activity periods.</returns>
		/// <param name="activities">Activities.</param>
		//List<ActivityEvent> aggregateActivitiesAsync(CMMotionActivity[] activities) {
		//	//List<CMMotionActivity> filteredActivities = new List<CMMotionActivity>();

		//	//// Skip all contiguous unclassified and stationary activities so that only one remains.
		//	//for(int i = 0; i < activities.Length; ++i) {
		//	//	CMMotionActivity activity = activities[i];
		//	//	filteredActivities.Add(activity);

		//	//	if(activity.Unknown || activity.Stationary) {
		//	//		while(++i < activities.Length) {
		//	//			CMMotionActivity skipActiv = activities[i];
		//	//			if(!skipActiv.Unknown && !skipActiv.Stationary) {
		//	//				i = i - 1;
		//	//				break;
		//	//			}
		//	//		}
		//	//	}
		//	//}

		//	//// Ignore all low confidence activities.
		//	//for(int i = 0; i < filteredActivities.Count;) {
		//	//	CMMotionActivity activity = filteredActivities[i];
		//	//	if(activity.Confidence == CMMotionActivityConfidence.Low) {
		//	//		filteredActivities.RemoveAt(i);
		//	//	}
		//	//	else {
		//	//		++i;
		//	//	}
		//	//}

		//	//// Skip all unclassified and stationary activities if their duration is smaller than
		//	//// some threshold.  This has the effect of coalescing the remaining med + high
		//	//// confidence activities together.
		//	//for(int i = 0; i < filteredActivities.Count - 1;) {
		//	//	CMMotionActivity activity = filteredActivities[i];
		//	//	CMMotionActivity nextActivity = filteredActivities[i + 1];

		//	//	var duration = nextActivity.StartDate.SecondsSinceReferenceDate - activity.StartDate.SecondsSinceReferenceDate;
		//	//	const int THERESHOLD = 60 * 3;
		//	//	if(duration < THERESHOLD && (activity.Stationary || activity.Unknown)) {
		//	//		filteredActivities.RemoveAt(i);
		//	//	}
		//	//	else {
		//	//		++i;
		//	//	}
		//	//}

		//	//// Coalesce activities where they differ only in confidence.
		//	//for(int i = 1; i < filteredActivities.Count;) {
		//	//	CMMotionActivity prevActivity = filteredActivities[i - 1];
		//	//	CMMotionActivity activity = filteredActivities[i];

		//	//	if((prevActivity.Walking && activity.Walking) ||
		//	//		(prevActivity.Running && activity.Running) ||
		//	//		(prevActivity.Cycling && activity.Cycling) ||
		//	//		(prevActivity.Automotive && activity.Automotive)) {
		//	//		filteredActivities.RemoveAt(i);
		//	//	}
		//	//	else {
		//	//		++i;
		//	//	}
		//	//}

		//	//// Finally transform into ActivityEvent and increment duration
		//	//var activityEvents = new List<ActivityEvent>();

		//	//for(int i = 0; i < filteredActivities.Count - 1; i++) {
		//	//	CMMotionActivity activity = filteredActivities[i];
		//	//	CMMotionActivity nextActivity = filteredActivities[i + 1];

		//	//	if(activity.Unknown || activity.Stationary)
		//	//		continue;

		//	//	var activityEvent = new ActivityEvent(ActivityToType(activity),
		//	//		NSDateConverter.ToDateTime(activity.StartDate),
		//	//		NSDateConverter.ToDateTime(nextActivity.StartDate));

		//	//	activityEvents.Add(activityEvent);
		//	//	ActivityToDuration(activityEvent.ActivityType, activityEvent.ActivityDurationInSeconds());
		//	//}

		//	//return activityEvents;
		//}

		#region Utility
		//public static ActivityType ActivityToType(CMMotionActivity activity) {
		//	if(activity.Walking)
		//		return ActivityType.Walking;
		//	if(activity.Running)
		//		return ActivityType.Running;
		//	if(activity.Automotive)
		//		return ActivityType.Automative;
		//	if(activity.Stationary)
		//		return ActivityType.Stationary;
		//	if(activity.Cycling)
		//		return ActivityType.Cycling;
		//	else
		//		return ActivityType.Unknown;
		//}

		public static string ActivityTypeToString(ActivityType type) {
			switch(type) {
				case ActivityType.Walking:
					return "Walking";
				case ActivityType.Running:
					return "Running";
				case ActivityType.Automative:
					return "Automotive";
				case ActivityType.Stationary:
					return "Stationary";
				case ActivityType.Cycling:
					return "Cycling";
				default:
					return "Unknown";
			}
		}

		public void ActivityToDuration(ActivityType type, long duration) {
			switch(type) {
				case ActivityType.Walking:
					WalkingDuration += duration;
					break;
				case ActivityType.Running:
					RunningDuration += duration;
					break;
				case ActivityType.Cycling:
					CyclingDuration += duration;
					break;
				case ActivityType.Automative:
					AutomativeDuration += duration;
					break;
			}
		}

		#endregion
	}
}
