using System;

namespace Trace {

	public static class TimeUtil {

		public static long CurrentEpochTimeSeconds() {
			TimeSpan t = DateTime.UtcNow - new DateTime(1970, 1, 1);
			return (int) t.TotalSeconds;
		}


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


		public static bool IsWithinPeriod(long time, long start, long end) {
			return time > start && time < end;
		}


		public static string SecondsToHHMMSS(long seconds) {
			string format = @"HH\:mm\:ss";
			return EpochSecondsToDatetime(seconds).ToString(format);
		}


		public static string SecondsToHHMM(long seconds) {
			string format = @"hh\:mm";
			return EpochSecondsToDatetime(seconds).ToString(format);
		}


		public static string SecondsToFullDate(long seconds) {
			string format = @"dd\/MM\/yyyy HH:mm";
			return EpochSecondsToDatetime(seconds).ToString(format);
		}

		// TODO verify this
		public static long RoundDownDay(long epochTimeS) {
			// the 'Date' component has the time set to (00:00:00)
			return EpochSecondsToDatetime(epochTimeS).Date.DatetimeToEpochSeconds();
			//var dateInSeconds = DateTime.UtcNow - new DateTime(1970, 1, 1).Date;
			//return (long) dateInSeconds.TotalSeconds;
		}
	}
}
