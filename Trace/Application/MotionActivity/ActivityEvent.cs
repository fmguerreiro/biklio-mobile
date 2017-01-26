using System;

namespace Trace {
	/// <summary>
	/// An activity event is a period of time with the same sustained activity.
	/// These are created after the user finishes running, where the raw outputs from the activity detector are cleaned and aggregated.
	/// </summary>
	public class ActivityEvent {

		private readonly DateTime startDate;
		public DateTime EndDate { get; set; }

		private ActivityType activityType;
		public ActivityType ActivityType {
			get {
				return activityType;
			}
		}

		public ActivityEvent(ActivityType type) { activityType = type; }

		public ActivityEvent(ActivityType type, DateTime start, DateTime end) {
			activityType = type;
			startDate = start;
			EndDate = end;
		}

		public long ActivityDurationInSeconds() {
			return (long) (EndDate - startDate).TotalSeconds;
		}

		public override string ToString() {
			return $"{activityType}, {TimeUtil.SecondsToHHMMSS(startDate.DatetimeToEpochSeconds())} -> {TimeUtil.SecondsToHHMMSS(EndDate.DatetimeToEpochSeconds())}";
		}
	}
}
