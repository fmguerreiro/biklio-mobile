using System;
using CoreMotion;
using Foundation;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Xamarin.Forms;
using System.Threading;
using UIKit;

[assembly: Dependency(typeof(Trace.iOS.MotionActivityManager))]
namespace Trace.iOS {

	public class MotionActivityManager : IMotionActivityManager {

		public IList<ActivityEvent> ActivityEvents { get; set; }
		public int WalkingDuration { get; set; }
		public int RunningDuration { get; set; }
		public int CyclingDuration { get; set; }
		public int DrivingDuration { get; set; }
		public bool IsInitialized { get; set; }

		private const double AVG_WALKING_SPEED_IN_METERS_S = 1.5;
		public double CurrentAvgSpeed { get; set; }
		//private const double ACCELEROMETER_INTERVAL_S = 6.4;
		CMMotionActivityManager motionActivityMgr;
		//CMMotionManager motionMgr;
		//int nAccelerometerReadings;
		//double accelerometerAvg;
		//CMPedometer pedometer;

		IList<CMMotionActivity> activityList;


		public void InitMotionActivity() {
			if(IsInitialized) return;
			motionActivityMgr = new CMMotionActivityManager();
			//pedometer = new CMPedometer();
			//motionMgr = new CMMotionManager();
			//motionMgr.AccelerometerUpdateInterval = ACCELEROMETER_INTERVAL_S;
			//motionMgr.StartAccelerometerUpdates();
			//var queue = new NSOperationQueue();
			//motionMgr.StartAccelerometerUpdates(NSOperationQueue.MainQueue, (CMAccelerometerData data, NSError error) => {
			//	accelerometerAvg = (accelerometerAvg + data.Acceleration.X) / ++nAccelerometerReadings;
			//	DependencyService.Get<INotificationMessage>().Send("accelerometer", "accelerometer", $"x:{data.Acceleration.X}\ny:{data.Acceleration.Y}\nz:{data.Acceleration.Z}\navg:{accelerometerAvg}", 1);
			//});
			ActivityEvents = new List<ActivityEvent>();
			activityList = new List<CMMotionActivity>();
			IsInitialized = true;
		}


		public void StartMotionUpdates(Action<ActivityType> handler) {
			if(Geolocator.IsTrackingInProgress) {
				motionActivityMgr.StartActivityUpdates(new NSOperationQueue(), ((activity) => {
					//Debug.WriteLine($"{activity.DebugDescription}");

					// A CMMotionActivity can have several modes set to true. We prioritize bycicle events. 
					if(activity.Cycling) {
						if(Geolocator.IsTrackingInProgress) activityList.Add(activity);
						//UpdateAccelerometerData();
						handler(ActivityType.Cycling);
					}
					else if(activity.Confidence != CMMotionActivityConfidence.Low) {
						if(Geolocator.IsTrackingInProgress) activityList.Add(activity);
						//UpdateAccelerometerData();
						handler(ActivityToType(activity));
					}
				}));
			}
		}


		public void StopMotionUpdates() {
			motionActivityMgr.StopActivityUpdates();
			IsInitialized = false;
		}


		//public async Task QueryPedometer(NSDate start, NSDate end) {
		//	var pedometerData = await pedometer.QueryPedometerDataAsync(start, end);
		//	Debug.WriteLine($"pedometer: description {pedometerData.Description}");
		//	DependencyService.Get<INotificationMessage>().Send("pedometer", "Pedometer", $"nr steps: {pedometerData.NumberOfSteps}\ndistance: {pedometerData.Distance}", 1);
		//}


		//private void UpdateAccelerometerData() {
		//	var xG = Math.Abs(motionMgr.AccelerometerData.Acceleration.X);
		//	var yG = Math.Abs(motionMgr.AccelerometerData.Acceleration.Y);
		//	var zG = Math.Abs(motionMgr.AccelerometerData.Acceleration.Z);

		//	if(!App.Current.Properties.ContainsKey("accelerometerN")) {
		//		App.Current.Properties.Add("accelerometerN", 1);
		//		App.Current.Properties.Add("accelerometerMaxX", xG);
		//		App.Current.Properties.Add("accelerometerMaxY", yG);
		//		App.Current.Properties.Add("accelerometerMaxZ", zG);
		//		App.Current.Properties.Add("accelerometerAvgX", xG);
		//		App.Current.Properties.Add("accelerometerAvgY", yG);
		//		App.Current.Properties.Add("accelerometerAvgZ", zG);
		//	}

		//	var n = (int) App.Current.Properties["accelerometerN"];
		//	App.Current.Properties["accelerometerN"] = n++;

		//	var maxX = (double) App.Current.Properties["accelerometerMaxX"];
		//	var maxY = (double) App.Current.Properties["accelerometerMaxY"];
		//	var maxZ = (double) App.Current.Properties["accelerometerMaxZ"];

		//	var avgX = ((double) App.Current.Properties["accelerometerAvgX"] + xG) / n;
		//	var avgY = ((double) App.Current.Properties["accelerometerAvgY"] + yG) / n;
		//	var avgZ = ((double) App.Current.Properties["accelerometerAvgZ"] + zG) / n;

		//	if(xG > maxX) App.Current.Properties["accelerometerMaxX"] = xG;
		//	if(yG > maxY) App.Current.Properties["accelerometerMaxY"] = yG;
		//	if(zG > maxZ) App.Current.Properties["accelerometerMaxZ"] = zG;

		//	App.Current.Properties["accelerometerAvgX"] = avgX;
		//	App.Current.Properties["accelerometerAvgX"] = avgY;
		//	App.Current.Properties["accelerometerAvgZ"] = avgZ;
		//}


