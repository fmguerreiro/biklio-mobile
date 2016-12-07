using System;

namespace Trace {
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
	}
}
