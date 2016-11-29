using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Trace {

	public partial class CheckpointDetailsPage : ContentPage {

		public CheckpointDetailsPage(Checkpoint checkpoint) {
			InitializeComponent();
			BindingContext = checkpoint;
		}

		async Task OnAddressTapped(object sender, EventArgs e) {
			// todo show location on map
			await DisplayAlert("Debug", "Feature not yet implemented.", "Ok");
		}

		async Task OnWebsiteAddressTapped(object sender, EventArgs e) {
			// todo 
			await DisplayAlert("Debug", "Feature not yet implemented.", "Ok");
		}
	}
}
