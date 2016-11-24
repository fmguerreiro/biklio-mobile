namespace Trace {

	public class WSResult {

		// Indicates whether the operation was successful or not.
		public bool success { get; set; }

		// Authentication token.
		public string token { get; set; }

		// Error message in case the operation is not successful.
		public string error { get; set; }

		// Generic message result -- used in fetching the list of challenges. 
		public WSPayload payload { get; set; }
	}
}
