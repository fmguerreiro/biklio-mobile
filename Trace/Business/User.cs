namespace Trace {

	/// <summary>
	/// The User class stores the user's information.
	/// Implements the Singleton pattern.
	/// </summary>
	public class User {

		private const int DEFAULT_RADIUS = 10000;

		private static User instance;

		private User() { }

		public static User Instance {
			get {
				if(instance == null) { instance = new User(); }
				return instance;
			}
		}

		public static string Username { get; set; }
		public static string Password { get; set; }
		public static string Email { get; set; }
		public static string AuthToken { get; set; }
		public static int SearchRadiusInKM { get; set; } = DEFAULT_RADIUS;
	}
}
