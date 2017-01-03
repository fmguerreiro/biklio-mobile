using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {

	public partial class CheckpointDetailsPage : ContentPage {

		Checkpoint checkpoint;

		public CheckpointDetailsPage(Checkpoint checkpoint) {
			InitializeComponent();
			this.checkpoint = checkpoint;
			BindingContext = checkpoint;
		}


		/// <summary>
		/// On tap show map page centered on the checkpoint location.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		async void AddressOnTap(object sender, EventArgs e) {

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
			if(checkpoint.Latitude == 0.0D || checkpoint.Longitude == 0.0D) {
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
				await DisplayAlert("Error", "This checkpoint did not have available coordinates. Likely a database issue.", "Ok");
				return;
			}

			// Go back to (tabbed) Home page.
			await Navigation.PopToRootAsync();
			var homePage = ((NavigationPage) ((MainPage) Application.Current.MainPage).Detail).CurrentPage as HomePage;

			// Shift to map page tab.
			var mapPage = (MapPage) homePage.Children.Last();
			homePage.CurrentPage = mapPage;

			var pos = new Position(latitude: checkpoint.Latitude, longitude: checkpoint.Longitude);
			Geolocator.UpdateMap(pos);
		}
	}
}
