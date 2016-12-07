namespace Trace {
	public abstract class OAuthConfig {

		public abstract string ClientId { get; }
		public abstract string ClientSecret { get; }

		public abstract string Scope { get; }

		public abstract string AuthorizeUrl { get; }
		public abstract string AccessTokenUrl { get; }
		public abstract string UserInfoUrl { get; }

		// The location the user will be redirected to after successfully authenticating
		public virtual string RedirectUrl { get { return "http://blank.org"; } }

		public abstract string KeystoreService { get; }
	}
}
