using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Trace {
	public class MainPage : MasterDetailPage {
		readonly HomeMasterPage masterPage;

		public MainPage() {
			Debug.WriteLine("MainPage Instantiated");
			masterPage = new HomeMasterPage();
			masterPage.ListView.ItemTapped += onItemTapped;
			Master = masterPage;

			var detailPage = new NavigationPage(new HomePage());
			detailPage.BarBackgroundColor = (Color) Application.Current.Resources["PrimaryColor"];
			detailPage.BarTextColor = (Color) Application.Current.Resources["PrimaryTextColor"];
			Detail = detailPage;

			// Place geofences on user favorite & closest checkpoints.
			DependencyService.Get<GeofencingBase>().Init();
		}

		void onItemTapped(object sender, ItemTappedEventArgs e) {
			var item = e.Item as MasterPageItem;
			masterPage.ListView.SelectedItem = null;
			IsPresented = false;

			// The tutorial is a modal page, so we cant push it to the masterdetail navigation page.
			if(item.TargetType == typeof(TutorialPage)) {
				Navigation.PushModalAsync((Page) Activator.CreateInstance(item.TargetType));
				return;
			}

			if(item != null) {
				var nextPage = new NavigationPage((Page) Activator.CreateInstance(item.TargetType));
				nextPage.BarBackgroundColor = (Color) Application.Current.Resources["PrimaryColor"];
				nextPage.BarTextColor = (Color) Application.Current.Resources["PrimaryTextColor"];
				Detail = nextPage;
			}
		}
	}
}

