using System;
using Android.Content;

namespace Trace.Droid {
	/// <summary>
	/// Receives motion data intent messages from the system.
	/// </summary>
	public class ActivityDetectionBroadcastReceiver : BroadcastReceiver {
		public Action<Context, Intent> OnReceiveImpl { get; set; }

		public override void OnReceive(Context context, Intent intent) {
			//System.Diagnostics.Debug.WriteLine("------ MOTION ACTIVITY INTENT RECEIVED ------");
			OnReceiveImpl(context, intent);
		}
	}
}
