using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Trace.Localization;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {
	/// <summary>
	/// Page displaying the map showing the user location and the nearby challenges.
	/// Allows the user to track their location, showing the trajectory and related statistics when finished tracking.
	/// </summary>
	public partial class MapPage : ContentPage {

		//private CurrentActivity currentActivity;
		// This prevents the user from issuing several tracking requests before the previous has completed.
		private static bool isProcessing = false;

		private static DateTime StartTrackingTime;
		private static DateTime StopTrackingTime;
		private const long MIN_SIZE_TRAJECTORY = 250; // meters

		// Used when user clicks on map from checkpoint details page.
		public static bool ShouldCenterOnUser = true;

		private static List<Plugin.Geolocator.Abstractions.Position> routeCoordinates;

		public MapPage() {
			InitializeComponent();
			Debug.WriteLine("MapPage.Initialize()");
			if(Device.OS == TargetPlatform.iOS) { Icon = "map__maps_icon.png"; }

			initializeCheckpointPins();

			// When the user switches pages, all non-static objects are GC'ed, including the map.
			// To keep the coordinates from the previous TraceMap, we use a static reference to the list.
			if(Geolocator.IsTrackingInProgress) {
				map.RouteCoordinates = routeCoordinates;
			}
			else {
				Geolocator.Map = map;
				routeCoordinates = map.RouteCoordinates;
			}
			//Locator.Start().DoNotAwait();

			//currentActivity = new CurrentActivity();
			//currentActivityLabel.BindingContext = currentActivity;

			if(!DependencyService.Get<IMotionActivityManager>().IsInitialized) {
				// Initialize handler function to receive input from the motion activity manager & send it to state machine. 
				DependencyService.Get<IMotionActivityManager>().InitMotionActivity();
				DependencyService.Get<IMotionActivityManager>().StartMotionUpdates((activity) => {
					if(activity != ActivityType.Unknown) {
						//currentActivity.ActivityType = activity;
						Debug.WriteLine(activity + ": " + DateTime.Now);
						App.DEBUG_ActivityLog += DateTime.Now + ": " + activity + "\n";
						RewardEligibilityManager.Instance.Input(activity);
					}
				});
			}

			if(Geolocator.IsTrackingInProgress) {
				trackButton.Image = "map__stop.png";
			}

			//var locateButtonGR = new TapGestureRecognizer();
			//locateButtonGR.Tapped += (sender, e) => onLocateUser(sender, e);
			//locateImage.GestureRecognizers.Add(locateButtonGR);

			// Add tap event handler to results grid so it can be dismissed by the user.
			var gridGR = new TapGestureRecognizer();
			gridGR.Tapped += onClearResults;
			resultsGrid.GestureRecognizers.Add(gridGR);
		}

		void onClearResults(object sender, EventArgs args) {
			resultsGrid.IsVisible = false;
		}

		// Center map on user position when page displays.
		protected override void OnAppearing() {
			Debug.WriteLine("MapPage.OnAppearing()");
			base.OnAppearing();
		}

		protected override void OnDisappearing() {
			base.OnDisappearing();

			((TapGestureRecognizer) resultsGrid.GestureRecognizers.FirstOrDefault()).Tapped -= onClearResults;
		}


		async void onLocateUser(object send, EventArgs eventArgs) {
			// Check for permissions before using the GPS.
			if(!Geolocator.IsEnabled()) {
				var wantToChangeSetting = await DisplayAlert(Language.Notice, Language.GPSDisabledErrorMsg, Language.Yes, Language.No);

				if(wantToChangeSetting)
					CrossPermissions.Current.OpenAppSettings();

				if(!Geolocator.IsEnabled())
					return;
			}

			// Let user know the operation is underway (it can take a few seconds to get the location).
			var toastCfg = new ToastConfig(Language.FetchUserLocation) { Duration = new TimeSpan(0, 0, 5) };
			UserDialogs.Instance.Toast(toastCfg);

			locateButton.IsVisible = false;

			var pos = await GeoUtils.GetCurrentUserLocation(timeout: 5000);
			Geolocator.UpdateMap(new Position(latitude: pos.Latitude, longitude: pos.Longitude));

			locateButton.IsVisible = true;
		}


		/// <summary>
		/// Called when user pressed the Track button.
		/// When 'start' is pressed, turn on GPS tracking and start storing user points.
		/// When 'stop' is pressed, process the trajectory (calculate distance, calories, etc.) and show trace on map.
		/// </summary>
		async void onStartTracking(object send, EventArgs eventArgs) {

			// Check for permissions first before trying to use the GPS.
			if(!Geolocator.IsEnabled()) {
				var wantToChangeSetting = await DisplayAlert(Language.Notice, Language.GPSDisabledErrorMsg, Language.Yes, Language.No);

				if(wantToChangeSetting)
					CrossPermissions.Current.OpenAppSettings();

				if(!Geolocator.IsEnabled())
					return;
			}

			// On Stop Button pressed
			if(Geolocator.IsTrackingInProgress && !isProcessing) {
				//var toastCfg = new ToastConfig(Language.ProcessingTrajectory) {
				//	Duration = new TimeSpan(0, 0, 3)
				//};
				//UserDialogs.Instance.Toast(toastCfg);
				trackButton.IsVisible = false;

				isProcessing = true;
				// Calculate stats and show the results to the user.
				StopTrackingTime = DateTime.Now; Debug.WriteLine($"StopTrackingTime: {StopTrackingTime}");

				// Put all CPU-bound operations outside of UI thread.
				var calculateMotionTask = DependencyService.Get<IMotionActivityManager>()
											.QueryHistoricalData(
												StartTrackingTime, StopTrackingTime
											  );

				var calculateDistanceTask = calculateRouteDistance();

				await Task.WhenAll(
					calculateMotionTask,
					calculateDistanceTask
				);
				double distanceInMeters = calculateDistanceTask.Result;

				// Create trajectory.
				Trajectory trajectory = createTrajectory(distanceInMeters);

				// Update KPI with new activity information.
				User.Instance.GetCurrentKPI().AddActivityEvent(trajectory.StartTime,
															   trajectory.TimeSpentWalking,
															   trajectory.TimeSpentRunning,
															   trajectory.TimeSpentCycling,
															   trajectory.TimeSpentDriving);

				// Calculate Motion activities along the trajectory.
				IList<ActivityEvent> activityEvents = DependencyService.Get<IMotionActivityManager>().ActivityEvents;
				trajectory.Points = await Task.Run(() => AssociatePointsWithActivity(activityEvents, map.RouteCoordinates));
				//Debug.WriteLine("6");
				// Save trajectory only if it is of relevant size.
				if(distanceInMeters > MIN_SIZE_TRAJECTORY) {
					trajectory.PointsJSON = JsonConvert.SerializeObject(trajectory.Points);

					// Save created trajectory.
					User.Instance.Trajectories.Add(trajectory);
					SQLiteDB.Instance.SaveItem(trajectory);

					// Try to send trajectory to the Web Server right away.
					Task.Run(() => new WebServerClient().SendTrajectory(trajectory)).DoNotAwait();
				}
				else {
					var toastCfg = new ToastConfig(Language.TrajectoryTooSmall) {
						Duration = new TimeSpan(0, 0, 5)
					};
					UserDialogs.Instance.Toast(toastCfg);
				}

				// Reset GPS recorded speeds.
				Geolocator.AvgSpeed = 0;
				Geolocator.MaxSpeed = 0;

				var mainActivity = DependencyService.Get<IMotionActivityManager>().GetMostCommonActivity().ToLocalizedString();
				var calories = await Task.Run(() => trajectory.CalculateCalories());

				var displayResultsModel = new MapPageModel {
					MainActivity = mainActivity,
					Distance = (int) distanceInMeters,
					Duration = calculateRouteTime(),
					Calories = calories,
					AvgSpeed = trajectory.AvgSpeed
				};

				trackButton.Image = "map__play_arrow.png";
				trackButton.IsVisible = true;

				displayResultsGrid(displayResultsModel);

				await Geolocator.Stop();
			}

			// On Start Button pressed
			else if(!isProcessing) {
				isProcessing = true;
				trackButton.IsVisible = false;

				var toastCfg = new ToastConfig(Language.StartTracking) {
					Duration = new TimeSpan(0, 0, 2)
				};
				UserDialogs.Instance.Toast(toastCfg);

				map.RouteCoordinates.Clear();
				StartTrackingTime = DateTime.Now;

				await Geolocator.Start();
				// Show Activity text again and remove Results grid.
				//currentActivityLabel.IsVisible = true;
				resultsGrid.IsVisible = false;

				// Reset in order to clean list of accumulated activities and counters.
				DependencyService.Get<IMotionActivityManager>().Reset();

				trackButton.Image = "map__stop.png";
				trackButton.IsVisible = true;
			}

			Geolocator.IsTrackingInProgress = !Geolocator.IsTrackingInProgress;
			isProcessing = false;

		}


		private void displayResultsGrid(MapPageModel bindingModel) {
			//currentActivityLabel.IsVisible = false;

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


		private async Task<double> calculateRouteDistance() {
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
				AvgSpeed = (float) (Geolocator.AvgSpeed / map.RouteCoordinates.Count),
				MaxSpeed = (float) Geolocator.MaxSpeed,
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
		/// For each activity event, match all the points that start and end during that period.
		/// Running time is O(n+m), where 'n' is the size of the trajectory and 'm' is the size of activity event list.
		/// </summary>
		/// <param name="points">Points in the trajectory.</param>
		public List<TrajectoryPoint> AssociatePointsWithActivity(IList<ActivityEvent> activityEvents, IEnumerable<Plugin.Geolocator.Abstractions.Position> points) {
			var res = new List<TrajectoryPoint>();

			// Check for the case where there are no activities registered.
			if(activityEvents.Count == 0) {
				return fillTailWithUnknownPoints(points.GetEnumerator(), res);
			}

			var i = 0;
			var ptr = points.GetEnumerator();
			Debug.WriteLine($"activities: {activityEvents.Count} points: {points.Count()}");
			foreach(ActivityEvent a in activityEvents) {
				Debug.WriteLine($"{i++} a: {a.EndDate}");

				while(ptr.MoveNext()) {
					Debug.WriteLine($"p: {ptr.Current.Timestamp}");

					var p = ptr.Current;
					if(p.Timestamp.UtcDateTime < a.EndDate) {
						res.Add(createPoint(p, a));
					}
				}
			}

			if(ptr.MoveNext()) {
				return fillTailWithUnknownPoints(ptr, res);
			}

			return res;
		}

		private TrajectoryPoint createPoint(Plugin.Geolocator.Abstractions.Position p, ActivityEvent activityEvent) {
			return new TrajectoryPoint() {
				Longitude = p.Longitude,
				Latitude = p.Latitude,
				Timestamp = p.Timestamp.DatetimeToEpochSeconds(),
				Activity = activityEvent.ActivityType
			};
		}

		private List<TrajectoryPoint> fillTailWithUnknownPoints(
			IEnumerator<Plugin.Geolocator.Abstractions.Position> pointsPtr,
			List<TrajectoryPoint> res
		) {
			var activityEvent = new ActivityEvent(ActivityType.Unknown);
			try {
				res.Add(createPoint(pointsPtr.Current, activityEvent));
			}
			catch(Exception) { return res; }

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