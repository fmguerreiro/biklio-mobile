using Plugin.Connectivity;
using Xamarin.Forms;

namespace Trace {
	public partial class App : Application {

		public static string AppName { get { return "Trace"; } }

		public App() {
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
