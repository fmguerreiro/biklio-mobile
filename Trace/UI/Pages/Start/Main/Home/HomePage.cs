using System.Threading.Tasks;
using Plugin.Geolocator;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public class HomePage : TabbedPage {

		public HomePage() {
			Title = Language.Home;

			// Remove 'Back' button to stop users from logging out accidently.
			NavigationPage.SetHasBackButton(this, false);

			// Add tabs to the page.
			var mapPage = new MapPage();
			var challengesPage = new ChallengesPage(mapPage);
			Children.Add(challengesPage);
			Children.Add(mapPage);

			var giftMenuButton = new ToolbarItem(Language.Rewards, "reward.png", () => {
				Navigation.PushAsync(new RewardsListPage());
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
			Navigation.PushAsync(new RewardsListPage());
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
			bool isLogout = await DisplayAlert(Language.Logout, Language.AreYouSure, Language.Yes, Language.No);
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