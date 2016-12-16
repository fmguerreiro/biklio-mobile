using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace Trace {
	public class HomePage : TabbedPage {
		private const string PAGE_TITLE = "Home";

		public HomePage() {
			Title = PAGE_TITLE;

			// Remove 'Back' button to stop users from logging out accidently.
			NavigationPage.SetHasBackButton(this, false);

			// Add tabs to the page.
			var mapPage = new MapPage();
			var challengesPage = new ChallengesPage(mapPage);
			Children.Add(challengesPage);
			Children.Add(mapPage);

			var giftMenuButton = new ToolbarItem("Rewards", "reward", async () => {
				await Navigation.PushAsync(new RewardsPage());
			});
			ToolbarItems.Add(giftMenuButton);

			// Add a Menu button to the page header to navigate the app.
			//ToolbarItems.Add(new ToolbarItem(MENU, "", async () => {
			//	var action = await DisplayActionSheet(MENU, CANCEL, null, MY_REWARDS, DASHBOARD, MY_ROUTES, SETTINGS, LOGOUT);
			//	switch(action) {
			//		case MY_REWARDS: OnMyRewardsClicked(); return;
			//		case DASHBOARD: OnDashboardClicked(); return;
			//		case MY_ROUTES: OnMyRoutesClicked(); return;
			//		case SETTINGS: OnSettingsClicked(); return;
			//		case LOGOUT: await OnLogoutClicked(); return;
			//		default: return;
			//	}
			//}));
		}

		void OnMyRewardsClicked() {
			Navigation.PushAsync(new RewardsPage());
		}

		void OnDashboardClicked() {
			Navigation.PushAsync(new DashboardPage());
		}

		void OnMyRoutesClicked() {
			Navigation.PushAsync(new MyTrajectoriesPage());
		}

		void OnSettingsClicked() {
			Navigation.PushAsync(new SettingsPage());
		}

		async Task OnLogoutClicked() {
			bool isLogout = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");
			if(isLogout) {
				User.Instance = null;
				RewardEligibilityManager.Instance = null;
				await CrossGeolocator.Current.StopListeningAsync();
				DependencyService.Get<IMotionActivityManager>().StopMotionUpdates();
				await Navigation.PopToRootAsync();
			}
		}

	}
}