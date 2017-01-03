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
				IconSource = "home.png",
				TargetType = typeof(HomePage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Rewards,
				IconSource = "rewards.png",
				TargetType = typeof(RewardsListPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Dashboard,
				IconSource = "dashboard.png",
				TargetType = typeof(DashboardPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.MyRoutes,
				IconSource = "my_routes.png",
				TargetType = typeof(MyTrajectoriesPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.JoinACampaign,
				IconSource = "subscribed_campaigns.png",
				TargetType = typeof(CampaignsPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Settings,
				IconSource = "settings.png",
				TargetType = typeof(SettingsPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Logout,
				IconSource = "logout.png",
				TargetType = typeof(SignInPage)
			});
			listView.ItemsSource = masterPageItems;

			BindingContext = User.Instance;
		}
	}
}
