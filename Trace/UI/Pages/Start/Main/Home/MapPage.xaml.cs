using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Trace.Localization;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {
	/// <summary>
	/// Page displaying the map showing the user location and the nearby challenges.
	/// Allows the user to track their location, showing the trajectory and related statistics when finished tracking.
	/// </summary>
	public partial class MapPage : ContentPage {

		private const long MIN_SIZE_TRAJECTORY = 250; // meters
		private Geolocator Locator;
		private CurrentActivity currentActivity;

		private DateTime StartTrackingTime;
		private DateTime StopTrackingTime;

		public MapPage() {
			InitializeComponent();
			Locator = new Geolocator(CustomMap);
			//Task.Run(() => Locator.Start());
			initializeChallengePins();
			Locator.Start().DoNotAwait();
			currentActivity = new CurrentActivity();
			DependencyService.Get<IMotionActivityManager>().InitMotionActivity();
			DependencyService.Get<IMotionActivityManager>().StartMotionUpdates((activity) => {
				currentActivity.ActivityType = activity;
				//activityLogResult += DateTime.Now + ": " + activity + "\n";
				// Send input to state machine 
				// TODO use activity confidence as well
				RewardEligibilityManager.Instance.Input(activity);
			});
			// create start tracking button
			//var normalFab = new FloatingActionButton();
			//normalFab.Source = "default_shop.png";
			//normalFab.Size = FabSize.Normal;
			//MyStack.Children.Add(
			//	normalFab
			////xConstraint: Constraint.RelativeToParent((parent) => { return (parent.Width - normalFab.Width) - 16; }),
			////yConstraint: Constraint.RelativeToParent((parent) => { return (parent.Height - normalFab.Height) - 16; })
			//);

			//normalFab.SizeChanged += (sender, args) => { MyStack.ForceLayout(); };
		}


		// TODO offload cpu-intensive work off the UI thread
		async void OnStartTracking(object send, EventArgs eventArgs) {

			// On Stop Button pressed
			if(Geolocator.IsTrackingInProgress) {
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
					trajectory.Points = await Task.Run(() => AssociatePointsWithActivity(activityEvents, CustomMap.RouteCoordinates));
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

				TotalDistanceLabel.BindingContext = new TotalDistance { Distance = (long) distanceInMeters };
				TotalDuration duration = calculateRouteTime();
				DurationLabel.BindingContext = duration;

				var calories = await Task.Run(() => trajectory.CalculateCalories());
				CaloriesLabel.BindingContext = new TotalCalories { Calories = calories };

				AvgSpeedLabel.BindingContext = trajectory.AvgSpeed;

				displayGrid((Button) send);
			}

			// On Track Button pressed
			else {
				((Button) send).Text = Language.Stop;
				CustomMap.RouteCoordinates.Clear();
				StartTrackingTime = DateTime.Now;
				// Show Activity text after Map and before Stop button and remove Results grid.
				ActivityLabel.IsVisible = true;
				ResultsGrid.IsVisible = false;
				ActivityLabel.BindingContext = currentActivity;
				// Reset in order to clean list of accumulated activities and counters.
				DependencyService.Get<IMotionActivityManager>().Reset();
			}
			Geolocator.IsTrackingInProgress = !Geolocator.IsTrackingInProgress;
		}


		private void displayGrid(Button trackButton) {
			trackButton.Text = Language.Track;
			ActivityLabel.IsVisible = false;
			MainActivityLabel.Text = DependencyService.Get<IMotionActivityManager>().GetMostCommonActivity().ToString();
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
				distanceInMeters += GeoUtils.DistanceBetweenPoints(pos1, pos2);
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
				TimeSpentWalking = DependencyService.Get<IMotionActivityManager>().WalkingDuration,
				TimeSpentRunning = DependencyService.Get<IMotionActivityManager>().RunningDuration,
				TimeSpentCycling = DependencyService.Get<IMotionActivityManager>().CyclingDuration,
				TimeSpentDriving = DependencyService.Get<IMotionActivityManager>().DrivingDuration
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


		private void initializeChallengePins() {
			var pins = new List<CustomPin>();
			foreach(Challenge c in User.Instance.Challenges) {
				if(c.ThisCheckpoint != null) {
					var pin = new CustomPin {
						Pin = new Pin {
							Type = PinType.Place,
							Position = new Position(c.ThisCheckpoint.Latitude, c.ThisCheckpoint.Longitude),
							Label = c.Description,
							Address = c.NeededCyclingDistance.ToString()
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


		/// <summary>
		/// Updates the pins on the map.
		/// Occurs when the challenge list is updated.
		/// </summary>
		public void UpdatePins() {
			CustomMap.Pins.Clear();
			initializeChallengePins();
			// Refresh the map to force renderer to run OnElementChanged() and display the new pins.
			MyStack.Children.RemoveAt(0);
			MyStack.Children.Insert(0, CustomMap);
		}
	}
}