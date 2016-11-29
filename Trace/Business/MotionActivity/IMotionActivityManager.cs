using System;
using System.Collections;
using System.Threading.Tasks;

namespace Trace {
	public abstract class IMotionActivityManager {
		protected IList ActivityEvents;
		protected long WalkingDuration;
		protected long RunningDuration;
		protected long CyclingDuration;
		protected long AutomativeDuration;

		public abstract void InitMotionActivity();
		public abstract void StartMotionUpdates(Action<ActivityType> handler);
		public abstract void StopMotionUpdates();
		public abstract Task QueryHistoricalData(DateTime start, DateTime end);
		public abstract ActivityType GetMostCommonActivity();
	}
}
