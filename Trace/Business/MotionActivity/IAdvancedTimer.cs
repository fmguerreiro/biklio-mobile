using System;

namespace Trace {
	public interface IAdvancedTimer {

		void initTimer(long interval, EventHandler callback, bool repeatable);
		void startTimer();
		void stopTimer();
		long getInterval();
		void setInterval(long interval);
		bool isTimerEnabled();
	}
}
