using System;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {
	public partial class MapPage : ContentPage {

		private Geolocator Locator;
		private CurrentActivity currentActivity;

		private DateTime StartTrackingTime;
		private DateTime StopTrackingTime;

		private string activityLogResult = "";

		public MapPage() {
			InitializeComponent();
			Locator = new Geolocator(CustomMap);
			Locator.Start();
			currentActivity = new CurrentActivity();
			DependencyService.Get<MotionActivityInterface>().InitMotionActivity();
		}

		void OnStartTracking(object send, EventArgs eventArgs) {

			// On Stop Button pressed
			if(Locator.IsTrackingInProgress) {
				// Calculate stats and the results to the user.
				//DependencyService.Get<MotionActivityInterface>().StopMotionUpdates();
				((Button) send).Text = "Track";
				StopTrackingTime = DateTime.Now;
				//DependencyService.Get<MotionActivityInterface>().QueryHistoricalData(StartTrackingTime, StopTrackingTime);
				ActivityLabel.IsVisible = false;

				// Refresh the map to display the trajectory.
				MyStack.Children.RemoveAt(0);
				MyStack.Children.Insert(0, CustomMap);

				// Make the results grid visible to the user.
				ResultsGrid.IsVisible = true;

				// Calculate route distance.
				double distanceInMeters = 0;
				var coordinates = CustomMap.RouteCoordinates.GetEnumerator();
				coordinates.MoveNext();
				var pos1 = coordinates.Current;
				while(coordinates.MoveNext()) {
					var pos2 = coordinates.Current;
					distanceInMeters += distanceBetweenPoints(pos1, pos2);
					pos1 = pos2;
				}

				TotalDistanceLabel.BindingContext = new TotalDistance { Distance = (long) distanceInMeters };

				TimeSpan elapsedTime = StopTrackingTime.Subtract(StartTrackingTime);
				DurationLabel.BindingContext = new TotalDuration {
					Hours = elapsedTime.Hours,
					Minutes = elapsedTime.Minutes,
					Seconds = elapsedTime.Seconds
				};
				CaloriesLabel.BindingContext = new TotalCalories { Calories = 0 };

				DrivenLabel.BindingContext = new TotalDuration { Hours = 0, Minutes = 0, Seconds = 0 };
				//MainActivityLabel.BindingContext = currentActivity;
				DisplayAlert("Activity log", activityLogResult, "Ok");
			}

			// On Track Button pressed
			else {
				((Button) send).Text = "Stop";
				CustomMap.RouteCoordinates.Clear();
				StartTrackingTime = DateTime.Now;
				activityLogResult = "";
				// Show Activity text after Map and before Stop button and remove Results grid.
				ActivityLabel.IsVisible = true;
				ResultsGrid.IsVisible = false;
				ActivityLabel.BindingContext = currentActivity;
				DependencyService.Get<MotionActivityInterface>().StartMotionUpdates((activity) => {
					currentActivity.ActivityType = activity;
					activityLogResult += DateTime.Now + ": " + activity + "\n";
				});
			}
			Locator.IsTrackingInProgress = !Locator.IsTrackingInProgress;
		}

		/// <summary>
		/// Calculate the distance between points using Equirectangular approximation.
		/// This does not take into account the arc of the Earth, but is much quicker and acceptable for our purposes.
		/// </summary>
		/// <returns>The distance in meters.</returns>
		/// <param name="pos1">Position 1.</param>
		/// <param name="pos2">Position 2.</param>
		private double distanceBetweenPoints(Position pos1, Position pos2) {
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