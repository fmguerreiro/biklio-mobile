using System;
using System.Diagnostics;
using Foundation;

namespace Trace.iOS {
	public static class NSDateConverter {

		public static NSDate ToNsDate(DateTime datetime) {
			//Debug.WriteLine(TimeUtil.SecondsToHHMMSS((long) datetime.DatetimeToEpochSeconds()));
			DateTime newDate = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(2001, 1, 1, 0, 0, 0));
			var nsDate = NSDate.FromTimeIntervalSinceReferenceDate((datetime - newDate).TotalSeconds);
			//Debug.WriteLine(TimeUtil.SecondsToHHMMSS((long) nsDate.SecondsSinceReferenceDate));
			return nsDate;
		}

		public static DateTime ToDateTime(NSDate date) {
			//Debug.WriteLine(TimeUtil.SecondsToHHMMSS((long) date.SecondsSinceReferenceDate));
			DateTime newDate = TimeZone.CurrentTimeZone.ToUniversalTime(
				new DateTime(2001, 1, 1, 0, 0, 0));
			//Debug.WriteLine(TimeUtil.SecondsToHHMMSS((long) newDate.DatetimeToEpochSeconds()));
			return newDate.AddSeconds(date.SecondsSinceReferenceDate);
		}
	}
}