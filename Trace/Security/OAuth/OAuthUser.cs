using Newtonsoft.Json;

namespace Trace {
	/// <summary>
	/// This is the format of the json object that is sent by the OAuth API with user information.
	/// </summary>
	[JsonObject]
	public class OAuthUser {

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("email")]
		public string Email { get; set; }

		[JsonProperty("verified_email")]
		public bool VerifiedEmail { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("given_name")]
		public string GivenName { get; set; }

		[JsonProperty("family_name")]
		public string FamilyName { get; set; }

		[JsonProperty("link")]
		public string Link { get; set; }

		[JsonProperty("picture")]
		public string Picture { get; set; }

		[JsonProperty("gender")]
		public string Gender { get; set; }
	}
}