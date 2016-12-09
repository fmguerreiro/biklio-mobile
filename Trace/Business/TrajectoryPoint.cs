using Plugin.Geolocator.Abstractions;

namespace Trace {

	public class TrajectoryPoint {

		public double Longitude { get; set; }

		public double Latitude { get; set; }

		public long Timestamp { get; set; }

		public ActivityType Activity { get; set; }

		public Position ToPosition() {
			var position = new Position();
			position.Longitude = Longitude;
			position.Latitude = Latitude;
			return position;
		}
	}
}
