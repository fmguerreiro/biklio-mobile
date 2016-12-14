namespace Trace {

	/// <summary>
	/// JSON representation of the oauth token to be sent to the WebServer after receiving it from a third-party.
	/// Is composed of the token string and a type parameter which indicates the provider, e.g., facebook or google.
	/// </summary>
	public class WSFederatedToken {

		public string token { get; set; }
		public string type { get; set; }
	}
}
