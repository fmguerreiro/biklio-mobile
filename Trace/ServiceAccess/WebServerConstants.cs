namespace Trace {

	public static class WebServerConstants {

		public static readonly string ENDPOINT = "http://146.193.41.50:8080/trace";

		public static readonly string LOGIN_ENDPOINT = ENDPOINT + "/auth/login";

		public static readonly string LOGOUT_ENDPOINT = ENDPOINT + "/auth/logout";

		public static readonly string REGISTER_ENDPOINT = ENDPOINT + "/tracker/register";

		public static readonly string FETCH_CHALLENGES_ENDPOINT = ENDPOINT + "/shops/challenges?";

		public static readonly string SUBMIT_TRAJECTORY_SUMMARY = ENDPOINT + "/routes/s";

		public static readonly string SUBMIT_TRAJECTORY = ENDPOINT + "/routes/s/track/trace?session=";
	}
}
