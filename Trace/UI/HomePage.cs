using System;

using Xamarin.Forms;

namespace Trace {
	public class HomePage : TabbedPage {
		private const string MENU = "Menu";
		private const string MY_REWARDS = "My Rewards";
		private const string DASHBOARD = "Dashboard";
		private const string MY_ROUTES = "My Routes";
		private const string SETTINGS = "Settings";
		private const string LOGOUT = "Logout";

		public HomePage() {
			Title = "Home";

			// Remove 'Back' button to stop users from logging out accidently.
			NavigationPage.SetHasBackButton(this, false);

			// Add tabs to the page.
			Children.Add(new ChallengesPage());
			Children.Add(new MapPage());

			// Add a Menu button to the page header to navigate the app.
			ToolbarItems.Add(new ToolbarItem(MENU, "", async () => {
				var action = await DisplayActionSheet(MENU, "Back", null, MY_REWARDS, DASHBOARD, MY_ROUTES, SETTINGS, LOGOUT);
				switch(action) {
					case MY_REWARDS: OnMyRewardsClicked(); return;
					case DASHBOARD: OnDashboardClicked(); return;
					case MY_ROUTES: OnMyRoutesClicked(); return;
					case SETTINGS: OnSettingsClicked(); return;
					case LOGOUT: OnLogoutClicked(); return;
					default: return;
				}
			}));
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

		async void OnLogoutClicked() {
			bool isLogout = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");
			if(isLogout) {
				User.Instance = new User();
				await Navigation.PopToRootAsync();
			}
		}

	}
}