namespace Trace
{

    public class GoogleOAuthConfig : OAuthConfig
    {

        public override string Type { get { return "google"; } }

        // OAuth for Google login
        public override string ClientId { get { return "**REMOVED**"; } }
        public override string ClientSecret { get { return "**REMOVED**"; } }


        // These values do not need changing
        public string[] Scopes
        {
            get
            {
                return
                    new string[] { "https://www.googleapis.com/auth/userinfo.email", "https://www.googleapis.com/auth/userinfo.profile" };
            }
        }
        public override string Scope
        {
            get
            {
                return "https://www.googleapis.com/auth/userinfo.email+" +
                       "https://www.googleapis.com/auth/userinfo.profile";
            }
        }
        public override string AuthorizeUrl { get { return "https://accounts.google.com/o/oauth2/auth"; } }
        public override string AccessTokenUrl { get { return "https://accounts.google.com/o/oauth2/token"; } }
        public override string UserInfoUrl { get { return "https://www.googleapis.com/oauth2/v2/userinfo"; } }

        // Unique name for the device keystore where the credentials are stored
        public override string KeystoreService { get { return App.AppName + "_GoogleOAuth"; } }
    }
}