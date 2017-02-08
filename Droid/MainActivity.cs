using Acr.UserDialogs;
using Android.App;
using Android.Content.PM;
using Android.OS;
using FFImageLoading.Forms.Droid;
using Plugin.Permissions;
using Xamarin.Forms;

namespace Trace.Droid {
	[Activity(Label = "Biklio", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
		protected override void OnCreate(Bundle savedInstanceState) {
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(savedInstanceState);

			global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

			// Library for Toast messages.
			UserDialogs.Init(() => (Activity) Forms.Context);

			// Library for faster image processing.
			CachedImageRenderer.Init();

			// Chart and graph visualize tool init.
			OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();

			// Initialize map
			Xamarin.FormsMaps.Init(this, savedInstanceState);

			LoadApplication(new App());
		}


		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults) {
			PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}
