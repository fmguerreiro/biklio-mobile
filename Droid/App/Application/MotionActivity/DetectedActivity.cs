using System;
namespace Trace.Droid {
	public class DetectedActivity : Android.Gms.Location.DetectedActivity {

		public DateTime Timestamp { get; set; }

		public DetectedActivity(int activityType, int confidence, DateTime ts)
			: base(activityType, confidence) {
			Timestamp = ts;
		}
	}
}