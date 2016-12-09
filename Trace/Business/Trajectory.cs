using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugin.Geolocator.Abstractions;
using SQLite;

namespace Trace {

	public class Trajectory : UserItemBase {

		// Start and end time of the trajectory in milliseconds since Unix epoch.
		public long StartTime { get; set; }
		public long EndTime { get; set; }

		// Values obtained from GPS in m/s.
		public float AvgSpeed { get; set; }
		public float MaxSpeed { get; set; }

		public long TotalDistanceMeters { get; set; }

		public string MostCommonActivity { get; set; }

		IEnumerable<TrajectoryPoint> points;
		[Ignore]
		public IEnumerable<TrajectoryPoint> Points {
			get {
				return points ?? JsonConvert.DeserializeObject<IEnumerable<TrajectoryPoint>>(PointsJSON);
			}
			set { points = value; }
		}
		public List<Position> ToPosition() {
			var res = new List<Position>();
			foreach(TrajectoryPoint p in Points)
				res.Add(p.ToPosition());
			return res;
		}

		// Used for serializing the points to store in SQLite.
		public string PointsJSON { get; set; }

		// Flags used to check if each part of the trajectory is already stored in the WebServer
		// and do not need to be retransmitted.
		public string TrackSession { get; set; }
		public bool WasTrackSent { get; set; } = false;


		// Used in the Trajectory Details page.
		[Ignore]
		public string DisplayTime {
			get {
				string format = @"dd\/MM\/yyyy HH:mm";
				string start = StartTime.EpochSecondsToDatetime().ToString(format);
				string end = EndTime.EpochSecondsToDatetime().ToString(format);
				return start + " to " + end;
			}
		}
		[Ignore]
		public string DisplayDuration {
			get {
				string format = @"hh\:mm\:ss";
				return TimeSpan.FromSeconds(ElapsedTime()).ToString(format);
			}
		}

		public long ElapsedTime() {
			return EndTime - StartTime;
		}


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