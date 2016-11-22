using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Plugin.Geolocator;
using System.Threading.Tasks;

namespace Trace {
	public partial class MapPage : ContentPage {

		private Geolocator Locator;

		public MapPage() {
			InitializeComponent();
			Locator = new Geolocator(CustomMap);
			Locator.Start();
			DependencyService.Get<MotionActivityInterface>().InitMotionActivity();
		}



		void OnStartTracking(object send, EventArgs eventArgs) {
			// On Stop Button pressed
			if(Locator.IsTrackingInProgress) {
				//DependencyService.Get<MotionActivityInterface>().StopMotionUpdates();
				((Button) send).Text = "Track";
				// todo calculate stats and show new page to user!
				//StopTrackingTime = DateTime.Now;
				//DependencyService.Get<MotionActivityInterface>().QueryHistoricalData(StartTrackingTime, StopTrackingTime);
				Navigation.PushAsync(new ShowTrackedResultsPage(CustomMap));
			}
			// On Track Button pressed
			else {
				((Button) send).Text = "Stop";
				CustomMap.RouteCoordinates.Clear();
				//StartTrackingTime = DateTime.Now;
				//DependencyService.Get<MotionActivityInterface>().StartMotionUpdates();
			}
			Locator.IsTrackingInProgress = !Locator.IsTrackingInProgress;
		}


	}
}