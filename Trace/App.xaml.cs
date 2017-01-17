using Plugin.Connectivity;
using Xamarin.Forms;
using Trace.Localization;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.Threading.Tasks;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace Trace {

	public partial class App : Application {

		public static string AppName { get { return "Trace"; } }

		public static string DEBUG_ActivityLog = "";

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

			// First page of the application.
			var firstPage = new NavigationPage(new SignInPage());
			firstPage.BarBackgroundColor = (Color) Current.Resources["PrimaryColor"];
			firstPage.BarTextColor = (Color) Current.Resources["PrimaryTextColor"];
			MainPage = firstPage;
		}


		/// <summary>
		/// Handle when your app starts
		/// </summary>
		protected override void OnStart() {
			CrossConnectivity.Current.ConnectivityChanged += LoginManager.OnConnectivityChanged;
		}


		/// <summary>
		/// Handle when your app enters background. Runs for about 5 seconds.
		/// </summary>
		protected override void OnSleep() {
			CrossConnectivity.Current.ConnectivityChanged -= LoginManager.OnConnectivityChanged;
			Geolocator.TryLowerAccuracy();

			var isUserSignedIn = LoginManager.IsOfflineLoggedIn;
			// Use either audio background if enabled or cell tower changes in iOS to fetch motion data in background (moved to AppDelegate.cs in iOS).
			if(!Geolocator.IsTrackingInProgress && isUserSignedIn) {

				if(User.Instance.IsBackgroundAudioEnabled) {
					Device.BeginInvokeOnMainThread(() => {
						DependencyService.Get<ISoundPlayer>().ActivateAudioSession();
						DependencyService.Get<ISoundPlayer>().PlaySound(null); // null -> play previous track
						Debug.WriteLine("StartAudioSession(): " + DependencyService.Get<ISoundPlayer>().IsPlaying());
					});
				}
				//else {
				//	await Geolocator.StartListeningSignificantLocationChanges();
				//}
			}

			// Serialize and store KPIs obtained.
			User.Instance.GetCurrentKPI().StoreKPI();
		}


		/// <summary>
		/// Handle when your app resumes.
		/// </summary>
		protected override void OnResume() {
			CrossConnectivity.Current.ConnectivityChanged += LoginManager.OnConnectivityChanged;

			Geolocator.ImproveAccuracy();
			//await Task.Delay(1000);
			if(LoginManager.IsOfflineLoggedIn) {
				DependencyService.Get<ISoundPlayer>().StopSound();
				DependencyService.Get<ISoundPlayer>().DeactivateAudioSession();
			}
			Debug.WriteLine("EndAudioSession(): " + !DependencyService.Get<ISoundPlayer>().IsPlaying());
		}
	}
}