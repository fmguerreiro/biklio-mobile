﻿using System.Collections.Generic;
using Plugin.Geolocator.Abstractions;
using SQLite;

namespace Trace {
	public class Checkpoint : UserItemBase {

		public string Name { get; set; }
		public string Address { get; set; }
		public string AvailableHours { get; set; }
		public string PhoneNumber { get; set; }
		public string WebsiteAddress { get; set; }
		public string FacebookAddress { get; set; }
		public string TwitterAddress { get; set; }
		public string BikeFacilities { get; set; }
		public string Description { get; set; }

		[Ignore]
		public List<Challenge> Challenges { get; set; }

		// 'double' instead of 'Position' because SQLite only supports basic types. 
		public double Longitude { get; set; }
		public double Latitude { get; set; }
	}
}
