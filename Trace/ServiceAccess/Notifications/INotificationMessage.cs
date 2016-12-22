namespace Trace {

	/// <summary>
	/// Class responsible for sending a local notification from the shared code library (PCL).
	/// </summary>
	public interface INotificationMessage {

		void Send(string id, string title, string body, int badgeCount);
	}
}
