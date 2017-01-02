using System;
using System.Diagnostics;
using FFImageLoading.Forms.Touch;
using Firebase.CloudMessaging;
using Foundation;
using Google.Core;
using Google.SignIn;
using UIKit;
using UserNotifications;

namespace Trace.iOS {
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate, IUNUserNotificationCenterDelegate, IMessagingDelegate {

		public override bool FinishedLaunching(UIApplication app, NSDictionary options) {
			global::Xamarin.Forms.Forms.Init();

			// Initialize map
			Xamarin.FormsMaps.Init();

			// Code for starting up the Xamarin Test Cloud Agent
#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
#endif

			LoadApplication(new App());

			// Library for faster image processing.
			CachedImageRenderer.Init();

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

			return base.FinishedLaunching(app, options);
		}


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
