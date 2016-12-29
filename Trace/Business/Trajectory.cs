using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Plugin.Geolocator.Abstractions;
using SQLite;
using Trace.Localization;

namespace Trace {

	public class Trajectory : UserItemBase {

		// Start and end time of the trajectory in milliseconds since Unix epoch.
		// [Indexed]
		public long StartTime { get; set; }
		public long EndTime { get; set; }

		// Values obtained from GPS in m/s.
		public float AvgSpeed { get; set; }
		public float MaxSpeed { get; set; }

		public long TotalDistanceMeters { get; set; }

		public string MostCommonActivity { get; set; }
		public int TimeSpentWalking { get; set; }
		public int TimeSpentRunning { get; set; }
		public int TimeSpentCycling { get; set; }
		public int TimeSpentDriving { get; set; }

		IEnumerable<TrajectoryPoint> points;
		[Ignore]
		public IEnumerable<TrajectoryPoint> Points {
			// Lazily serializes the points when they are requested (when showing the trajectory in the Trajectory Details page).
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

		// In milliseconds.
		public long ElapsedTime() {
			return EndTime - StartTime;
		}


		public int CalculateWalkingDistance() { return (int) ((TotalDistanceMeters * TimeSpentWalking) / ElapsedTime()); }
		public int CalculateRunningDistance() { return (int) ((TotalDistanceMeters * TimeSpentRunning) / ElapsedTime()); }
		public int CalculateCyclingDistance() { return (int) ((TotalDistanceMeters * TimeSpentCycling) / ElapsedTime()); }
		public int CalculateDrivingDistance() { return (int) ((TotalDistanceMeters * TimeSpentDriving) / ElapsedTime()); }


		// In kcal. Source: http://www.ideafit.com/fitness-library/calculating-caloric-expenditure-0
		public int CalculateCalories() {
			return CalculateWalkingCalories() + CalculateCyclingCalories() + CalculateRunningCalories() + CalculateDrivingCalories();
		}

		public int CalculateWalkingCalories() {
			var speed = 83.1494; // avg. walking speed (m/min)
			var grade = 0.1; // land slope - we assume a slight angle
			var formula = (0.1 * speed) + (1.8 * speed * grade) + 3.5; // (kg/m)
			var total = (formula * User.Instance.Weight / 1000) * 5 * (ElapsedTime() / 60);
			return (int) total;
		}

		public int CalculateRunningCalories() {
			var speed = 210.701; // avg. running speed (m/min)
			var grade = 0.1;
			var formula = (0.2 * speed) + (0.9 * speed * grade) + 3.5; // (ml/kg/m)
			var total = (formula * User.Instance.Weight / 1000) * 5 * (ElapsedTime() / 60);
			return (int) total;
		}

		public int CalculateCyclingCalories() {
			int workRate = 250;
			var formula = (1.8 * workRate) / User.Instance.Weight + 7;
			var total = (formula * User.Instance.Weight / 1000) * 5 * (ElapsedTime() / 60);
			return (int) total;
		}

		public int CalculateDrivingCalories() {
			var total = (3.5 * User.Instance.Weight / 1000) * 5 * (ElapsedTime() / 60);
			return (int) total;
		}

		public override string ToString() {
			return string.Format("[Trajectory Id->{0} UserId->{1} StartTime->{2} AvgSpeed->{3} TotalDistance->{4} MostCommonActivity->{5}]",
								 Id, UserId, StartTime, AvgSpeed, TotalDistanceMeters, MostCommonActivity);
		}
	}

	class TrajectoryVM {
		public IList<Trajectory> Trajectories { get; set; }
		public string Summary {
			get {
				if(Trajectories.Count == 0) {
					return Language.NoRoutesYet;
				}
				if(Trajectories.Count != 1)
					return Language.YouHave + " " + Trajectories.Count + " " + Language.Routes;
				else
					return Language.OneRoute;
			}
		}
	}
}