using System.Diagnostics;
using UserNotifications;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.iOS.NotificationMessage))]
namespace Trace.iOS {

	/// <summary>
	/// Class responsible for implementing the device-specific send local notification.
	/// </summary>
	public class NotificationMessage : INotificationMessage {

		public void Send(string id, string title, string body, int badgeCount) {
			var content = new UNMutableNotificationContent();
			content.Title = title;
			content.Body = body;
			content.Badge = badgeCount;

			var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(timeInterval: 5, repeats: false);

			var request = UNNotificationRequest.FromIdentifier(id, content, trigger);

			UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
				if(err != null) {
					Debug.WriteLine("Notification error: ", err);
				}
			});
		}
	}
}
