using System;
namespace Trace {
	public class ClaimRewardModel {

		public Challenge Challenge { get; set; }
		public string ClaimedAt { get { return TimeUtil.SecondsToHHMMSS(Challenge.ClaimedAt); } }
		public string EarnedAt { get { return TimeUtil.SecondsToHHMMSS(Challenge.CompletedAt); } }
	}
}
