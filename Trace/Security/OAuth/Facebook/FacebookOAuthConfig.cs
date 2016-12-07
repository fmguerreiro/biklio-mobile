namespace Trace {

	public class FacebookOAuthConfig : OAuthConfig {

		// OAuth for Facebook login
		public override string ClientId { get { return "***REMOVED***"; } }
		public override string ClientSecret { get { return ""; } }

		// The scopes for the particular API accessed, delimited by "+" symbols
		public override string Scope { get { return "email"; } }

		public override string AuthorizeUrl { get { return "https://m.facebook.com/dialog/oauth"; } }
		public override string AccessTokenUrl { get { return "https://m.facebook.com/dialog/oauth/token"; } }
		public override string UserInfoUrl { get { return ""; } }

		public override string RedirectUrl { get { return "https://www.facebook.com/connect/login_success.html"; } }

		// Unique name for the device keystore where the credentials are stored
		public override string KeystoreService { get { return App.AppName + "_FacebookOAuth"; } }
	}
}
