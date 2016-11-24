using System;
using Xamarin.Forms;

namespace Trace {

	public partial class CheckpointDetailsPage : ContentPage {

		public CheckpointDetailsPage(Checkpoint checkpoint) {
			InitializeComponent();
			BindingContext = checkpoint;
		}

		async void OnAddressTapped(object sender, EventArgs e) {
			// todo show location on map
		}

		async void OnWebsiteAddressTapped(object sender, EventArgs e) {
			// todo 
		}
	}
}