		public void Reset() {
			WalkingDuration = 0;
			RunningDuration = 0;
			CyclingDuration = 0;
			DrivingDuration = 0;
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
			if(!IsInitialized) { InitMotionActivity(); }
			await QueryHistoricalData(NSDateConverter.ToNsDate(start), NSDateConverter.ToNsDate(end));
		}

		public async Task QueryHistoricalData(NSDate start, NSDate end) {
			CMMotionActivity[] activities = null;
			if(activityList.Count == 0) {
				Debug.WriteLine($"QueryHistoricalData: {start} - {end}");
				try {
					activities = await motionActivityMgr.QueryActivityAsync(
						start, end, NSOperationQueue.MainQueue
					);
					// If there were no activities recorded (CMMotionManager has several minute delay), loop until we have a few.
					const int MIN_ACTIVITY_THRESHOLD = 6;
					if(activities.Length < MIN_ACTIVITY_THRESHOLD && !Geolocator.IsTrackingInProgress) {
						var minActivities = MIN_ACTIVITY_THRESHOLD;
						var _ = new List<CMMotionActivity>(activities);
						motionActivityMgr.StartActivityUpdates(new NSOperationQueue(), (activity) => {
							if(activity.Confidence == CMMotionActivityConfidence.Low) return;
							Debug.WriteLine($"QueryHistoricalData - handler: {ActivityToType(activity)}, bg task seconds remaining {UIApplication.SharedApplication.BackgroundTimeRemaining}");
							//DependencyService.Get<INotificationMessage>().Send("handlerthread", "handlerthread", Thread.CurrentThread.ManagedThreadId.ToString(), 0);
							_.Add(activity);
							minActivities--;
						});

						// Don't let this thread continue until we have at least MIN activities OR background run time is running out OR app has come to the foreground.
						SpinWait.SpinUntil(() => minActivities == 0 ||
												 UIApplication.SharedApplication.BackgroundTimeRemaining < 10 ||
												 App.IsInForeground);
						motionActivityMgr.StopActivityUpdates();

						// TODO make direct date conversion instead of NSDate -> DateTime -> long
						_.ForEach((x) => RewardEligibilityManager.Instance.Input(ActivityToType(x), NSDateConverter.ToDateTime(x.StartDate).DatetimeToEpochSeconds()));

						activities = _.ToArray();


#if DEBUG
						var res = "";
						_.ForEach((x) => res += $"{x.StartDate} {ActivityToType(x)}\n");
						Debug.WriteLine($"QueryHistoricalData: got {MIN_ACTIVITY_THRESHOLD} activities:\n" + res);
						App.DEBUG_ActivityLog += res;
#endif
					}
					//await QueryPedometer(start, end);
				}
				catch(Exception) { Debug.WriteLine("No activities found for this trajectory"); return; }
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

			Debug.WriteLine($"aggregateActivitiesAsync step 1, nrActivities = {activities.Length}");
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
			Debug.WriteLine($"aggregateActivitiesAsync step 2, nrActivities = {filteredActivities.Count}");
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
			Debug.WriteLine($"aggregateActivitiesAsync step 3, nrActivities = {filteredActivities.Count}");
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
			Debug.WriteLine($"aggregateActivitiesAsync step 4, nrActivities = {filteredActivities.Count}");
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
			Debug.WriteLine($"aggregateActivitiesAsync step 5, nrActivities = {filteredActivities.Count}");
			// Finally, create ActivityEvents, which are periods where users used activity A from time X to Y.
			var activityEvents = new List<ActivityEvent>();

			for(int i = 0; i < filteredActivities.Count - 1; i++) {
				CMMotionActivity activity = filteredActivities[i];
				CMMotionActivity nextActivity = filteredActivities[i + 1];

				//Debug.WriteLine($"{activity.DebugDescription} - {nextActivity.DebugDescription}");
				var activityType = ActivityToType(activity);

				if(activityType == ActivityType.Unknown || activityType == ActivityType.Stationary)
					continue;

				var activityEvent = new ActivityEvent(activityType,
					NSDateConverter.ToDateTime(activity.StartDate),
					NSDateConverter.ToDateTime(nextActivity.StartDate));
				Debug.WriteLine($"aggregateActivitiesAsync significant EVENT detected: {activityEvent.ToString()}");

				activityEvents.Add(activityEvent);
				ActivityToDuration(activityEvent.ActivityType, activityEvent.ActivityDurationInSeconds());
			}

			//if(activityList == null || activityList.Count == 0) {
			//	insertActivitiesIntoStateMachine(activityEvents);
			//}

			activityList.Clear();
			return activityEvents;
		}


		//void insertActivitiesIntoStateMachine(List<ActivityEvent> activityEvents) {
		//	foreach(var actEvent in activityEvents) {
		//		RewardEligibilityManager.Instance.Input(actEvent.ActivityType, (int) (actEvent.ActivityDurationInSeconds() * 1000));
		//	}
		//}


		#region Utility
		public static ActivityType ActivityToType(CMMotionActivity activity) {
			if(activity.Cycling)
				return ActivityType.Cycling;
			if(activity.Running)
				return ActivityType.Running;

			// iOS does not detect cycling automatically (tested on 10.0.2).
			// All cycling activity comes back as walking, so:
			if(activity.Walking) {
				// we take the user velocity into account before declaring it as either cycling or walking
				if(DependencyService.Get<IMotionActivityManager>().CurrentAvgSpeed > AVG_WALKING_SPEED_IN_METERS_S) {
					return ActivityType.Cycling;
				}
				return ActivityType.Walking;
			}

			if(activity.Automotive)
				return ActivityType.Automative;
			if(activity.Stationary)
				return ActivityType.Stationary;

			//return ActivityType.Unknown;
			return ActivityType.Cycling; // Better to err on the side of the user.
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
