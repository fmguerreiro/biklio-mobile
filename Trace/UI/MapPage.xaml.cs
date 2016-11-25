using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Plugin.Geolocator;
using System.Threading.Tasks;

namespace Trace {
	public partial class MapPage : ContentPage {

		private Geolocator Locator;
		private CurrentActivity currentActivity;

		private DateTime StartTrackingTime;
		private DateTime StopTrackingTime;

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
				TotalDistanceLabel.BindingContext = new TotalDistance { Distance = 0 };

				TimeSpan elapsedTime = StopTrackingTime.Subtract(StartTrackingTime);
				DurationLabel.BindingContext = new TotalDuration {
					Hours = elapsedTime.Hours,
					Minutes = elapsedTime.Minutes,
					Seconds = elapsedTime.Seconds
				};
				CaloriesLabel.BindingContext = new TotalCalories { Calories = 0 };
				DrivenLabel.BindingContext = new TotalDuration { Hours = 0, Minutes = 0, Seconds = 0 };
				//MainActivityLabel.BindingContext = currentActivity;
			}
			// On Track Button pressed
			else {
				((Button) send).Text = "Stop";
				CustomMap.RouteCoordinates.Clear();
				StartTrackingTime = DateTime.Now;

				// Show Activity text after Map and before Stop button and remove Results grid.
				ActivityLabel.IsVisible = true;
				ResultsGrid.IsVisible = false;
				ActivityLabel.BindingContext = currentActivity;
				DependencyService.Get<MotionActivityInterface>().StartMotionUpdates((activity) => {
					currentActivity.ActivityType = activity;
				});
			}
			Locator.IsTrackingInProgress = !Locator.IsTrackingInProgress;
		}
	}
}