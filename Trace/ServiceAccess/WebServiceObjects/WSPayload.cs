namespace Trace {

	public class WSPayload {

		public int version { get; set; }

		public WSShop[] shops { get; set; }

		public WSChallenge[] challenges { get; set; }

		// Ids of checkpoints/shops that are no longer available.
		public long[] canceled { get; set; }

		// Ids of challenges that have expired.
		public long[] canceledChallenges { get; set; }
	}
}