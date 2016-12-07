using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator.Abstractions;

namespace Trace {

	public class WebServerClient : HTTPClientBase {

		/// <summary>
		/// Defers the constructor back to BaseHTTPClient to call HttpClient with handler optimized for each platform.
		/// </summary>
		public WebServerClient() : base() {
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.Instance.AuthToken);
		}


		/// <summary>
		/// Registers a new Trace User. 
		/// Request and response is in JSON format.
		/// </summary>
		/// <returns>True or False, depending on the result of the operation.</returns> 
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		/// <param name="email">Email.</param>
		public async Task<WSResult> Register(string username, string password, string email) {
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
			JObject output = await PostAsyncJSON(WebServerConstants.REGISTER_ENDPOINT, request);
			//Debug.WriteLine(result.Result.ToString());
			var result = output.ToObject<WSResult>();
			return result;
		}


		/// <summary>
		/// Login using the user/password credentials.
		/// Message request content is of type: 'application/x-www-form-urlencoded'
		/// WS response is in JSON format.
		/// </summary>
		/// <returns><c>string</c>, an authentication token, or an error if the authentication failed.</returns>
		/// <param name="username">Username.</param>
		/// <param name="password">Password.</param>
		public async Task<WSResult> LoginWithCredentials(string username, string password) {
			var request = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password)
			});
			var output = await PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			Debug.WriteLine("NativeLogin(): " + output);
			return output.ToObject<WSResult>();
		}


		/// <summary>
		/// Login using the user's authentication token.
		/// Message request content is of type: 'application/x-www-form-urlencoded'
		/// WS response is in JSON format.
		/// </summary>
		/// <returns><c>string</c>, an authentication token, or an error if the authentication failed.</returns>
		/// <param name="authToken">Authentication token.</param>
		public async Task<WSResult> LoginWithToken(string authToken) {
			var request = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("token", authToken)
			});

			var output = await PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			var result = output.ToObject<WSResult>();
			User.Instance.AuthToken = result.token;

			return result;
		}


		/// <summary>
		/// Fetches the challenges from the Webserver in a defined radius from the given position.
		/// Sends the device's DB version so the WS knows to send only the data that changed.
		/// </summary>
		/// <returns>The challenges.</returns>
		/// <param name="position">Position.</param>
		/// <param name="radiusInKM">Radius in KM.</param>
		/// <param name="version">Version.</param>
		public async Task<WSResult> FetchChallenges(Position position, int radiusInKM, long version) {
			var query = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("latitude", position.Latitude.ToString()),
				new KeyValuePair<string, string>("longitude", position.Longitude.ToString()),
				new KeyValuePair<string, string>("radius", radiusInKM.ToString()),
				new KeyValuePair<string, string>("version", version.ToString())
			});
			//Debug.WriteLine(query.ReadAsStringAsync().Result);
			JObject output = await GetAsyncFormURL(WebServerConstants.FETCH_CHALLENGES_ENDPOINT, query);
			//Debug.WriteLine(output.Result.ToString());
			WSResult result = output.ToObject<WSResult>();
			User.Instance.AuthToken = result.token;

			return result;
		}


		/// <summary>
		/// Sends the trajectory to the Web Server.
		/// It is divided into two POST requests, first we need to send a summary of the trajectory, which returns a token.
		/// If this is successful we then send the trajectory.
		/// Both request and result formats are in JSON.
		/// </summary>
		/// <param name="trajectory">Trajectory.</param>
		public async Task SendTrajectory(Trajectory trajectory) {
			var trackSummary = new WSTrackSummary {
				session = "",
				startedAt = trajectory.StartTime * 1000, // this is in ms.
				endedAt = trajectory.EndTime * 1000,
				elapsedTime = (int) trajectory.ElapsedTime(),
				elapsedDistance = trajectory.TotalDistanceMeters,
				avgSpeed = trajectory.AvgSpeed,
				topSpeed = trajectory.MaxSpeed,
				points = trajectory.Points.Count(),
				modality = trajectory.MostCommonActivity.ToAndroidInt()
			};

			string request = null;
			JObject output = null;
			WSResult trackSummaryResult = null;
			if(trajectory.TrackSession == null) {
				request = JsonConvert.SerializeObject(trackSummary, Formatting.None);
				output = await PostAsyncJSON(WebServerConstants.SUBMIT_TRAJECTORY_SUMMARY, request);
				//Debug.WriteLine("SendTrackSummary(): " + output);
				trackSummaryResult = output.ToObject<WSResult>();
				if(!trackSummaryResult.success)
					return;
				trajectory.TrackSession = trackSummaryResult.payload.session;
				SQLiteDB.Instance.SaveItem<Trajectory>(trajectory);
			}

			if(!trajectory.WasTrackSent) {
				// Send Trajectory.
				var track = WSTrajectory.ToWSPoints(trajectory.Points);
				request = JsonConvert.SerializeObject(track, Formatting.None);
				//Debug.WriteLine("SendTrack() request path: " + WebServerConstants.SUBMIT_TRAJECTORY + trajectory.trackSession);
				//Debug.WriteLine("SendTrack() request: " + request);
				output = await PostAsyncJSON(WebServerConstants.SUBMIT_TRAJECTORY + trajectory.TrackSession, request);
				//Debug.WriteLine("SendTrack() response: " + output);
				if(!output.ToObject<WSResult>().success)
					return;
				trajectory.WasTrackSent = true;
				SQLiteDB.Instance.SaveItem<Trajectory>(trajectory);
			}
		}
	}
}
