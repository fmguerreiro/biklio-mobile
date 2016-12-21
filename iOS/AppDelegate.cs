using System;
using Foundation;
using Google.Core;
using Google.SignIn;
using UIKit;

namespace Trace.iOS {
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate {
		public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
			global::Xamarin.Forms.Forms.Init();

			// Initialize map
			Xamarin.FormsMaps.Init();

			// Code for starting up the Xamarin Test Cloud Agent
#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
#endif

			LoadApplication(new App());

			// Stats and plot lib initialization.
			OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();

			// Apply green theme to Navigation bar.
			UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(75, 177, 102); //bar background
			UINavigationBar.Appearance.TintColor = UIColor.White; //Tint color of button items
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes() {
				Font = UIFont.FromName("HelveticaNeue-Light", (nfloat) 20f),
				TextColor = UIColor.White
			});
			UITabBar.Appearance.SelectedImageTintColor = UIColor.FromRGB(36, 193, 33);

			// Google sign-in for iOS.
			string clientId = new GoogleOAuthConfig().ClientId;
			NSError configureError;
			Context.SharedInstance.Configure(out configureError);
			if(configureError != null) {
				// If something went wrong, assign the clientID manually
				Console.WriteLine("Error configuring the Google context: {0}", configureError);
				SignIn.SharedInstance.ClientID = clientId;
			}

			// Initialize Firebase for push notifications.
			Firebase.Analytics.App.Configure();

			return base.FinishedLaunching(app, options);
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation) {
			return SignIn.SharedInstance.HandleUrl(url, sourceApplication, annotation);
		}
	}
}
