using System;
using System.Diagnostics;
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

			RegisterRemoteNotifications();

			// Initialize Firebase for push notifications.
			Firebase.Analytics.App.Configure();

			return base.FinishedLaunching(app, options);
		}


		public override void DidEnterBackground(UIApplication uiApplication) {
			base.DidEnterBackground(uiApplication);
			Debug.WriteLine("Entered BackgroundMode");
			// Messaging.SharedInstance.Disconnect ();
		}


		public override void WillEnterForeground(UIApplication uiApplication) {
			base.WillEnterForeground(uiApplication);
			Debug.WriteLine("Entered ForegroundMode");
		}


		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation) {
			return SignIn.SharedInstance.HandleUrl(url, sourceApplication, annotation);
		}


		public void RegisterRemoteNotifications() {
			if(UIDevice.CurrentDevice.CheckSystemVersion(10, 0)) {
				// iOS 10 or later
				var authOptions = UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound;
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
				var allNotificationTypes = UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound;
				var settings = UIUserNotificationSettings.GetSettingsForTypes(allNotificationTypes, null);
				UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
			}

			UIApplication.SharedApplication.RegisterForRemoteNotifications();
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
			Debug.WriteLine(userInfo);
		}


		/// <summary>
		/// To receive notifications in foreground on iOS 10 devices.
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="notification">Notification.</param>
		/// <param name="completionHandler">Completion handler.</param>
		[Export("userNotificationCenter:willPresentNotification:withCompletionHandler:")]
		public void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler) {
			// TODO handle the notification data
			Debug.WriteLine(notification);
		}


		/// <summary>
		/// Receives data messages on iOS 10.
		/// </summary>
		/// <param name="remoteMessage">Remote message.</param>
		public void ApplicationReceivedRemoteMessage(RemoteMessage remoteMessage) {
			// TODO handle the data message
			Debug.WriteLine(remoteMessage);
		}
	}
}
