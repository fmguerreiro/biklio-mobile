using System;

namespace Trace {

	public static class TimeConverter {

		public static DateTime EpochSecondsToDatetime(this long epochTime) {
			var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return dateTime.AddSeconds(epochTime);
		}

		public static long DatetimeToEpochSeconds(this DateTime date) {
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
		}

		public static long DatetimeToEpochSeconds(this DateTimeOffset date) {
			return DatetimeToEpochSeconds(date.UtcDateTime);
		}
	}
}
