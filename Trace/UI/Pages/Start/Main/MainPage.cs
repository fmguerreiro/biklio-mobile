using System;
using Xamarin.Forms;

namespace Trace {
	public class MainPage : MasterDetailPage {
		HomeMasterPage masterPage;

		public MainPage() {

			masterPage = new HomeMasterPage();
			masterPage.ListView.ItemSelected += OnItemSelected;
			Master = masterPage;

			var detailPage = new NavigationPage(new HomePage());
			detailPage.BarBackgroundColor = (Color) App.Current.Resources["PrimaryColor"];
			detailPage.BarTextColor = (Color) App.Current.Resources["PrimaryTextColor"];
			Detail = detailPage;

		}

		void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
			var item = e.SelectedItem as MasterPageItem;
			if(item != null) {
				var nextPage = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
				nextPage.BarBackgroundColor = (Color) App.Current.Resources["PrimaryColor"];
				nextPage.BarTextColor = (Color) App.Current.Resources["PrimaryTextColor"];
				Detail = nextPage;
				masterPage.ListView.SelectedItem = null;
				IsPresented = false;
			}
		}
	}
}

