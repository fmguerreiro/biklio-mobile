namespace Trace {

	public class WSPayload {

		public int version { get; set; }

		public WSShop[] shops { get; set; }

		public WSChallenge[] challenges { get; set; }
	}
}