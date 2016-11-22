namespace Trace {

	public static class WebServerConstants {

		public static readonly string ENDPOINT = "http://146.193.41.50:8080/trace";

		public static readonly string LOGIN_ENDPOINT = ENDPOINT + "/auth/login";

		public static readonly string LOGOUT_ENDPOINT = ENDPOINT + "/auth/logout";

		public static readonly string UPLOAD_TRACK_ENDPOINT = ENDPOINT + "/tracker/put/track/";

		public static readonly string REGISTER_ENDPOINT = ENDPOINT + "/tracker/register";

		public static readonly string FETCH_CHALLENGES_ENDPOINT = ENDPOINT + "/shops/challenges?";

		public static readonly int VERSION = 1;
	}
}
