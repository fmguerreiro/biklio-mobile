using System;

using Xamarin.Forms;

namespace Trace {
	public class HomePage : TabbedPage {
		private const string MENU = "Menu";
		private const string MY_REWARDS = "My Rewards";
		private const string DASHBOARD = "Dashboard";
		private const string MY_ROUTES = "My routes";
		private const string SETTINGS = "Settings";
		private const string LOGOUT = "Logout";

		public HomePage() {
			this.Title = "Home";

			// Remove 'Back' button to stop users from logging out accidently.
			NavigationPage.SetHasBackButton(this, false);

			// Add tabs to the page.
			this.Children.Add(new ChallengesPage());
			this.Children.Add(new MapPage());

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

		async void OnMyRewardsClicked() {
			await Navigation.PushAsync(new RewardsPage());
		}

		async void OnDashboardClicked() {
			await Navigation.PushAsync(new DashboardPage());
		}

		async void OnMyRoutesClicked() {
			await Navigation.PushAsync(new MyRoutesPage());
		}

		async void OnSettingsClicked() {
			await Navigation.PushAsync(new SettingsPage());
		}

		async void OnLogoutClicked() {
			bool isLogout = await DisplayAlert("Logout", "Are you sure?", "Yes", "No");
			if(isLogout) {
				await Navigation.PopToRootAsync();
			}
		}

	}
}