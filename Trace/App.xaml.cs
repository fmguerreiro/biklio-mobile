using Plugin.Connectivity;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public partial class App : Application {

		public static string AppName { get { return "Trace"; } }

		public App() {
			// Localize the display language.
			var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
			Language.Culture = ci; // set the RESX for resource localization
			DependencyService.Get<ILocalize>().SetLocale(ci); // set the Thread for locale-aware methods

			InitializeComponent();

			MainPage = new NavigationPage(new StartPage());
		}

		protected override void OnStart() {
			// Handle when your app starts
			CrossConnectivity.Current.ConnectivityChanged += LoginManager.OnConnectivityChanged;
		}

		protected override void OnSleep() {
			// Handle when your app sleeps
			CrossConnectivity.Current.ConnectivityChanged -= LoginManager.OnConnectivityChanged;
		}

		protected override void OnResume() {
			// Handle when your app resumes
			CrossConnectivity.Current.ConnectivityChanged += LoginManager.OnConnectivityChanged;
		}
	}
}
