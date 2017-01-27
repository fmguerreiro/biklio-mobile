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

namespace Trace.iOS {
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate {

		private CLLocationManager locationManager;

		private long prevTime; // seconds
		private CMMotionActivityManager activityManager;

		public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
			global::Xamarin.Forms.Forms.Init();

			// Initialize map
			Xamarin.FormsMaps.Init();

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
				//SignIn.SharedInstance.Scopes = googleOAuthConfig.Scopes;
				//SignIn.SharedInstance.ServerClientID = "***REMOVED***";
			}

			RegisterLocalNotifications();
			RegisterRemoteNotifications();
			HandleAppStartFromNotification(options);

			// Initialize Firebase configuration for push notifications.
			Firebase.Analytics.App.Configure();

			//sendTestNotification();

			locationManager = new CLLocationManager();
			locationManager.RequestAlwaysAuthorization(); //to access user's location in the background
			locationManager.RequestWhenInUseAuthorization(); //to access user's location when the app is in use.
			locationManager.AllowsBackgroundLocationUpdates = true;
			activityManager = new CMMotionActivityManager();
			prevTime = (long) NSDate.Now.SecondsSinceReferenceDate;

			return base.FinishedLaunching(app, options);
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
			if(CLLocationManager.LocationServicesEnabled && isUserLoggedIn && !User.Instance.IsBackgroundAudioEnabled && !Geolocator.IsTrackingInProgress) {
				// TODO this would be a good time for releasing system resources ... call preparetologout() ?
				Task.Run(() => {
					Debug.WriteLine($"DidEnterBackground() -> starting significant motion changes monitoring");
					locationManager.StartMonitoringSignificantLocationChanges();

					// On location change, start background task to get enough time to process motion history.
					// This is to make sure there is not a new event handler added every time!
					locationManager.LocationsUpdated -= onLocationUpdate;
					locationManager.LocationsUpdated += onLocationUpdate;
				});
			}
		}


		private void onLocationUpdate(object sender, CLLocationsUpdatedEventArgs args) {
			nint taskID = UIApplication.SharedApplication.BeginBackgroundTask(() => { });
			new Task(() => {
				var now = (long) NSDate.Now.SecondsSinceReferenceDate;
				Debug.WriteLine($"Location change received: {NSDate.Now.SecondsSinceReferenceDate}");
				processMotionData(now);
				UIApplication.SharedApplication.EndBackgroundTask(taskID);
			}).Start();
		}


		public override void WillEnterForeground(UIApplication uiApplication) {
			var isUserLoggedIn = WebServerLoginManager.IsOfflineLoggedIn;
			if(CLLocationManager.LocationServicesEnabled && isUserLoggedIn && !User.Instance.IsBackgroundAudioEnabled && !Geolocator.IsTrackingInProgress) {
				locationManager.StopMonitoringSignificantLocationChanges();
				var now = (long) NSDate.Now.SecondsSinceReferenceDate;
				processMotionData(now);
				prevTime = now;
			}
		}


		void processMotionData(long now) {
			var opQueue = NSOperationQueue.MainQueue;
			//var prevDate = NSDate.FromTimeIntervalSinceReferenceDate(now.SecondsSinceReferenceDate - 6 * 24 * 60 * 60);
			var previousDate = NSDate.FromTimeIntervalSinceReferenceDate(prevTime);
			var nowDate = NSDate.FromTimeIntervalSinceReferenceDate(now);
			activityManager.QueryActivity(start: previousDate, end: nowDate, queue: opQueue, handler: (activities, error) => {
				Debug.WriteLine($"{error?.LocalizedFailureReason}");
				Debug.WriteLine($"prevDate: {prevTime}, now: {now}, nr. activities: {activities?.Length}, time remaining: {UIApplication.SharedApplication.BackgroundTimeRemaining}");
				RewardEligibilityManager.Instance.Input(ActivityType.Cycling, (int) (now - prevTime) * 1000);
				if(activities != null && activities.Length > 0) {
					var start = activities[0].Timestamp;
					var DEBUG_activityString = "";
					foreach(var a in activities) {
						Debug.WriteLine($"{a.Description}");
						DEBUG_activityString += a.Description + "\n";
						if(a.Confidence != CMMotionActivityConfidence.Low) {
							var elapsed = (int) (a.Timestamp - start); // TODO check if timestamp is from reference time and in secs
							RewardEligibilityManager.Instance.Input(MotionActivityManager.ActivityToType(a), elapsed * 1000);
						}
					}
					new NotificationMessage().Send("debug", "DEBUG", DEBUG_activityString, activities.Length);
				}
				prevTime = now;
			});
		}


		/// <summary>
		/// Required for the iOS google sign-in SDK.
		/// </summary>
		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation) {
			return SignIn.SharedInstance.HandleUrl(url, sourceApplication, annotation);
		}


		/// <summary>
		/// Registers the local notifications.
		/// </summary>
		public static void RegisterLocalNotifications() {
			// Register for local notifications on iOS 10+.
			if(UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
				var settings10 = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge;
				UNUserNotificationCenter.Current.RequestAuthorization(settings10, (approved, err) => {
					// Handle approval
				});

				// TODO Check current notification settings before presenting notifications
				//UNUserNotificationCenter.Current.GetNotificationSettings((settings10) => {
				//	var alertsAllowed = (settings10.AlertSetting == UNNotificationSetting.Enabled);
				//});
			}
			else {
				// Register for local notifications on iOS <= 9.
				var settings9 = UIUserNotificationSettings.GetSettingsForTypes(
					  UIUserNotificationType.Alert | UIUserNotificationType.Badge, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(settings9);
			}
		}

		/// <summary>
		/// When the user starts the app by clicking on a notification.
		/// </summary>
		/// <param name="launchOptions">Launch options.</param>
		private void HandleAppStartFromNotification(NSDictionary launchOptions) {
			// check for a notification
			if(launchOptions != null) {
				// check for a notification
				if(launchOptions.ContainsKey(UIApplication.LaunchOptionsLocalNotificationKey) ||
				   launchOptions.ContainsKey(UIApplication.LaunchOptionsRemoteNotificationKey)) {
					var notification = launchOptions[UIApplication.LaunchOptionsLocalNotificationKey] as UILocalNotification;
					if(notification != null) {
						UIAlertController okayAlertController = UIAlertController.Create(notification.AlertAction, notification.AlertBody, UIAlertControllerStyle.Alert);
						okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

						UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okayAlertController, true, null);

						// reset our badge
						UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
					}
				}
			}
		}


		public void RegisterRemoteNotifications() {
			if(UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
				// iOS 10 or later
				var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge;
				UNUserNotificationCenter.Current.RequestAuthorization(authOptions, (granted, error) => {
					Console.WriteLine(granted);
				});

				// For iOS 10 display notification (sent via APNS)
				UNUserNotificationCenter.Current.Delegate = this;

				// For iOS 10 data message (sent via FCM)
				Messaging.SharedInstance.RemoteMessageDelegate = this;
			}
			else {
				// iOS 9 or before
				var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge;
				var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			}

			UIApplication.SharedApplication.RegisterForRemoteNotifications();
		}


		/// <summary>
		/// Function that handles received local notification in background for iOS 9 or lower.
		/// </summary>
		/// <param name="application">Application.</param>
		/// <param name="notification">Notification.</param>
		public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification) {
			Debug.WriteLine("ReceivedLocalNotification(): " + notification.AlertBody);
			// show an alert
			UIAlertController okayAlertController = UIAlertController.Create(notification.AlertAction, notification.AlertBody, UIAlertControllerStyle.Alert);
			okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
			UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okayAlertController, true, null);

			// reset our badge
			UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
		}


		/// <summary>
		/// Receives remote notifications for iOS 9 and older when in foreground.
		/// Also handles notification for notifications when in the background for any version.
		/// </summary>
		/// <param name="application">Application.</param>
		/// <param name="userInfo">User info.</param>
		/// <param name="completionHandler">Completion handler.</param>
		public override void DidReceiveRemoteNotification(UIApplication application, NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler) {
			// If you are receiving a notification message while your app is in the background,
			// this callback will not be fired 'till the user taps on the notification launching the application.

			// TODO handle the notification data
			Debug.WriteLine("DidReceiveRemoteNotification(): " + userInfo);
		}


		/// <summary>
		/// To receive notifications in foreground on iOS 10 devices.
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="notification">Notification.</param>
		/// <param name="completionHandler">Completion handler.</param>
		[Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
		public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler) {
			Debug.WriteLine("WillPresentNotification(): " + notification);

			// display notification
			var title = notification.Request.Content.Title;
			var body = notification.Request.Content.Body;
			UIAlertController okayAlertController = UIAlertController.Create(title, body, UIAlertControllerStyle.Alert);
			okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
			UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okayAlertController, true, null);

			// reset our badge
			UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
		}


		/// <summary>
		/// Receives data messages on iOS 10.
		/// </summary>
		/// <param name="remoteMessage">Remote message.</param>
		public void ApplicationReceivedRemoteMessage(RemoteMessage remoteMessage) {
			// TODO handle the data message
			Debug.WriteLine("ApplicationReceivedRemoteMessage(): " + remoteMessage);
		}

		public void sendTestNotification() {
			var content = new UNMutableNotificationContent();
			content.Title = "Notification Title";
			content.Subtitle = "Notification Subtitle";
			content.Body = "This is the message body of the notification.";
			content.Badge = 1;

			var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(5, false);

			var requestID = "sampleRequest";
			var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

			UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
				if(err != null) {
					Debug.WriteLine("Notification error: ", err);
				}
			});
		}
	}
}
