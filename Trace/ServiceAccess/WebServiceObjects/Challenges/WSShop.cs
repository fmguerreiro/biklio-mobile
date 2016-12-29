namespace Trace {

	public class WSShop {

		public long id { get; set; }

		public long ownerId { get; set; }

		public string name { get; set; }

		public double latitude { get; set; }

		public double longitude { get; set; }

		public string logoURL { get; set; }

		public string mapURL { get; set; }

		public WSDetails details { get; set; }

		public WSContacts contacts { get; set; }

		public WSFacilities[] facilities { get; set; }
	}
}
