namespace Trace {

	public static class WebServerConstants {

		public static readonly string HOST = "146.193.41.50";

		public static readonly int PORT = 8080;

		public static readonly string ENDPOINT = "http://" + HOST + ":" + PORT + "/trace";

		public static readonly string LOGIN_ENDPOINT = ENDPOINT + "/auth/v2/login";

		public static readonly string LOGOUT_ENDPOINT = ENDPOINT + "/auth/logout";

		public static readonly string REGISTER_ENDPOINT = ENDPOINT + "/tracker/register";

		public static readonly string FETCH_CHALLENGES_ENDPOINT = ENDPOINT + "/shops/challenges?";

		public static readonly string SUBMIT_TRAJECTORY_SUMMARY = ENDPOINT + "/routes/s";

		public static readonly string SUBMIT_TRAJECTORY = ENDPOINT + "/routes/s/track/trace?session=";

		public static readonly string SUBMIT_KPI = ENDPOINT + "/kpi/s";
	}
}
