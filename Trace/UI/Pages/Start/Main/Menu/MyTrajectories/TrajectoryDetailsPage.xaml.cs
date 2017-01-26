using System;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// Page that shows the details for a particular trajectory.
	/// This includes displaying the trajectory on the map, showing the dates, length and duration.
	/// </summary>
	public partial class TrajectoryDetailsPage : ContentPage {
		private TrajectoryViewModel trajectoryVM;

		public TrajectoryDetailsPage(TrajectoryViewModel trajectoryVM) {
			InitializeComponent();
			BindingContext = this.trajectoryVM = trajectoryVM;
			uploadTrajectoryButton.IsVisible |= !trajectoryVM.Trajectory.WasTrackSent;
			// Redraw map with trajectory.
			CustomMap.RouteCoordinates = trajectoryVM.Trajectory.ToPosition();
			Stack.Children.RemoveAt(1);
			Stack.Children.Insert(1, CustomMap);
		}

		async void onUploadClicked(object sender, EventArgs args) {
			uploadTrajectoryButton.IsVisible = false;
			await new WebServerClient().SendTrajectory(trajectoryVM.Trajectory);
			if(trajectoryVM.Trajectory.WasTrackSent) {
				uploadTrajectoryButton.IsVisible = false;
				await DisplayAlert(Language.Result, Language.OperationCompleted, Language.Ok);
				trajectoryVM.TriggerPropertyChangedEvent(); // change image in mytrajectorylist page.
			}
			else {
				await DisplayAlert(Language.Error, Language.TrajectoryNotUploadedError, Language.Ok);
				uploadTrajectoryButton.IsVisible = true;
			}
		}
	}
}
