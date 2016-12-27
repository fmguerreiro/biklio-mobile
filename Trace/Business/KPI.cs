using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using SQLite;

namespace Trace {
	/// <summary>
	/// A set of key performance indicators that are collected from the user 
	/// in a given period, and sent to the Web Server for statistical analysis.
	/// </summary>
	public class KPI : UserItemBase {

		// The KPI stores information for a specific period. In this case, 1 day (in seconds).
		public const long KPI_PERIOD = 24 * 60 * 60;

		// Each KPI collects for a period of 24h starting at (00:00:00)
		private long date;
		public long Date {
			get {
				return date;
			}
			set { date = TimeUtil.RoundDownDay(value); }
		}

		WSKPIs wsFormatKPI;
		[Ignore]
		public WSKPIs WSFormatKPI {
			get {
				if(wsFormatKPI == null && !string.IsNullOrEmpty(SerializedKPI)) {
					wsFormatKPI = JsonConvert.DeserializeObject<WSKPIs>(SerializedKPI);
				}
				if(wsFormatKPI == null) { wsFormatKPI = new WSKPIs(); }
				return wsFormatKPI;
			}
			set { wsFormatKPI = value; }
		}

		public string SerializedKPI { get; set; }


		/// <summary>
		/// Serializes the KPI and stores it in SQLite.
		/// Called when OnSleep() happens or when a more recent KPI is created. 
		/// When a user switches to another the app, store information before it is lost. 
		/// Apps have at least 5 seconds to run before they are removed for memory.
		/// </summary>
		public void StoreKPI() {
			SerializedKPI = JsonConvert.SerializeObject(WSFormatKPI);
			SQLiteDB.Instance.SaveItem(this);
		}


		public bool IsKPIExpired() {
			Debug.WriteLine("IsKPIExpired(): \nCurrent->" + TimeUtil.CurrentEpochTimeSeconds() + " \nLimit--->" + (Date + KPI_PERIOD));
			return TimeUtil.CurrentEpochTimeSeconds() > Date + KPI_PERIOD;
		}


		/// <summary>
		/// Added when a new trajectory is created, 
		/// i.e., MapPage.xaml.cs on stop button pressed.
		/// </summary>
		/// <param name="date">Date.</param>
		/// <param name="walkingDuration">Walking duration.</param>
		/// <param name="runningDuration">Running duration.</param>
		/// <param name="cyclingDuration">Cycling duration.</param>
		/// <param name="vehicleDuration">Vehicle duration.</param>
		public void AddActivityEvent(long date, long walkingDuration, long runningDuration, long cyclingDuration, long vehicleDuration) {
			var list = WSFormatKPI.modalities;
			if(list == null)
				list = new List<WSModalitiesDuration>();

			list.Add(new WSModalitiesDuration {
				date = date,
				walking = walkingDuration,
				running = runningDuration,
				cycling = cyclingDuration,
				vehicular = vehicleDuration
			});
		}

		/// <summary>
		/// Added when a cycling event is recorded, 
		/// i.e., RewardEligibilityManager during CyclingIneligible and CyclingEligible states.
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="end">End.</param>
		public void AddCyclingEvent(long start, long end) {
			var list = WSFormatKPI.cycling;
			if(list == null)
				list = new List<WSCyclingEvent>();

			list.Add(new WSCyclingEvent { start = start, end = end });
		}

		/// <summary>
		/// Added when the user logins in, either offline or not, 
		/// i.e., SignInPage.xaml.cs.
		/// </summary>
		/// <param name="loginTime">Login time.</param>
		public void AddLoginEvent(long loginTime) {
			var list = WSFormatKPI.logins;
			if(list == null)
				list = new List<WSLoginEvent>();

			list.Add(new WSLoginEvent { loginAt = loginTime });
		}

		// TODO
		/// <summary>
		/// Added when the user claims a reward ...
		/// </summary>
		/// <param name="challengeId">Challenge identifier.</param>
		/// <param name="claimedAt">Claimed at.</param>
		public void AddClaimedRewardEvent(long challengeId, long claimedAt) {
			var list = WSFormatKPI.claimedRewards;
			if(list == null)
				list = new List<WSClaimedRewardEvent>();

			list.Add(new WSClaimedRewardEvent { challengeId = challengeId, claimedAt = claimedAt });
		}

		// TODO claimedAt
		/// <summary>
		/// Added when the user completes a challenge with a 'distance' condition, 
		/// i.e., Challenges.xaml.cs on challenges update.
		/// </summary>
		/// <param name="challengeId">Challenge identifier.</param>
		/// <param name="completedAt">Completed at.</param>
		/// <param name="claimedAt">Claimed at.</param>
		public void AddChallengeConditionCompletedEvent(long challengeId, long completedAt, long claimedAt) {
			var list = WSFormatKPI.specialChallenges;
			if(list == null)
				list = new List<WSChallengeConditionCompletedEvent>();

			list.Add(new WSChallengeConditionCompletedEvent {
				challengeId = challengeId,
				completedAt = completedAt,
				claimedAt = claimedAt
			});
		}

		/// <summary>
		/// Added when the user enters a checkpoint after riding a bycicle, 
		/// i.e., RewardEligibilityManager.checkNearbyCheckpoints()
		/// </summary>
		/// <param name="checkInAt">Check in at.</param>
		/// <param name="checkpointId">Checkpoint identifier.</param>
		public void AddCheckInEvent(long checkInAt, long checkpointId) {
			var list = WSFormatKPI.checkIns;
			if(list == null)
				list = new List<WSCheckInEvent>();

			list.Add(new WSCheckInEvent { checkInAt = checkInAt, shopId = checkpointId });
		}
	}
}
