namespace Trace {

	public static class OAuthConfigurationManager {

		private static OAuthConfig instance;

		public static void SetConfig(OAuthConfig config) {
			instance = config;
		}

		public static string ClientId { get { return instance.ClientId; } }
		public static string ClientSecret { get { return instance.ClientSecret; } }

		public static string Scope { get { return instance.Scope; } }

		public static string AuthorizeUrl { get { return instance.AuthorizeUrl; } }
		public static string AccessTokenUrl { get { return instance.AccessTokenUrl; } }
		public static string UserInfoUrl { get { return instance.UserInfoUrl; } }

		public static string RedirectUrl { get { return instance.RedirectUrl; } }

		public static string KeystoreService { get { return instance.KeystoreService; } }
	}
}
