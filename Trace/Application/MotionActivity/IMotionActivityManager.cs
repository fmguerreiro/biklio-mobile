using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Trace {
	public interface IMotionActivityManager {
		IList<ActivityEvent> ActivityEvents { get; }
		int WalkingDuration { get; }
		int RunningDuration { get; }
		int CyclingDuration { get; }
		int DrivingDuration { get; }

		void InitMotionActivity();
		void StartMotionUpdates(Action<ActivityType> handler);
		void StopMotionUpdates();
		void Reset();
		Task QueryHistoricalData(DateTime start, DateTime end);
		ActivityType GetMostCommonActivity();
	}
}
