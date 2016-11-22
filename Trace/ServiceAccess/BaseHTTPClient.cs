using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Trace {
	public class BaseHTTPClient : HttpClient {

		public async Task<JObject> GetAsyncJSON(string uri) {
			var content = await GetStringAsync(uri);
			return await Task.Run(() => JObject.Parse(content));
		}

		public async Task<JObject> PostAsyncJSON(string uri, string data) {
			var content = new StringContent(data, Encoding.UTF8, "application/json");
			var response = await PostAsync(uri, content);

			response.EnsureSuccessStatusCode();

			string result = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JObject.Parse(result));
		}


		public async Task<JObject> GetAsyncFormURL(string uri, FormUrlEncodedContent query) {
			var response = await GetAsync(uri + query.ReadAsStringAsync().Result);

			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JObject.Parse(content));
		}

		public async Task<JObject> PostAsyncFormURL(string uri, FormUrlEncodedContent data) {
			var response = await PostAsync(uri, data);

			response.EnsureSuccessStatusCode();

			string content = await response.Content.ReadAsStringAsync();
			return await Task.Run(() => JObject.Parse(content));
		}
	}
}
