using System;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// Page that shows the details for a particular trajectory.
	/// This includes displaying the trajectory on the map, showing the dates, length and duration.
	/// </summary>
	public partial class TrajectoryDetailsPage : ContentPage {
		private Trajectory trajectory;

		public TrajectoryDetailsPage(Trajectory trajectory) {
			InitializeComponent();
			BindingContext = this.trajectory = trajectory;
			UploadTrajectoryButton.IsVisible |= !trajectory.WasTrackSent;
			// Redraw map with trajectory.
			CustomMap.RouteCoordinates = trajectory.ToPosition();
			Stack.Children.RemoveAt(1);
			Stack.Children.Insert(1, CustomMap);
		}

		async void onUploadClicked(object sender, EventArgs args) {
			UploadTrajectoryButton.IsVisible = false;
			await new WebServerClient().SendTrajectory(trajectory);
			if(trajectory.WasTrackSent) {
				UploadTrajectoryButton.IsVisible = false;
			}
			else {
				await DisplayAlert(Language.Error, Language.TrajectoryNotUploadedError, Language.Ok);
				UploadTrajectoryButton.IsVisible = true;
			}
		}
	}
}
