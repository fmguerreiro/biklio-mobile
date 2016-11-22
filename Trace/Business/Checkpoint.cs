using System;
using System.Collections.Generic;
using Plugin.Geolocator.Abstractions;

namespace Trace {
	public class Checkpoint {
		public string Name { get; set; }
		public string Address { get; set; }
		public DateTime OpeningHours { get; set; }
		public DateTime ClosingHours { get; set; }
		public string PhoneNumber { get; set; }
		public string WebsiteAddress { get; set; }
		public string FacebookAddress { get; set; }
		public string TwitterAddress { get; set; }
		public string BikeFacilities { get; set; }
		public string Description { get; set; }

		public List<Challenge> Challenges { get; set; }

		public Position Location { get; set; }

		public Checkpoint() {
		}
	}
}
