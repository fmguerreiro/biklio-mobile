using System;

namespace Trace {
	public class ActivityEvent {

		private DateTime startDate;

		private DateTime endDate;

		private ActivityType activityType;
		public ActivityType ActivityType {
			get {
				return activityType;
			}
		}

		public ActivityEvent(ActivityType type, DateTime start, DateTime end) {
			activityType = type;
			startDate = start;
			endDate = end;
		}

		public long ActivityDurationInSeconds() {
			return (long) (endDate - startDate).TotalSeconds;
		}
	}
}
