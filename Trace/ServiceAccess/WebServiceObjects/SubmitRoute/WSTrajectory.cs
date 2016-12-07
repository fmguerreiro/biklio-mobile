using System.Collections.Generic;
using System.Linq;

namespace Trace {

	public class WSTrajectory {

		public string session { get; set; }

		public WSPoint[] track { get; set; }


		public static WSPoint[] ToWSPoints(IEnumerable<TrajectoryPoint> trajectory) {
			var res = new WSPoint[trajectory.Count()];
			var i = 0;
			foreach(TrajectoryPoint p in trajectory) {
				res[i++] = new WSPoint {
					longitude = p.Longitude,
					latitude = p.Latitude,
					timestamp = p.Timestamp,
					attributes = new WSAttributes { activity = (int) p.Activity, confidence = 100 } // TODO confidence
				};
			}
			return res;
		}
	}
}