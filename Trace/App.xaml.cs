using System.Reflection;
using Plugin.Connectivity;
using Xamarin.Forms;
using Trace.Localization;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Trace {
	public partial class App : Application {

		public static string AppName { get { return "Trace"; } }

		public App() {
			// Localize the display language.
			var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
			Language.Culture = ci; // set the RESX for resource localization
			DependencyService.Get<ILocalize>().SetLocale(ci); // set the Thread for locale-aware methods

			// This is for checking if the language resource file is linked correctly, i.e., Language.Designer.cs
			//var assembly = typeof(StartPage).GetTypeInfo().Assembly;
			//foreach(var res in assembly.GetManifestResourceNames()) {
			//	System.Diagnostics.Debug.WriteLine("found resource: " + res);
			//}

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
			Geolocator.LowerAccuracy();
		}

		protected async override void OnResume() {
			// Handle when your app resumes
			CrossConnectivity.Current.ConnectivityChanged += LoginManager.OnConnectivityChanged;
			Geolocator.ImproveAccuracy();
			await Task.Delay(1000);
		}
	}
}
