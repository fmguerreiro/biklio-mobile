using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {

	public partial class CheckpointDetailsPage : ContentPage {

		CheckpointViewModel checkpoint;

		public CheckpointDetailsPage(CheckpointViewModel checkpoint) {
			InitializeComponent();
			this.checkpoint = checkpoint;
			BindingContext = checkpoint;

			// Add favorite/unfavorite interaction with star image.
			var starOnTap = new TapGestureRecognizer();
			starOnTap.Tapped += (sender, e) => {
				checkpoint.IsUserFavorite = !checkpoint.IsUserFavorite;
				favoriteStar.Source = checkpoint.FavoriteImage;
				SQLiteDB.Instance.SaveItem(checkpoint.Checkpoint);
			};
			favoriteStar.GestureRecognizers.Add(starOnTap);
		}


		/// <summary>
		/// On tap show map page centered on the checkpoint location.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		async void AddressOnTap(object sender, EventArgs e) {

#pragma warning disable RECS0018 // Comparison of floating point numbers with equality operator
			if(checkpoint.Checkpoint.Latitude == 0.0D || checkpoint.Checkpoint.Longitude == 0.0D) {
#pragma warning restore RECS0018 // Comparison of floating point numbers with equality operator
				await DisplayAlert("Error", "This checkpoint did not have available coordinates. Likely a database issue.", "Ok");
				return;
			}

			// Stops view from going back to user location after centering on checkpoint.
			MapPage.ShouldCenterOnUser = false;

			// Go back to (tabbed) Home page.
			await Navigation.PopToRootAsync();
			var homePage = ((NavigationPage) ((MainPage) Application.Current.MainPage).Detail).CurrentPage as HomePage;

			// Shift to map page tab.
			var mapPage = (MapPage) homePage.Children.Last();
			homePage.CurrentPage = mapPage;

			var pos = new Position(latitude: checkpoint.Checkpoint.Latitude, longitude: checkpoint.Checkpoint.Longitude);
			Geolocator.UpdateMap(pos);
		}
	}
}
