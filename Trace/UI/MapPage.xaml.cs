using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
			//Task.Run(() => Locator.Start());
			Locator.Start();
			currentActivity = new CurrentActivity();
			DependencyService.Get<IMotionActivityManager>().InitMotionActivity();
		}

		// TODO offload cpu-intensive work off the UI thread, the slowdown is noticeable!
		async void OnStartTracking(object send, EventArgs eventArgs) {

			// On Stop Button pressed
			if(Locator.IsTrackingInProgress) {
				// Calculate stats and show the results to the user.
				StopTrackingTime = DateTime.Now;
				DependencyService.Get<IMotionActivityManager>().StopMotionUpdates();

				// Put all CPU-bound operations outside of UI thread.
				var calculateMotionTask = new Task(() =>
					DependencyService.Get<IMotionActivityManager>().QueryHistoricalData(StartTrackingTime, StopTrackingTime));
				var calculateDistanceTask = new Task<double>(calculateRouteDistance);

				calculateMotionTask.Start();
				calculateDistanceTask.Start();
				await Task.WhenAll(calculateMotionTask, calculateDistanceTask);

				double distanceInMeters = calculateDistanceTask.Result;
				Trajectory trajectory = await Task.Run(() => createTrajectory(distanceInMeters));
				User.Instance.Trajectories.Add(trajectory);
				SQLiteDB.Instance.SaveItem<Trajectory>(trajectory);

				Locator.AvgSpeed = 0;
				Locator.MaxSpeed = 0;

				TotalDistanceLabel.BindingContext = new TotalDistance { Distance = (long) distanceInMeters };
				TotalDuration duration = calculateRouteTime();
				DurationLabel.BindingContext = duration;

				// TODO Calculate calories ...
				CaloriesLabel.BindingContext = new TotalCalories { Calories = 0 };

				// TODO Calculate how much time was spent driving.
				// Probably also show time for each activity!
				DrivenLabel.BindingContext = new TotalDuration { Hours = 0, Minutes = 0, Seconds = 0 };
				displayGrid((Button) send);
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
				DependencyService.Get<IMotionActivityManager>().StartMotionUpdates((activity) => {
					currentActivity.ActivityType = activity;
					activityLogResult += DateTime.Now + ": " + activity + "\n";
				});
			}
			Locator.IsTrackingInProgress = !Locator.IsTrackingInProgress;
		}

		void displayGrid(Button trackButton) {
			trackButton.Text = "Track";
			ActivityLabel.IsVisible = false;
			// Refresh the map to display the trajectory.
			MyStack.Children.RemoveAt(0);
			MyStack.Children.Insert(0, CustomMap);

			// Make the results grid visible to the user.
			ResultsGrid.IsVisible = true;
		}

		double calculateRouteDistance() {
			double distanceInMeters = 0;
			var coordinates = CustomMap.RouteCoordinates.GetEnumerator();
			coordinates.MoveNext();
			var pos1 = coordinates.Current;
			while(coordinates.MoveNext()) {
				var pos2 = coordinates.Current;
				distanceInMeters += distanceBetweenPoints(pos1, pos2);
				pos1 = pos2;
			}
			return distanceInMeters;
		}

		TotalDuration calculateRouteTime() {
			TimeSpan elapsedTime = StopTrackingTime.Subtract(StartTrackingTime);
			return new TotalDuration {
				Hours = elapsedTime.Hours,
				Minutes = elapsedTime.Minutes,
				Seconds = elapsedTime.Seconds
			};
		}

		Trajectory createTrajectory(double distanceInMeters) {
			return new Trajectory {
				UserId = User.Instance.Id,
				StartTime = (long) (StartTrackingTime - new DateTime(1970, 1, 1)).TotalSeconds,
				EndTime = (long) (StopTrackingTime - new DateTime(1970, 1, 1)).TotalSeconds,
				AvgSpeed = (float) (Locator.AvgSpeed / CustomMap.RouteCoordinates.Count),
				MaxSpeed = (float) Locator.MaxSpeed,
				TotalDistanceMeters = (long) distanceInMeters,
				MostCommonActivity = DependencyService.Get<IMotionActivityManager>().GetMostCommonActivity().ToString(),
				Points = CustomMap.RouteCoordinates,
				PointsJSON = JsonConvert.SerializeObject(CustomMap.RouteCoordinates)
			};
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