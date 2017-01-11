using System;
using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public class MainPage : MasterDetailPage {
		HomeMasterPage masterPage;

		public MainPage() {

			masterPage = new HomeMasterPage();
			masterPage.ListView.ItemSelected += OnItemSelected;
			Master = masterPage;

			var detailPage = new NavigationPage(new HomePage());
			detailPage.BarBackgroundColor = (Color) App.Current.Resources["PrimaryColor"];
			Detail = detailPage;

		}

		async void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
			RewardEligibilityManager.Instance.Input(ActivityType.Cycling); // TODO just for testing state machine -- remove!
			var item = e.SelectedItem as MasterPageItem;
			if(item != null) {
				// If 'logout' was clicked.
				if(item.TargetType.Equals(typeof(SignInPage))) {
					await logout(item);
				}
				// Else load another page from the menu.
				else {
					var nextPage = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
					nextPage.BarBackgroundColor = (Color) App.Current.Resources["PrimaryColor"];
					Detail = nextPage;
					masterPage.ListView.SelectedItem = null;
					IsPresented = false;
				}
			}
		}

		async Task logout(MasterPageItem item) {
			bool isLogout = await DisplayAlert(Language.Logout, Language.AreYouSure, Language.Yes, Language.No);
			await DisplayAlert("Activity Results", App.DEBUG_ActivityLog, "Ok");
			App.DEBUG_ActivityLog = "";
			if(isLogout) {
				await LoginManager.PrepareLogout();
				var nextPage = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
				nextPage.BarBackgroundColor = (Color) App.Current.Resources["PrimaryColor"];
				Application.Current.MainPage = nextPage;
			}
		}
	}
}
