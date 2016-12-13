using System;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace Trace {
	public class MainPage : MasterDetailPage {
		HomeMasterPage masterPage;

		public MainPage() {
			masterPage = new HomeMasterPage();
			Master = masterPage;
			Detail = new NavigationPage(new HomePage());

			masterPage.ListView.ItemSelected += OnItemSelected;

			//if(Device.OS == TargetPlatform.Windows) {
			//	Master.Icon = "swap.png";
			//}
		}

		async void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
			var item = e.SelectedItem as MasterPageItem;
			if(item != null) {
				// If 'logout' was clicked.
				if(item.TargetType.Equals(typeof(StartPage))) {
					await logout(item);
				}
				// Else load another page from the menu.
				else {
					Detail = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
					masterPage.ListView.SelectedItem = null;
					IsPresented = false;
				}
			}
		}

		async Task logout(MasterPageItem item) {
			bool isLogout = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");
			if(isLogout) {
				User.Instance = null;
				RewardEligibilityManager.Instance = null;
				await CrossGeolocator.Current.StopListeningAsync();
				DependencyService.Get<IMotionActivityManager>().StopMotionUpdates();
				Application.Current.MainPage = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
			}
		}
	}
}
