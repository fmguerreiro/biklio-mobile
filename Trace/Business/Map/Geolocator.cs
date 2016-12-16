using System.Threading.Tasks;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using Xamarin.Forms.Maps;

namespace Trace {

	// TODO: reduce accuracy to 1 when user did not click 'track' to save battery.
	public class Geolocator {
		private const int LOCATOR_TIMEOUT = 1000000;
		private const int MOTION_ONLY_ACCURACY = 1;
		private const int LOCATOR_ACCURACY = 50;

		public bool IsTrackingInProgress { get; set; }
		private readonly TraceMap Map;

		public double MaxSpeed;
		public double AvgSpeed;

		public Geolocator(TraceMap map) {
			Map = map;
			IsTrackingInProgress = false;
		}

		public async Task Start() {
			var locator = CrossGeolocator.Current;
			/*if(!locator.IsGeolocationEnabled) {
				await DisplayAlert("", "GPS is disabled, please enable it and come back", "Return");
				return;
			}*/
			if(locator.IsListening)
				await locator.StopListeningAsync();

			locator.DesiredAccuracy = LOCATOR_ACCURACY;
			var locationSettings = new ListenerSettings() {
				AllowBackgroundUpdates = true,
				PauseLocationUpdatesAutomatically = true,
				//ListenForSignificantChanges = true
			};

			// Get first position
			var position = await locator.GetPositionAsync(timeoutMilliseconds: LOCATOR_TIMEOUT);
			UpdateMap(position);

			// Listen to position changes and update map
			await locator.StartListeningAsync(minTime: 5000, minDistance: 15, includeHeading: false, settings: locationSettings);
			locator.PositionChanged += (sender, e) => {
				if(IsTrackingInProgress) {
					UpdateMap(e.Position);
					if(e.Position.Speed > MaxSpeed) MaxSpeed = e.Position.Speed;
					AvgSpeed += e.Position.Speed;
					Map.RouteCoordinates.Add(e.Position);
				}
			};
		}

		private void UpdateMap(Plugin.Geolocator.Abstractions.Position position) {
			Map.MoveToRegion(
				MapSpan.FromCenterAndRadius(
					new Xamarin.Forms.Maps.Position(position.Latitude, position.Longitude), Distance.FromKilometers(1)));
		}
	}
}
