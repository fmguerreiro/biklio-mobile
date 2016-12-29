namespace Trace {

	public class WSPayload {

		// This value is given by the webserver when sending a trajectory summary.
		public string session { get; set; }

		// The current version of the data stored in the WS.
		// Used for informing how out of sync the device info and WS info is.
		public long version { get; set; }

		public WSShop[] shops { get; set; }

		public WSChallenge[] challenges { get; set; }

		// Ids of checkpoints/shops that are no longer available.
		public long[] canceled { get; set; }

		// Ids of challenges that have expired.
		public long[] canceledChallenges { get; set; }

		// Session token used for sending route.
		public string token { get; set; }

		// Information passed by OAuth providers to the WS, which passes it to the user after sending the token.
		public string id;
		public string name;
		public string email;
		public string picture;

		// Campaign information
		public string website;
		public long start;
		public long end;
		public string description;
		public string image;

	}
}