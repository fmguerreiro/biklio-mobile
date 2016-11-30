using System.Collections.Generic;
using Newtonsoft.Json;
using SQLite;
using Xamarin.Forms.Maps;

namespace Trace {

	public class Trajectory : UserItemBase {

		public long StartTime { get; set; }
		public long EndTime { get; set; }
		public float AvgSpeed { get; set; }
		public float MaxSpeed { get; set; }
		public long TotalDistanceMeters { get; set; }
		public string MostCommonActivity { get; set; }

		public long WSId { get; set; }

		IEnumerable<Position> points;
		[Ignore]
		public IEnumerable<Position> Points {
			get {
				return points ?? JsonConvert.DeserializeObject<IEnumerable<Position>>(PointsJSON);
			}
			set { points = value; }
		}

		// Used for serializing the points to store in SQLite.
		public string PointsJSON { get; set; }

		public override string ToString() {
			return string.Format("[Trajectory Id->{0} UserId->{1} StartTime->{2} AvgSpeed->{3} TotalDistance->{4} MostCommonActivity->{5}]",
								 Id, UserId, StartTime, AvgSpeed, TotalDistanceMeters, MostCommonActivity);
		}
	}

	class TrajectoryVM {
		public List<Trajectory> Trajectories { get; set; }
		public string Summary {
			get {
				if(Trajectories.Count == 0) {
					return "No routes yet.";
				}
				if(Trajectories.Count != 1)
					return "You have " + Trajectories.Count + " routes.";
				else
					return "You have 1 route.";
			}
		}
	}
}