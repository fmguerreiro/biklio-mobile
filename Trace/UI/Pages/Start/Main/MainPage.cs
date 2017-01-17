using System;
using Xamarin.Forms;

namespace Trace {
	public class MainPage : MasterDetailPage {
		readonly HomeMasterPage masterPage;

		public MainPage() {

			masterPage = new HomeMasterPage();
			masterPage.ListView.ItemSelected += OnItemSelected;
			Master = masterPage;

			var detailPage = new NavigationPage(new HomePage());
			detailPage.BarBackgroundColor = (Color) Application.Current.Resources["PrimaryColor"];
			detailPage.BarTextColor = (Color) Application.Current.Resources["PrimaryTextColor"];
			Detail = detailPage;
		}

		void OnItemSelected(object sender, SelectedItemChangedEventArgs e) {
			var item = e.SelectedItem as MasterPageItem;
			if(item != null) {
				var nextPage = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
				nextPage.BarBackgroundColor = (Color) Application.Current.Resources["PrimaryColor"];
				nextPage.BarTextColor = (Color) Application.Current.Resources["PrimaryTextColor"];
				Detail = nextPage;
				masterPage.ListView.SelectedItem = null;
				IsPresented = false;
			}
		}
	}
}

