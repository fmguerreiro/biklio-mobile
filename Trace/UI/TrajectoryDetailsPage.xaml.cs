using System;
using Xamarin.Forms;

namespace Trace {

	public partial class TrajectoryDetailsPage : ContentPage {
		private Trajectory trajectory;

		public TrajectoryDetailsPage(Trajectory trajectory) {
			InitializeComponent();
			BindingContext = this.trajectory = trajectory;
			UploadTrajectoryButton.IsVisible |= !trajectory.WasTrackSent;
			// Redraw map with drawn trajectory.
			CustomMap.RouteCoordinates = trajectory.ToPosition();
			Stack.Children.RemoveAt(1);
			Stack.Children.Insert(1, CustomMap);
		}

		async void OnUploadClicked(object sender, EventArgs args) {
			await new WebServerClient().SendTrajectory(trajectory);
			if(trajectory.WasTrackSent) {
				UploadTrajectoryButton.IsVisible = false;
			}
			else {
				await DisplayAlert("Error", "Trajectory could not be uploaded to the server.", "Ok");
			}
		}
	}
}
