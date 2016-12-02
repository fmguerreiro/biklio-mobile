namespace Trace {

	public static class OAuthConstants {

		// OAuth for Google login
		public static string GoogleClientId = "***REMOVED***-289qb3qjig6gn659k939iqm11csrdo8j.apps.googleusercontent.com";
		public static string GoogleClientSecret = "***REMOVED***";

		// These values do not need changing
		public static string Scope = "https://www.googleapis.com/auth/userinfo.email";
		public static string AuthorizeUrl = "https://accounts.google.com/o/oauth2/auth";
		public static string AccessTokenUrl = "https://accounts.google.com/o/oauth2/token";
		public static string UserInfoUrl = "https://www.googleapis.com/oauth2/v2/userinfo";

		// The location the user will be redirected to after successfully authenticating
		public static string RedirectUrl = "http://blank.org";

		// Unique name for the device keystore where the credentials are stored
		public static string KeystoreService = App.AppName + "_GoogleOAuth";

	}
}