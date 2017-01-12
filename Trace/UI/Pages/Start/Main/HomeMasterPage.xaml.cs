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
				IconSource = "images/home/home.png",
				TargetType = typeof(HomePage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Rewards,
				IconSource = "images/home/reward.png",
				TargetType = typeof(RewardsListPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Dashboard,
				IconSource = "images/home/dashboard.png",
				TargetType = typeof(DashboardPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.MyRoutes,
				IconSource = "images/home/my_routes.png",
				TargetType = typeof(MyTrajectoriesPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.JoinACampaign,
				IconSource = "images/home/subscribed_campaigns.png",
				TargetType = typeof(CampaignsPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = Language.Settings,
				IconSource = "images/home/settings.png",
				TargetType = typeof(SettingsPage)
			});

			listView.ItemsSource = masterPageItems;

			BindingContext = User.Instance;
		}
	}
}
