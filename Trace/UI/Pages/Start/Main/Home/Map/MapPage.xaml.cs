using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {
	/// <summary>
	/// Page displaying the map showing the user location and the nearby challenges.
	/// Allows the user to track their location, showing the trajectory and related statistics when finished tracking.
	/// </summary>
	public partial class MapPage : ContentPage {

		private Geolocator Locator;
		private CurrentActivity currentActivity;
		// This prevents the user from issuing several tracking requests before the previous has completed.
		private bool isProcessing = false;

		private DateTime StartTrackingTime;
		private DateTime StopTrackingTime;
		private const long MIN_SIZE_TRAJECTORY = 250; // meters

		public MapPage() {
			InitializeComponent();
			Debug.WriteLine("MapPage.Initialize()");
			if(Device.OS == TargetPlatform.iOS) { Icon = "images/map/maps_icon.png"; }

			initializeCheckpointPins();

			Locator = new Geolocator(map);
			//Locator.Start().DoNotAwait();

			currentActivity = new CurrentActivity();
			currentActivityLabel.BindingContext = currentActivity;

			// Send input to state machine 
			DependencyService.Get<IMotionActivityManager>().InitMotionActivity();
			DependencyService.Get<IMotionActivityManager>().StartMotionUpdates((activity) => {
				if(activity != ActivityType.Unknown) {
					currentActivity.ActivityType = activity;
					App.DEBUG_ActivityLog += DateTime.Now + ": " + activity + "\n";
					// TODO use activity confidence as well
					RewardEligibilityManager.Instance.Input(activity);
				}
			});

			// Add tap event handlers to the images on top of the circle buttons (otherwise, if users click on the img nothing happens)
			var trackButtonGR = new TapGestureRecognizer();
			trackButtonGR.Tapped += (sender, e) => OnStartTracking(null, null);
			trackButtonImage.GestureRecognizers.Add(trackButtonGR);

			var locateButtonGR = new TapGestureRecognizer();
			locateButtonGR.Tapped += (sender, e) => OnLocateUser(null, null);
			locateImage.GestureRecognizers.Add(locateButtonGR);
		}


		// Center map on user position when page displays.
		protected override async void OnAppearing() {
			base.OnAppearing();
			Debug.WriteLine("MapPage.OnAppearing()");
			Geolocator.TryLowerAccuracy();
			var userLocation = await GeoUtils.GetCurrentUserLocation();
			Locator.UpdateMap(userLocation);
		}


		async void OnLocateUser(object send, EventArgs eventArgs) {
			var pos = await GeoUtils.GetCurrentUserLocation();
			Geolocator.UpdateMap(new Position(latitude: pos.Latitude, longitude: pos.Longitude));
		}


		/// <summary>
		/// Called when user pressed the Track button.
		/// When 'start' is pressed, turn on GPS tracking and start storing user points.
		/// When 'stop' is pressed, process the trajectory (calculate distance, calories, etc.) and show trace on map.
		/// </summary>
		async void OnStartTracking(object send, EventArgs eventArgs) {

			// On Stop Button pressed
			if(Geolocator.IsTrackingInProgress && !isProcessing) {
				isProcessing = true;
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

				// Update KPI with new activity information.
				User.Instance.GetCurrentKPI().AddActivityEvent(trajectory.StartTime,
															   trajectory.TimeSpentWalking,
															   trajectory.TimeSpentRunning,
															   trajectory.TimeSpentCycling,
															   trajectory.TimeSpentDriving);

				// Save trajectory only if it is of relevant size.
				if(distanceInMeters > MIN_SIZE_TRAJECTORY) {
					// Calculate Motion activities along the trajectory.
					IList<ActivityEvent> activityEvents = DependencyService.Get<IMotionActivityManager>().ActivityEvents;
					trajectory.Points = await Task.Run(() => AssociatePointsWithActivity(activityEvents, map.RouteCoordinates));
					trajectory.PointsJSON = JsonConvert.SerializeObject(trajectory.Points);

					// Save created trajectory.
					User.Instance.Trajectories.Add(trajectory);
					SQLiteDB.Instance.SaveItem(trajectory);

					// Try to send trajectory to the Web Server right away.
					Task.Run(() => new WebServerClient().SendTrajectory(trajectory)).DoNotAwait();
				}

				// Reset GPS recorded speeds.
				Locator.AvgSpeed = 0;
				Locator.MaxSpeed = 0;

				var mainActivity = DependencyService.Get<IMotionActivityManager>().GetMostCommonActivity().ToLocalizedString();
				var calories = await Task.Run(() => trajectory.CalculateCalories());

				var displayResultsModel = new MapPageModel {
					MainActivity = mainActivity,
					Distance = (int) distanceInMeters,
					Duration = calculateRouteTime(),
					Calories = calories,
					AvgSpeed = trajectory.AvgSpeed
				};

				trackButtonImage.Source = "images/map/play_arrow.png";

				displayResultsGrid(displayResultsModel);

				await Geolocator.Stop();
			}

			// On Start Button pressed
			else if(!isProcessing) {
				isProcessing = true;

				// TODO show toast here.

				await Locator.Start();

				trackButtonImage.Source = "images/map/stop.png";
				map.RouteCoordinates.Clear();
				StartTrackingTime = DateTime.Now;

				// Show Activity text again and remove Results grid.
				currentActivityLabel.IsVisible = true;
				resultsGrid.IsVisible = false;

				// Reset in order to clean list of accumulated activities and counters.
				DependencyService.Get<IMotionActivityManager>().Reset();
			}

			Geolocator.IsTrackingInProgress = !Geolocator.IsTrackingInProgress;
			isProcessing = false;
		}


		private void displayResultsGrid(MapPageModel bindingModel) {
			currentActivityLabel.IsVisible = false;

			Debug.WriteLine("Trajectory # of points: " + map.RouteCoordinates.Count);
			// Refresh the map to display the trajectory.
			mapLayout.Children.Remove(map);
			mapLayout.Children.Insert(0, map);

			// Make the results grid visible to the user.
			foreach(var child in resultsGrid.Children) {
				child.BindingContext = bindingModel;
			}
			resultsGrid.IsVisible = true;
		}


		private double calculateRouteDistance() {
			double distanceInMeters = 0;
			var coordinates = map.RouteCoordinates.GetEnumerator();
			coordinates.MoveNext();
			var pos1 = coordinates.Current;
			while(coordinates.MoveNext()) {
				var pos2 = coordinates.Current;
				distanceInMeters += GeoUtils.DistanceBetweenPoints(pos1, pos2);
				pos1 = pos2;
			}
			return distanceInMeters;
		}


		private string calculateRouteTime() {
			TimeSpan elapsedTime = StopTrackingTime.Subtract(StartTrackingTime);
			return TimeUtil.SecondsToHHMMSS((long) elapsedTime.TotalSeconds);
		}


		private Trajectory createTrajectory(double distanceInMeters) {
			return new Trajectory {
				UserId = User.Instance.Id,
				StartTime = StartTrackingTime.DatetimeToEpochSeconds(),
				EndTime = StopTrackingTime.DatetimeToEpochSeconds(),
				AvgSpeed = (float) (Locator.AvgSpeed / map.RouteCoordinates.Count),
				MaxSpeed = (float) Locator.MaxSpeed,
				TotalDistanceMeters = (long) distanceInMeters,
				MostCommonActivity = DependencyService.Get<IMotionActivityManager>().GetMostCommonActivity().ToString(),
				TimeSpentWalking = DependencyService.Get<IMotionActivityManager>().WalkingDuration,
				TimeSpentRunning = DependencyService.Get<IMotionActivityManager>().RunningDuration,
				TimeSpentCycling = DependencyService.Get<IMotionActivityManager>().CyclingDuration,
				TimeSpentDriving = DependencyService.Get<IMotionActivityManager>().DrivingDuration
				//Points = map.RouteCoordinates
				//PointsJSON = JsonConvert.SerializeObject(map.RouteCoordinates)
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
				}
			}
			return res;
		}

		private TrajectoryPoint createPoint(Plugin.Geolocator.Abstractions.Position p, ActivityEvent activityEvent) {
			return new TrajectoryPoint() {
				Longitude = p.Longitude,
				Latitude = p.Latitude,
				Timestamp = TimeUtil.DatetimeToEpochSeconds(p.Timestamp),
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


		private void initializeCheckpointPins() {
			var pins = new List<CustomPin>();
			foreach(Checkpoint c in User.Instance.Checkpoints.Values) {
				var firstChallenge = c.Challenges.FirstOrDefault();
				var pin = new CustomPin {
					Pin = new Pin {
						Type = PinType.Place,
						Position = new Position(c.Latitude, c.Longitude),
						Label = c.Name,
						Address = firstChallenge?.Reward
					},
					Id = "",
					Checkpoint = c,
					ImageURL = c.LogoURL
				};
				pins.Add(pin);
				map.Pins.Add(pin.Pin);

			}
			map.CustomPins = pins;
		}


		/// <summary>
		/// Updates the pins on the map.
		/// Called when the checkpoint list in the CheckpointListPage is updated.
		/// </summary>
		public void UpdatePins() {
			map.Pins.Clear();
			initializeCheckpointPins();
			// Refresh the map to force renderer to run OnElementChanged() and display the new pins.
			mapLayout.Children.Remove(map);
			mapLayout.Children.Insert(0, map);
		}
	}
}