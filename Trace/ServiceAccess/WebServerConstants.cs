namespace Trace {

	public static class WebServerConstants {

		public static readonly string HOST = "146.193.41.50";

		public static readonly int PORT = 8080;

		public static readonly string ENDPOINT = "http://" + HOST + ":" + PORT + "/trace";

		public static readonly string LOGIN_ENDPOINT = ENDPOINT + "/auth/v2/login";

		public static readonly string LOGOUT_ENDPOINT = ENDPOINT + "/auth/logout";

		public static readonly string REGISTER_ENDPOINT = ENDPOINT + "/tracker/register";

		public static readonly string GET_CHALLENGES = ENDPOINT + "/shops/challenges?";

		public static readonly string SUBMIT_TRAJECTORY_SUMMARY = ENDPOINT + "/routes/s";

		public static readonly string SUBMIT_TRAJECTORY = ENDPOINT + "/routes/s/track/trace?session=";

		public static readonly string SUBMIT_KPI = ENDPOINT + "/kpi/s";

		public static readonly string GET_CLOSEST_CAMPAIGN = ENDPOINT + "/campaign/near?";

		public static readonly string SUBSCRIBE_CAMPAIGN = ENDPOINT + "/campaign/subscribe";

		public static readonly string UNSUBSCRIBE_CAMPAIGN = ENDPOINT + "/campaign/unsubscribe";
	}
}
