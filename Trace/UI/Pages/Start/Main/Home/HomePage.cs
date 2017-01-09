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

			var giftToolbarButton = new ToolbarItem(Language.Rewards, "images/home/reward.png", async () => {
				await Navigation.PushAsync(new RewardsListPage());
			});
			ToolbarItems.Add(giftToolbarButton);

			var tutorialToolbarButton = new ToolbarItem(Language.Tutorial, "images/home/tutorial.png", async () => {
				await Navigation.PushModalAsync(new TutorialPage());
			});
			ToolbarItems.Add(tutorialToolbarButton);
		}
	}
}