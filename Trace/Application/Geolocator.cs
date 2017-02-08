using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {

	public static class Geolocator {

		// Location precision in meters.
		public const int LOCATOR_GOOD_ACCURACY = 15;
		private const int MOTION_ONLY_ACCURACY = 50;
		private const int ZOOM_DISTANCE_KM = 1;

		private static IGeolocator locator;

		//private static IList<Plugin.Geolocator.Abstractions.Position> positions;

		public static bool IsTrackingInProgress { get; set; }
		public static TraceMap Map { get; set; }

		public static double MaxSpeed { get; set; }

		private static int nSamples = 1;
		private static double cumulativeAvg = 3;
		public static double CumulativeAvgSpeed {
			get {
				return cumulativeAvg;
			}
			set {
				cumulativeAvg = (cumulativeAvg + value / ++nSamples);
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
				DependencyService.Get<IMotionActivityManager>().CurrentAvgSpeed = args.Position.Speed;
				CumulativeAvgSpeed = args.Position.Speed;
				Map.RouteCoordinates.Add(args.Position);
			}
		}


		/// <summary>
		/// Starts the GPS for using iOS's deferred location update feature.
		/// After 5 minutes or 100m pass, trigger the location background update.
		/// </summary>
		/// <returns>The in background.</returns>
		//public static async Task StartInBackground() {
		//	locator = CrossGeolocator.Current;

		//	if(locator.IsListening)
		//		await locator.StopListeningAsync();

		//	locator.DesiredAccuracy = LOCATOR_GOOD_ACCURACY;
		//	var locationSettings = new ListenerSettings {
		//		AllowBackgroundUpdates = true,
		//		DeferLocationUpdates = true,
		//		DeferralDistanceMeters = 1,
		//		DeferralTime = new TimeSpan(0, 0, 30)
		//	};

		//	var firstPosition = await GeoUtils.GetCurrentUserLocation();
		//	if(positions == null) {
		//		positions = new List<Plugin.Geolocator.Abstractions.Position>();
		//	}
		//	positions.Clear();
		//	positions.Add(firstPosition);

		//	await locator.StartListeningAsync(5000, 15, false, locationSettings);
		//	locator.PositionChanged += onDeferredPositionChange;
		//}


		//private static void onDeferredPositionChange(object sender, PositionEventArgs args) {
		//	Debug.WriteLine("onDeferredPositionChange");
		//	var firstPos = positions.First();
		//	var secondPos = args.Position;
		//	double distance = GeoUtils.DistanceBetweenPoints(firstPos, secondPos);
		//	double time = Math.Abs((secondPos.Timestamp - firstPos.Timestamp).TotalSeconds);

		//	var speed = distance / time;

		//	DependencyService.Get<IMotionActivityManager>().CurrentAvgSpeed = speed;

		//	positions.Clear();
		//	positions.Add(secondPos);

		//	DependencyService.Get<IMotionActivityManager>().QueryHistoricalData(
		//		firstPos.Timestamp.DateTime, secondPos.Timestamp.DateTime
		//	);
		//	// TODO DEBUG notification -- remove
		//	DependencyService.Get<INotificationMessage>().Send(
		//		"deferredPosition",
		//		"deferredPosition m/s",
		//		$"distance {distance}\ntime {time} -> speed {speed}", 0
		//	);
		//}


		public static async Task Stop() {
			if(locator != null) {
				locator.PositionChanged -= onPositionChanged;
				//locator.PositionChanged -= onDeferredPositionChange;
				//positions?.Clear();
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
			cumulativeAvg = 3;
			nSamples = 1;
			MaxSpeed = 0;
		}
	}
}
