using System;
using System.Collections.Generic;
using Trace.Localization;

namespace Trace {


	public class MyTrajectoriesViewModel {

		public IList<TrajectoryViewModel> Trajectories { get; set; }

		public string Summary {
			get {
				if(Trajectories.Count == 0) {
					return Language.NoRoutesYet;
				}
				if(Trajectories.Count != 1)
					return Language.YouHave + " " + Trajectories.Count + " " + Language.routes_;
				else
					return Language.OneRoute;
			}
		}
	}


	public class TrajectoryViewModel {

		//public event PropertyChangedEventHandler PropertyChanged;

		public Trajectory Trajectory { get; set; }

		public TrajectoryViewModel(Trajectory trajectory) {
			Trajectory = trajectory;
		}

		public string MainActivityImage {
			get {
				var activityEnum = (ActivityType) Enum.Parse(typeof(ActivityType), Trajectory.MostCommonActivity);
				switch(activityEnum) {
					case ActivityType.Cycling: return "mytrajectories__cycle";
					case ActivityType.Running: return "mytrajectories__run";
					case ActivityType.Walking: return "mytrajectories__walk";
					case ActivityType.Automative: return "mytrajectories__vehicle";
					default: return "mytrajectories__stationary";
				}
			}
		}

		//public string TrackSentImage {
		//	get {
		//		if(Trajectory.WasTrackSent)
		//			return "mytrajectorieslist__green_check.png";
		//		return "mytrajectorieslist__cross_error.png";
		//	}
		//}


		/// <summary>
		/// Change image in MyTrajectoryListPage to reflect that the trajectory was sent.
		/// </summary>
		//public void TriggerPropertyChangedEvent() {
		//	if(PropertyChanged != null) {
		//		PropertyChanged(this, new PropertyChangedEventArgs("ImageSource"));
		//	}
		//}

		public string DisplayTime {
			get {
				string start = TimeUtil.SecondsToFullDate(Trajectory.StartTime);
				string end = TimeUtil.SecondsToHHMM(Trajectory.EndTime);
				return start + Language.To + end;
			}
		}

		public string DisplayDuration {
			get { return TimeUtil.SecondsToHHMMSS(Trajectory.ElapsedTime()); }
		}
	}
}

