using System;
using System.Diagnostics;
using CoreLocation;
using FFImageLoading.Forms.Touch;
using Firebase.CloudMessaging;
using Foundation;
using CoreMotion;
using Google.Core;
using Google.SignIn;
using UIKit;
using UserNotifications;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Trace.iOS {
	[Register("AppDelegate")]
	public partial class AppDelegate :
			global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate {

		public CLLocationManager locationManager;
		public NotificationManager notificationManager;
		private CMMotionActivityManager activityManager;

		private long prevTime; // seconds

		public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions) {
			Debug.WriteLine("Started AppDelegate");
			global::Xamarin.Forms.Forms.Init();

			// Initialize map
			Xamarin.FormsMaps.Init();

			// Register for GSM and shop/checkpoint geofencing events to wake the app in the background.
			locationManager = new CLLocationManager();
			locationManager.RequestAlwaysAuthorization(); // to access user's location in the background
			locationManager.RequestWhenInUseAuthorization(); // to access user's location when the app is in use.
			locationManager.AllowsBackgroundLocationUpdates = true;
			locationManager.StartMonitoringSignificantLocationChanges();
			//locationManager.LocationsUpdated -= onLocationUpdate;
			locationManager.LocationsUpdated += onLocationUpdate;
			//slocationManager.AuthorizationChanged -= onAuthorizationChanged;
			locationManager.AuthorizationChanged += onAuthorizationChanged;
			Geofencing.LocMgr = locationManager;

			LoadApplication(new App());

			// Library for faster image processing.
			CachedImageRenderer.Init();

			// Stats and plot lib initialization.
			OxyPlot.Xamarin.Forms.Platform.iOS.PlotViewRenderer.Init();

			// Apply theme to Navigation bar.
			//UINavigationBar.Appearance.BarTintColor = UIColor.FromRGB(75, 177, 102); //bar background
			UINavigationBar.Appearance.TintColor = UIColor.White; //Tint color of button items
			UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes() {
				Font = UIFont.FromName("HelveticaNeue-Light", (nfloat) 20f),
				TextColor = UIColor.White
			});

			// Apply color theme to iOS tab bar.
			var tabBarTint = (Xamarin.Forms.Color) App.Current.Resources["PrimaryColor"];
			var r = new nfloat(tabBarTint.R);
			var g = new nfloat(tabBarTint.G);
			var b = new nfloat(tabBarTint.B);
			UITabBar.Appearance.SelectedImageTintColor = UIColor.FromRGB(r, g, b);

			// Google sign-in for iOS.
			var googleOAuthConfig = new GoogleOAuthConfig();
			string clientId = googleOAuthConfig.ClientId;
			NSError configureError;
			Context.SharedInstance.Configure(out configureError);
			if(configureError != null) {
				// If something went wrong, assign the clientID manually
				Console.WriteLine("Error configuring the Google context: {0}", configureError);
				SignIn.SharedInstance.ClientID = clientId;
			}

			notificationManager = new NotificationManager();
			notificationManager.RegisterLocalNotifications();
			notificationManager.RegisterRemoteNotifications();
			notificationManager.HandleAppStartFromNotification(launchOptions);

			// Initialize Firebase configuration for push notifications.
			Firebase.Analytics.App.Configure();

			activityManager = new CMMotionActivityManager();
			prevTime = (long) NSDate.Now.SecondsSinceReferenceDate;

			return base.FinishedLaunching(uiApplication, launchOptions);
		}


		/// <summary>
		/// When the iOS app enters background, register for cell tower change events.
		/// When an event fires, query the motion data obtained in the meantime and update the eligibility state machine.
		/// The app is given about 5 seconds of processing time before being suspended when entering this method.
		/// </summary>
		/// <param name="uiApplication">User interface application.</param>
		public override void DidEnterBackground(UIApplication uiApplication) {
			base.DidEnterBackground(uiApplication);
			var isUserLoggedIn = WebServerLoginManager.IsOfflineLoggedIn;
			//if(CLLocationManager.LocationServicesEnabled && isUserLoggedIn && !User.Instance.IsBackgroundAudioEnabled && !Geolocator.IsTrackingInProgress) {

			//	 TODO this would be a good time for releasing system resources to help prevent app from being terminated by the OS... call preparetologout() ?
			//	nint taskID = UIApplication.SharedApplication.BeginBackgroundTask(() => { });
			//	new Task(() => {
			//		Debug.WriteLine($"DidEnterBackground() 2 -> monitoring for significant motion changes");
			//		//locationManager.StartMonitoringSignificantLocationChanges();

			//		// On location change, start background task to get enough time to process motion history.
			//		// This is to make sure there is not a new event handler added every time!
			//		locationManager.LocationsUpdated -= onLocationUpdate;
			//		locationManager.LocationsUpdated += onLocationUpdate;

			//		locationManager.AuthorizationChanged -= onAuthorizationChanged;
			//		locationManager.AuthorizationChanged += onAuthorizationChanged;

			//		Debug.WriteLine($"DidEnterBackground() -> ending task");
			//		UIApplication.SharedApplication.EndBackgroundTask(taskID);
			//	}).Start();
			//}
		}

		private void onAuthorizationChanged(object sender, CLAuthorizationChangedEventArgs e) {
			if(e.Status != CLAuthorizationStatus.AuthorizedAlways) {
				locationManager.StopMonitoringSignificantLocationChanges();
				locationManager.StopUpdatingLocation();
				DependencyService.Get<GeofencingBase>().RemoveAllGeofences();
			}
		}


		/// <summary>
		/// Event triggered when the user switches cell towers.
		/// When this happens, update average speed to check if the user is cycling.
		/// Geofence the closest checkpoints. 
		/// Fetch motion data and update state machine.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="args">Arguments.</param>
		private void onLocationUpdate(object sender, CLLocationsUpdatedEventArgs args) {
			Debug.WriteLine($"onLocationUpdate");

			// Check locations obtained to determine user speed (helps with cycling activity detection).
			foreach(var loc in args.Locations) {
				Debug.WriteLine($"gsm location: {loc.Coordinate}, speed {loc.Speed} m/s");
				if(loc.Speed > 0)
					Geolocator.CumulativeAvgSpeed = loc.Speed;
			}

			// Update geofences.
			var firstLoc = args.Locations.FirstOrDefault();
			if(firstLoc != null && WebServerLoginManager.IsOfflineLoggedIn) {
				DependencyService.Get<GeofencingBase>().RefreshGeofences(
					new Plugin.Geolocator.Abstractions.Position {
						Longitude = firstLoc.Coordinate.Longitude,
						Latitude = firstLoc.Coordinate.Latitude
					}
				);
			}

			// If the user does not have background audio enabled, query activity history to insert into the state machine.
			if(!User.Instance.IsBackgroundAudioEnabled) {
				nint taskID = UIApplication.SharedApplication.BeginBackgroundTask(() => { });
				new Task(() => {
					var now = (long) NSDate.Now.SecondsSinceReferenceDate;
					Debug.WriteLine($"onLocationUpdate -> Processing in background at time {NSDate.Now}");
					queryMotionData(now, taskID);
				}).Start();
			}
		}


		public override void WillEnterForeground(UIApplication uiApplication) {
			var isUserLoggedIn = WebServerLoginManager.IsOfflineLoggedIn;
			if(CLLocationManager.LocationServicesEnabled && isUserLoggedIn && !User.Instance.IsBackgroundAudioEnabled && !Geolocator.IsTrackingInProgress) {
				//locationManager.StopMonitoringSignificantLocationChanges();

				var now = (long) NSDate.Now.SecondsSinceReferenceDate;
				Debug.WriteLine($"Processing in Foreground at time {NSDate.Now}");
				queryMotionData(now, new nint(-1));
				prevTime = now;

			}
			// TODO delete debug message
			new NotificationMessage().Send("activityQueryDebug", "willEnterForeground", App.DEBUG_ActivityLog, 1);
		}


		void queryMotionData(long now, nint taskID) {
			var opQueue = new NSOperationQueue();
			var previousDate = NSDate.FromTimeIntervalSinceReferenceDate(prevTime);
			var nowDate = NSDate.FromTimeIntervalSinceReferenceDate(now);

			activityManager.QueryActivity(start: previousDate, end: nowDate, queue: opQueue, handler: (activities, error) => {

				Debug.WriteLine($"{error?.LocalizedFailureReason}");
				Debug.WriteLine($"prevDate: {previousDate}, now: {nowDate}, nr. activities: {activities?.Length}");
				if(activities != null && activities.Length > 0) {
					var start = activities[0].Timestamp;

					foreach(var a in activities) {
						Debug.WriteLine($"{MotionActivityManager.ActivityToType(a)} {a.StartDate}");
						App.DEBUG_ActivityLog += $"{MotionActivityManager.ActivityToType(a)} {a.StartDate}" + "\n";
						if(a.Confidence != CMMotionActivityConfidence.Low) {
							var elapsed = (int) (a.Timestamp - start); // TODO check if timestamp is from reference time and in secs
							RewardEligibilityManager.Instance.Input(MotionActivityManager.ActivityToType(a), elapsed * 1000);
						}
					}
					//new NotificationMessage().Send("debug", "DEBUG", DEBUG_activityString, activities.Length);
				}
				prevTime = now;
				if(taskID.Equals(new nint(-1))) {
					Debug.WriteLine($"onLocationUpdate -> Ending background task");
					UIApplication.SharedApplication.EndBackgroundTask(taskID);
				}
			});
		}


		/// <summary>
		/// Required for the iOS google sign-in SDK.
		/// </summary>
		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation) {
			return SignIn.SharedInstance.HandleUrl(url, sourceApplication, annotation);
		}
	}
}
