using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms.Maps;

namespace Trace {

	public static class Geolocator {

		// Location precision in meters.
		public const int LOCATOR_GOOD_ACCURACY = 15;
		private const int MOTION_ONLY_ACCURACY = 30;
		private const int ZOOM_DISTANCE_KM = 1;

		private static IGeolocator locator;

		public static bool IsTrackingInProgress { get; set; }
		public static TraceMap Map { get; set; }

		public static double MaxSpeed { get; set; }

		private static int nSamples;
		private static double cumulativeAvg = 1.5;
		public static double CumulativeAvgSpeed {
			get {
				return cumulativeAvg;
			}
			set {
				if(nSamples++ == 0) { cumulativeAvg = value; return; }
				cumulativeAvg = (cumulativeAvg + value / nSamples);
			}
		}


		public static async Task Start() {
			locator = CrossGeolocator.Current;

			if(locator.IsListening)
				await locator.StopListeningAsync();

			locator.DesiredAccuracy = LOCATOR_GOOD_ACCURACY;
			var locationSettings = new ListenerSettings {
				AllowBackgroundUpdates = true
			};

			// Get first position
			var position = await GeoUtils.GetCurrentUserLocation();
			UpdateMap(position);

			// Listen to position changes and update map
			try {
				if(!locator.IsListening)
					await locator.StartListeningAsync(minTime: 5000, minDistance: 15, includeHeading: false, settings: locationSettings);
			}
			catch(Exception e) { Debug.WriteLine(e); return; }

			locator.PositionChanged += onPositionChanged;
		}


		private static void onPositionChanged(object sender, PositionEventArgs args) {
			if(IsTrackingInProgress) {
				UpdateMap(args.Position);
				if(args.Position.Speed > MaxSpeed) MaxSpeed = args.Position.Speed;
				CumulativeAvgSpeed = args.Position.Speed;
				Map.RouteCoordinates.Add(args.Position);
			}
		}


		public static async Task Stop() {
			if(locator != null) {
				locator.PositionChanged -= onPositionChanged;
				await locator.StopListeningAsync();
			}
		}


		public static void UpdateMap(Plugin.Geolocator.Abstractions.Position position) {
			Map.MoveToRegion(
				MapSpan.FromCenterAndRadius(
					new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude), Distance.FromKilometers(ZOOM_DISTANCE_KM)));
		}


		public static void UpdateMap(Xamarin.Forms.Maps.Position position) {
			Map.MoveToRegion(
				MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(ZOOM_DISTANCE_KM)));
		}


		public static void ImproveAccuracy() {
			if(locator != null)
				locator.DesiredAccuracy = LOCATOR_GOOD_ACCURACY;
		}


		public static void TryLowerAccuracy() {
			if(locator != null && !IsTrackingInProgress)
				locator.DesiredAccuracy = MOTION_ONLY_ACCURACY;
		}

		public static bool IsEnabled() {
			return CrossGeolocator.Current.IsGeolocationEnabled;
		}

		public static void Reset() {
			cumulativeAvg = 0;
			nSamples = 0;
			MaxSpeed = 0;
		}
	}
}
