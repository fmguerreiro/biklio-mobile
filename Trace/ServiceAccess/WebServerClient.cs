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
		public WebServerClient() : base() { }


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
				name = "XamarinName", // TODO replace these with input from user in the Registration window
				username = username,
				password = password,
				confirm = password,
				email = email,
				phone = "966845129",
				address = "XamarinAddress"
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
			Debug.WriteLine("LoginWithCredentials() - request: " + request.ReadAsStringAsync().Result);
			var output = await PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			Debug.WriteLine("LoginWithCredentials() - response: " + output);
			return output.ToObject<WSResult>();
		}


		/// <summary>
		/// Login using the user's authentication token.
		/// Message request content is of type: 'application/x-www-form-urlencoded'.
		/// The request parameter is a JSON token with attributes: 'token' and 'type' (e.g., google or facebook).
		/// WS response is in JSON format.
		/// </summary>
		/// <returns><c>string</c>, an authentication token, or an error if the authentication failed.</returns>
		/// <param name="authToken">Authentication token.</param>
		public async Task<WSResult> LoginWithToken(string authToken) {
			var token = new WSFederatedToken { token = authToken, type = OAuthConfigurationManager.Type };
			var request = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("token", JsonConvert.SerializeObject(token))
			});
			Debug.WriteLine("LoginWithToken() - request: " + request.ReadAsStringAsync().Result);
			var output = await PostAsyncFormURL(WebServerConstants.LOGIN_ENDPOINT, request);
			Debug.WriteLine("LoginWithToken() - result: " + output);
			var result = output.ToObject<WSResult>();
			if(!string.IsNullOrWhiteSpace(result.token)) User.Instance.IDToken = result.token;

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
		public async Task<WSResult> FetchCheckpoints(Position position, int radiusInKM, long version) {
			var query = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("latitude", position.Latitude.ToString().Replace(',','.')),
				new KeyValuePair<string, string>("longitude", position.Longitude.ToString().Replace(',','.')),
				new KeyValuePair<string, string>("radius", radiusInKM.ToString()),
				new KeyValuePair<string, string>("version", version.ToString())
			});
			Debug.WriteLine("FetchChallenges() - request: " + query.ReadAsStringAsync().Result);
			JObject output = await GetAsyncFormURL(WebServerConstants.GET_CHALLENGES, query);
			Debug.WriteLine("FetchChallenges() - response: " + output);
			WSResult result = output.ToObject<WSResult>();
			if(!string.IsNullOrWhiteSpace(result.token)) User.Instance.IDToken = result.token;
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
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.Instance.SessionToken ?? User.Instance.IDToken);

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
				request = JsonConvert.SerializeObject(trackSummary, Formatting.Indented);
				Debug.WriteLine("SendTrackSummary() - request: " + request);
				output = await PostAsyncJSON(WebServerConstants.SUBMIT_TRAJECTORY_SUMMARY, request);
				Debug.WriteLine("SendTrackSummary(): " + WebServerConstants.SUBMIT_TRAJECTORY_SUMMARY + "\nresult: " + output);
				trackSummaryResult = output.ToObject<WSResult>();
				if(!trackSummaryResult.success)
					return;
				trajectory.TrackSession = trackSummaryResult.payload.session;
				SQLiteDB.Instance.SaveItem(trajectory);
			}

			if(!trajectory.WasTrackSent) {
				// Send Trajectory.
				var track = WSTrajectory.ToWSPoints(trajectory.Points);
				request = JsonConvert.SerializeObject(track, Formatting.Indented);
				Debug.WriteLine("SendTrack() request path: " + WebServerConstants.SUBMIT_TRAJECTORY + trajectory.TrackSession);
				Debug.WriteLine("SendTrack() request: " + request);
				output = await PostAsyncJSON(WebServerConstants.SUBMIT_TRAJECTORY + trajectory.TrackSession, request);
				Debug.WriteLine("SendTrack() response: " + output);
				if(!output.ToObject<WSResult>().success)
					return;
				trajectory.WasTrackSent = true;
				SQLiteDB.Instance.SaveItem(trajectory);
			}
		}


		/// <summary>
		/// Sends the available KPIs to the Web Server.
		/// Sent KPIs are then removed from the device.
		/// Both request and result formats are in JSON.
		/// </summary>
		/// <returns>The KPI.</returns>
		/// <param name="kpis">Kpis.</param>
		public async Task SendKPIs(IEnumerable<KPI> kpis) {
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.Instance.SessionToken ?? User.Instance.IDToken);

			foreach(KPI kpi in kpis) {
				string request = kpi.SerializedKPI;
				JObject output = null;
				WSResult result = null;
				Debug.WriteLine("SendKPIs() - request: " + request);
				output = await PostAsyncJSON(WebServerConstants.SUBMIT_KPI, request);
				Debug.WriteLine("SendKPIs(): " + WebServerConstants.SUBMIT_KPI + "\nresult: " + output);
				result = output.ToObject<WSResult>();
				// Delete sent KPIs.
				if(result.success) {
					SQLiteDB.Instance.DeleteItem<KPI>(kpi.Id);
				}
			}
		}


		/// <summary>
		/// Gets the nearest campaign from the user position.
		/// Message request content is of type: 'application/x-www-form-urlencoded'.
		/// Response if of type JSON in format ... TODO .
		/// </summary>
		/// <returns>The nearest campaign.</returns>
		public async Task<WSResult> GetNearestCampaign() {
			//DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.Instance.SessionToken ?? User.Instance.IDToken);
			var position = await GeoUtils.GetCurrentUserLocation();

			var query = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("latitude", position.Latitude.ToString().Replace(',','.')),
				new KeyValuePair<string, string>("longitude", position.Longitude.ToString().Replace(',','.')),
			});
			Debug.WriteLine("GetNearestCampaign() - request: " + query.ReadAsStringAsync().Result);

			JObject output = await GetAsyncFormURL(WebServerConstants.GET_CLOSEST_CAMPAIGN, query);
			Debug.WriteLine("GetNearestCampaign() - response: " + output);
			WSResult result = output.ToObject<WSResult>();
			return result;
		}


		/// <summary>
		/// Subscribes to the campaign with the specified "campaignId".
		/// Message request content is a POST of type: 'application/x-www-form-urlencoded'.
		/// Response if of type JSON.
		/// </summary>
		/// <returns>The campaign.</returns>
		/// <param name="campaignId">Campaign identifier.</param>
		public async Task<WSResult> SubscribeCampaign(long campaignId) {
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.Instance.SessionToken ?? User.Instance.IDToken);

			var query = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("campaign", campaignId.ToString())
			});
			Debug.WriteLine("SubscribeCampaign() - request: " + query.ReadAsStringAsync().Result);

			JObject output = await PostAsyncFormURL(WebServerConstants.SUBSCRIBE_CAMPAIGN, query);
			Debug.WriteLine("SubscribeCampaign() - response: " + output);
			WSResult result = output.ToObject<WSResult>();
			return result;
		}


		/// <summary>
		/// Unsubscribes from the campaign with the specified "campaignId".
		/// Message request content is a POST of type: 'application/x-www-form-urlencoded'.
		/// Response if of type JSON.
		/// </summary>
		/// <returns>The campaign.</returns>
		/// <param name="campaignId">Campaign identifier.</param>
		public async Task<WSResult> UnsubscribeCampaign(long campaignId) {
			DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", User.Instance.SessionToken ?? User.Instance.IDToken);

			var query = new FormUrlEncodedContent(new[] {
				new KeyValuePair<string, string>("campaign", campaignId.ToString())
			});
			Debug.WriteLine("UnsubscribeCampaign() - request: " + query.ReadAsStringAsync().Result);

			JObject output = await PostAsyncFormURL(WebServerConstants.UNSUBSCRIBE_CAMPAIGN, query);
			Debug.WriteLine("UnsubscribeCampaign() - response: " + output);
			WSResult result = output.ToObject<WSResult>();
			return result;
		}
	}
}
