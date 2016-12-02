using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator.Abstractions;

namespace Trace {

	public class WebServerClient : HTTPClientBase {

		/// <summary>
		/// Defers the constructor back to BaseHTTPClient to call HttpClient with handler optimized for each platform.
		/// </summary>
		public WebServerClient() : base() { }

		/// <summary>
		/// Registers a new Trace User. 
		/// Request and response is in JSON format.
		/// </summary>
		/// <returns>True or False, depending on the result of the operation.</returns> 
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="email">Email.</param>
		public WSResult register(string username, string password, string email) {
			var user = new WSUser {
				name = "Filipe Guerreiro",
				username = username,
				password = password,
				confirm = password,
				email = email,
				phone = "966845129",
				address = "Tapada das Merces"
			};

			string request = JsonConvert.SerializeObject(user, Formatting.None);
			Task<JObject> result = PostAsyncJSON(WebServerConstants.REGISTER_ENDPOINT, request);
			//Debug.WriteLine(result.Result.ToString());
			return result.Result.ToObject<WSResult>();
		}

		/// <summary>
		/// Login using the user/password credentials.
		/// Message request content is of type: 'application/x-www-form-urlencoded'
		/// WS response is in JSON format.
		/// </summary>
		/// <returns><c>string</c>, an authentication token, or an error if the authentication failed.</returns>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		public WSResult loginWithCredentials(string username, string password) {
			var request = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password)
			});

			var output = PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			return output.Result.ToObject<WSResult>();
		}

		/// <summary>
		/// Login using the user's authentication token.
		/// Message request content is of type: 'application/x-www-form-urlencoded'
		/// WS response is in JSON format.
		/// </summary>
		/// <returns><c>string</c>, an authentication token, or an error if the authentication failed.</returns>
		/// <param name="authToken">Authentication token.</param>
		public WSResult loginWithToken(string authToken) {
			var request = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("token", authToken)
			});

			var output = PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			return output.Result.ToObject<WSResult>();
		}

		/// <summary>
		/// Fetches the challenges from the Webserver in a defined radius from the given position.
		/// Sends the device's DB version so the WS knows to send only the data that changed.
		/// </summary>
		/// <returns>The challenges.</returns>
		/// <param name="position">Position.</param>
		/// <param name="radiusInKM">Radius in KM.</param>
		/// <param name="version">Version.</param>
		public WSResult fetchChallenges(Position position, int radiusInKM, long version) {
			var query = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("latitude", position.Latitude.ToString()),
				new KeyValuePair<string, string>("longitude", position.Longitude.ToString()),
				new KeyValuePair<string, string>("radius", radiusInKM.ToString()),
				new KeyValuePair<string, string>("version", version.ToString())
			});
			Debug.WriteLine(query.ReadAsStringAsync().Result);
			Task<JObject> output = GetAsyncFormURL(WebServerConstants.FETCH_CHALLENGES_ENDPOINT, query);
			Debug.WriteLine(output.Result.ToString());
			return output.Result.ToObject<WSResult>();
		}
	}
}
