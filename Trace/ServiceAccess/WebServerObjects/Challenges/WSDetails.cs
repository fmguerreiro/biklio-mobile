namespace Trace {

	public class WSDetails {

		public string description { get; set; }

		public int dayOff { get; set; }

		public string openTime { get; set; }

		public string closeTime { get; set; }

		public string photoURL { get; set; }

		public WSType type { get; set; }

		public WSFacilities[] facilities { get; set; }
	}
}
