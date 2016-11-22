using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Trace {

	public class WebServerClient : BaseHTTPClient {

		/// <summary>
		/// Registers a new Trace User. 
		/// Request and response is in JSON format.
		/// </summary>
		/// <returns>True or False, depending on the result of the operation.</returns> 
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="email">Email.</param>
		public WSSuccess register(string username, string password, string email) {
			var user = new WSUser {
				name = "Filipe Guerreiro",
				username = username,
				password = password,
				confirm = password,
				email = email,
				phone = "966845129",
				address = "Rua Maria Vileira, n1, 2E"
			};

			string request = JsonConvert.SerializeObject(user, Formatting.None);
			Task<JObject> result = PostAsyncJSON(WebServerConstants.REGISTER_ENDPOINT, request);
			//Debug.WriteLine(result.Result.ToString());
			return result.Result.ToObject<WSSuccess>();
		}

		/// <summary>
		/// Login using the user/password credentials.
		/// Message request content is of type: 'application/x-www-form-urlencoded'
		/// WS response is in JSON format.
		/// </summary>
		/// <returns><c>string</c>, an authentication token, or an error if the authentication failed.</returns>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		public WSSuccess loginWithCredentials(string username, string password) {
			var request = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password)
			});

			var output = PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			return output.Result.ToObject<WSSuccess>();
		}

		/// <summary>
		/// Login using the user's authentication token.
		/// Message request content is of type: 'application/x-www-form-urlencoded'
		/// WS response is in JSON format.
		/// </summary>
		/// <returns><c>string</c>, an authentication token, or an error if the authentication failed.</returns>
		/// <param name="authToken">Authentication token.</param>
		public WSSuccess loginWithToken(string authToken) {
			var request = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("token", authToken)
			});

			var output = PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			return output.Result.ToObject<WSSuccess>();
		}

	}
}
