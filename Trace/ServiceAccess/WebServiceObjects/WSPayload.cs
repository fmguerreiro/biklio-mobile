namespace Trace {

	public class WSPayload {

		// The current version of the data stored in the WS.
		// Used for informing how out of sync the device info and WS info is.
		public long version { get; set; }

		public WSShop[] shops { get; set; }

		public WSChallenge[] challenges { get; set; }

		// Ids of checkpoints/shops that are no longer available.
		public long[] canceled { get; set; }

		// Ids of challenges that have expired.
		public long[] canceledChallenges { get; set; }
	}
}