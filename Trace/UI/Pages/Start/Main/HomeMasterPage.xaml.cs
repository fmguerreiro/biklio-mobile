using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Trace {
	public partial class HomeMasterPage : ContentPage {

		public ListView ListView { get { return listView; } }

		public HomeMasterPage() {
			InitializeComponent();

			var masterPageItems = new List<MasterPageItem>();
			masterPageItems.Add(new MasterPageItem {
				Title = "Home",
				IconSource = "home.png",
				TargetType = typeof(HomePage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = "Rewards",
				IconSource = "rewards.png",
				TargetType = typeof(RewardsPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = "Dashboard",
				IconSource = "dashboard.png",
				TargetType = typeof(DashboardPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = "My Routes",
				IconSource = "my_routes.png",
				TargetType = typeof(MyTrajectoriesPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = "Settings",
				IconSource = "settings.png",
				TargetType = typeof(SettingsPage)
			});
			masterPageItems.Add(new MasterPageItem {
				Title = "Logout",
				IconSource = "logout.png",
				TargetType = typeof(StartPage)
			});
			listView.ItemsSource = masterPageItems;
		}
	}
}
