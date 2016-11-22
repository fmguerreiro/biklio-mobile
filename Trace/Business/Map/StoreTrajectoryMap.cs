using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace Trace {
	public class StoreTrajectoryMap : Map {

		public List<Position> RouteCoordinates { get; set; }

		public StoreTrajectoryMap() {
			RouteCoordinates = new List<Position>();
		}
	}
}
