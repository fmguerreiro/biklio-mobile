using System.Collections.Generic;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public partial class HomeMasterPage : ContentPage {

		public ListView ListView { get { return listView; } }

		public HomeMasterPage() {
			InitializeComponent();
			var masterPageItems = new List<MasterPageItem>();
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Home,
				IconSource = "home__home.png",
				TargetType = typeof(HomePage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Rewards,
				IconSource = "home__reward.png",
				TargetType = typeof(RewardsListPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Dashboard,
				IconSource = "home__dashboard.png",
				TargetType = typeof(DashboardPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.MyRoutes,
				IconSource = "home__my_routes.png",
				TargetType = typeof(MyTrajectoriesListPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.JoinACampaign,
				IconSource = "home__subscribed_campaigns.png",
				TargetType = typeof(CampaignsPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Settings,
				IconSource = "home__settings.png",
				TargetType = typeof(SettingsPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Tutorial,
				IconSource = "home__tutorial.png",
				TargetType = typeof(TutorialPage)
			});

			listView.ItemsSource = masterPageItems;

			BindingContext = User.Instance;
		}
	}
}
