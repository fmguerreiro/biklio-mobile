using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;

namespace Trace {

	public static class GeoUtils {

		static int GET_LOCATION_TIMEOUT = 10000;

		/// <summary>
		/// Attempts to get the user location within a specified timeout.
		/// </summary>
		/// <returns>The current user location.</returns>
		public static async Task<Position> GetCurrentUserLocation() {
			Position location;
			try {
				location = await CrossGeolocator.Current.GetPositionAsync(GET_LOCATION_TIMEOUT);
			}
			catch(GeolocationException) { return new Position { Longitude = 0, Latitude = 0 }; }
			return location;
		}


		/// <summary>
		/// Calculates the distance between points using Equirectangular approximation.
		/// This does not take into account the arc of the Earth, but is much quicker than more accurate options and acceptable for our purposes.
		/// </summary>
		/// <returns>The distance in meters.</returns>
		/// <param name="pos1">Position 1.</param>
		/// <param name="pos2">Position 2.</param>
		public static double DistanceBetweenPoints(Position pos1, Position pos2) {
			const int EARTH_RADIUS_METERS = 6371009;

			var lng1 = Math.Abs(pos1.Longitude);
			var lng2 = Math.Abs(pos2.Longitude);
			var alpha = lng2 - lng1;
			var lat1 = Math.Abs(pos1.Latitude);
			var lat2 = Math.Abs(pos2.Latitude);
			var x = (alpha * (Math.PI / 180)) * Math.Cos((lat1 + lat2) * (Math.PI / 180) / 2);
			var y = (Math.PI / 180) * (lat1 - lat2);
			return Math.Sqrt(x * x + y * y) * EARTH_RADIUS_METERS;
		}
	}
}
