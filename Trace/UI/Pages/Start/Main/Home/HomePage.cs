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
			var challengesPage = new ChallengeListPage(mapPage);
			Children.Add(challengesPage);
			Children.Add(mapPage);

			var giftToolbarButton = new ToolbarItem(Language.Rewards, "reward.png", async () => {
				await Navigation.PushAsync(new RewardsListPage());
			});
			ToolbarItems.Add(giftToolbarButton);

			var tutorialToolbarButton = new ToolbarItem(Language.Tutorial, "tutorial.png", async () => {
				await Navigation.PushModalAsync(new TutorialPage());
			});
			ToolbarItems.Add(tutorialToolbarButton);
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