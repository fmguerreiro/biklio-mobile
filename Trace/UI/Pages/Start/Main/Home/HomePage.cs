using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public class HomePage : TabbedPage {

		static ToolbarItem rewardToolBarItem;

		public HomePage() {
			Title = Language.Home;

			// Remove 'Back' button to stop users from logging out accidently.
			NavigationPage.SetHasBackButton(this, false);

			// Add tabs to the page.
			var mapPage = new MapPage();
			var challengesPage = new CheckpointListPage(mapPage);

			Children.Add(challengesPage);
			Children.Add(mapPage);

			if(rewardToolBarItem == null) {
				rewardToolBarItem = new ToolbarItem(Language.Rewards, "home__number0.png", async () => {
					await Navigation.PushAsync(new RewardsListPage());
				});
			}
			UpdateRewardIcon(User.Instance.GetRewards().Count);
			ToolbarItems.Add(rewardToolBarItem);

			//var tutorialToolbarItem = new ToolbarItem(Language.Tutorial, "home__tutorial.png", async () => {
			//	await Navigation.PushModalAsync(new TutorialPage());
			//});
			//ToolbarItems.Add(tutorialToolbarItem);

			// Show tutorial to user on first login.
			if(User.Instance.IsFirstLogin) {
				Device.BeginInvokeOnMainThread(async () => {
					await Task.Delay(1500); // await this page to load first.
					await Navigation.PushModalAsync(new TutorialPage());
				});
			}
		}

		/// <summary>
		/// Modify the icon to reflect the number of rewards the user is eligible for.
		/// </summary>
		/// <param name="nRewards">Number of rewards.</param>
		public static void UpdateRewardIcon(int nRewards) {
			if(rewardToolBarItem == null) return;
			switch(nRewards) {
				case 0: rewardToolBarItem.Icon = "home__number0.png"; return;
				case 1: rewardToolBarItem.Icon = "home__number1.png"; return;
				case 2: rewardToolBarItem.Icon = "home__number2.png"; return;
				case 3: rewardToolBarItem.Icon = "home__number3.png"; return;
				case 4: rewardToolBarItem.Icon = "home__number4.png"; return;
				case 5: rewardToolBarItem.Icon = "home__number5.png"; return;
				case 6: rewardToolBarItem.Icon = "home__number6.png"; return;
				case 7: rewardToolBarItem.Icon = "home__number7.png"; return;
				case 8: rewardToolBarItem.Icon = "home__number8.png"; return;
				case 9: rewardToolBarItem.Icon = "home__number9.png"; return;
				default: rewardToolBarItem.Icon = "home__number9plus.png"; return;
			}
		}
	}
}