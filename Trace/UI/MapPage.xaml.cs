using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {
	public partial class MapPage : ContentPage {

		private const long MIN_SIZE_TRAJECTORY = 250;
		private Geolocator Locator;
		private CurrentActivity currentActivity;

		private DateTime StartTrackingTime;
		private DateTime StopTrackingTime;

		private string activityLogResult = "";


		public MapPage() {
			InitializeComponent();
			Locator = new Geolocator(CustomMap);
			//Task.Run(() => Locator.Start());
			initializeChallengePins();
			Locator.Start().DoNotAwait();
			currentActivity = new CurrentActivity();
			DependencyService.Get<IMotionActivityManager>().InitMotionActivity();
		}


		// TODO offload cpu-intensive work off the UI thread
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

				// Create trajectory.
				double distanceInMeters = calculateDistanceTask.Result;
				Trajectory trajectory = await Task.Run(() => createTrajectory(distanceInMeters));

				// Save trajectory only if it is of relevant size.
				if(trajectory.TotalDistanceMeters > MIN_SIZE_TRAJECTORY) {
					// TODO offload this out of UI thread
					// Calculate Motion activities along the trajectory.
					IList<ActivityEvent> activityEvents = DependencyService.Get<IMotionActivityManager>().ActivityEvents;
					trajectory.Points = AssociatePointsWithActivity(activityEvents, CustomMap.RouteCoordinates);
					trajectory.PointsJSON = JsonConvert.SerializeObject(trajectory.Points);

					// Save created trajectory.
					User.Instance.Trajectories.Add(trajectory);
					SQLiteDB.Instance.SaveItem<Trajectory>(trajectory);

					// Send trajectory to the Web Server.
					Task.Run(() => new WebServerClient().SendTrajectory(trajectory)).DoNotAwait();
				}

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


		private void displayGrid(Button trackButton) {
			trackButton.Text = "Track";
			ActivityLabel.IsVisible = false;
			Debug.WriteLine("Trajectory # of points: " + CustomMap.RouteCoordinates.Count);
			// Refresh the map to display the trajectory.
			MyStack.Children.RemoveAt(0);
			MyStack.Children.Insert(0, CustomMap);

			// Make the results grid visible to the user.
			ResultsGrid.IsVisible = true;
		}


		private double calculateRouteDistance() {
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


		private TotalDuration calculateRouteTime() {
			TimeSpan elapsedTime = StopTrackingTime.Subtract(StartTrackingTime);
			return new TotalDuration {
				Hours = elapsedTime.Hours,
				Minutes = elapsedTime.Minutes,
				Seconds = elapsedTime.Seconds
			};
		}


		private Trajectory createTrajectory(double distanceInMeters) {
			return new Trajectory {
				UserId = User.Instance.Id,
				StartTime = StartTrackingTime.DatetimeToEpochSeconds(),
				EndTime = StopTrackingTime.DatetimeToEpochSeconds(),
				AvgSpeed = (float) (Locator.AvgSpeed / CustomMap.RouteCoordinates.Count),
				MaxSpeed = (float) Locator.MaxSpeed,
				TotalDistanceMeters = (long) distanceInMeters,
				MostCommonActivity = DependencyService.Get<IMotionActivityManager>().GetMostCommonActivity().ToString(),
				//Points = CustomMap.RouteCoordinates
				//PointsJSON = JsonConvert.SerializeObject(CustomMap.RouteCoordinates)
			};
		}


		/// <summary>
		/// For each point in the trajectory, the corresponding activity type is matched.
		/// Matching is done by finding the activity period on which it fits.
		/// Running time is O(n+m), where 'n' is the size of the trajectory and 'm' is the size of activity event list.
		/// </summary>
		/// <param name="points">Points in the trajectory.</param>
		public List<TrajectoryPoint> AssociatePointsWithActivity(IList<ActivityEvent> activityEvents, IEnumerable<Plugin.Geolocator.Abstractions.Position> points) {
			var res = new List<TrajectoryPoint>();
			IEnumerator activityEventPtr = activityEvents.GetEnumerator();

			// Check for the case where there are no activities registered.
			if(!activityEventPtr.MoveNext()) {
				return fillTailWithUnknownPoints(points.GetEnumerator(), res);
			}

			foreach(Plugin.Geolocator.Abstractions.Position p in points) {
				// Points are associated with the activity event period in which they occur.
				var activityEvent = (ActivityEvent) activityEventPtr.Current;

				if(p.Timestamp.UtcDateTime < activityEvent.EndDate) {
					res.Add(createPoint(p, activityEvent));
				}
				// If the point does not belong to the activity period, search the next activity events until the period is found.
				else {
					while(p.Timestamp.UtcDateTime >= ((ActivityEvent) activityEventPtr.Current).EndDate)
						activityEventPtr.MoveNext();
					try {
						activityEvent = (ActivityEvent) activityEventPtr.Current;
					}
					catch(Exception) { return fillTailWithUnknownPoints(points.GetEnumerator(), res); }
					res.Add(createPoint(p, activityEvent));
				}

				if(activityEventPtr.MoveNext()) {
					return fillTailWithUnknownPoints(points.GetEnumerator(), res);
				};
			}
			return res;
		}

		private TrajectoryPoint createPoint(Plugin.Geolocator.Abstractions.Position p, ActivityEvent activityEvent) {
			return new TrajectoryPoint() {
				Longitude = p.Longitude,
				Latitude = p.Latitude,
				Timestamp = TimeConverter.DatetimeToEpochSeconds(p.Timestamp),
				Activity = activityEvent.ActivityType
			};
		}

		private List<TrajectoryPoint> fillTailWithUnknownPoints(IEnumerator<Plugin.Geolocator.Abstractions.Position> pointsPtr, List<TrajectoryPoint> res) {
			var activityEvent = new ActivityEvent(ActivityType.Unknown);
			while(pointsPtr.MoveNext()) {
				var p = pointsPtr.Current;
				res.Add(createPoint(p, activityEvent));
			}
			return res;
		}

		/// <summary>
		/// Calculate the distance between points using Equirectangular approximation.
		/// This does not take into account the arc of the Earth, but is much quicker and acceptable for our purposes.
		/// </summary>
		/// <returns>The distance in meters.</returns>
		/// <param name="pos1">Position 1.</param>
		/// <param name="pos2">Position 2.</param>
		private double distanceBetweenPoints(Plugin.Geolocator.Abstractions.Position pos1, Plugin.Geolocator.Abstractions.Position pos2) {
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


		private void initializeChallengePins() {
			var pins = new List<CustomPin>();
			foreach(Challenge c in User.Instance.Challenges) {
				if(c.ThisCheckpoint != null) {
					var pin = new CustomPin {
						Pin = new Pin {
							Type = PinType.Place,
							Position = new Position(c.ThisCheckpoint.Latitude, c.ThisCheckpoint.Longitude),
							Label = c.Description,
							Address = c.Condition
						},
						Id = "",
						Checkpoint = c.ThisCheckpoint,
						ImageURL = c.ThisCheckpoint.LogoURL
					};
					pins.Add(pin);
					CustomMap.Pins.Add(pin.Pin);
				}
			}
			CustomMap.CustomPins = pins;
		}


		public void UpdatePins() {
			CustomMap.Pins.Clear();
			initializeChallengePins();
			// Refresh the map to force renderer to run OnElementChanged() and display the new pins.
			MyStack.Children.RemoveAt(0);
			MyStack.Children.Insert(0, CustomMap);
		}
	}
}