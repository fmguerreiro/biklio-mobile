namespace Trace {

	/// <summary>
	/// JSON representation of the user.
	/// Used when logging in and registering.
	/// </summary>
	public class WSUser {

		public string name { get; set; }
		public string username { get; set; }
		public string password { get; set; }
		public string email { get; set; }
		public string confirm { get; set; }
		public string phone { get; set; }
		public string address { get; set; }

		public string idToken { get; set; }
	}
}
