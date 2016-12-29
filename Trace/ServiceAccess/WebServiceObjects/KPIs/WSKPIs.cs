using System.Collections.Generic;

namespace Trace {
	/// <summary>
	/// The json format for the KPI collected by users.
	/// </summary>
	public class WSKPIs {

		public IList<WSModalitiesDuration> modalities;
		public IList<WSCyclingEvent> cycling;
		public IList<WSLoginEvent> logins;
		public IList<WSClaimedRewardEvent> claimedRewards;
		public IList<WSChallengeConditionCompletedEvent> specialChallenges;
		public IList<WSCheckInEvent> checkIns;
	}
}
