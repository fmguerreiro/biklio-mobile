using System;
using System.Threading;
using System.Threading.Tasks;

namespace Trace {

	internal delegate void TimerCallback(object state);

	/// <summary>
	/// A simple timer implementation used by the RewardEligibilityManager.
	/// This is needed because there is no suitable available timer implementation for Xamarin.Forms as of time of writing.
	/// </summary>
	sealed class Timer : CancellationTokenSource, IDisposable {

		internal Timer(TimerCallback callback, object state, int dueTime) {
			Task.Delay(dueTime, Token).ContinueWith((t, s) => {
				var tuple = (Tuple<TimerCallback, object>) s;
				tuple.Item1(tuple.Item2);
			}, Tuple.Create(callback, state), CancellationToken.None,
											  TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.OnlyOnRanToCompletion,
											  TaskScheduler.Default);
		}

		public new void Dispose() { Cancel(); }
	}
}