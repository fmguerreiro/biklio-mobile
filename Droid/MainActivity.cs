using Android.App;
using Android.Content.PM;
using Android.OS;
using FFImageLoading.Forms.Droid;

namespace Trace.Droid {
	[Activity(Label = "Trace.Droid", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
		protected override void OnCreate(Bundle bundle) {
			TabLayoutResource = Resource.Layout.Tabbar;
			ToolbarResource = Resource.Layout.Toolbar;

			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);


			// Library for faster image processing.
			CachedImageRenderer.Init();

			// Chart and graph visualize tool init.
			OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();

			// Initialize map
			Xamarin.FormsMaps.Init(this, bundle);

			LoadApplication(new App());
		}
	}
}
