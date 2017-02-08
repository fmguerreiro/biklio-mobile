using System;
using System.Collections.Generic;
using SQLite;
using Trace.Localization;

namespace Trace {

	/// <summary>
	/// A Challenge is issued by a particular Checkpoint.
	/// They have a 'reward', e.g., 10% discount, which is unlocked upon completing a given 'distance', e.g, cycle 2000 m.
	/// </summary>
	public class Challenge : UserItemBase {

		public string Reward { get; set; }
		public int NeededMetersCycling { get; set; }

		public long CreatedAt { get; set; }
		public long ExpiresAt { get; set; }

		[Ignore]
		public bool IsComplete { get; set; }
		public long CompletedAt { get; set; }

		// Indicator showing if the user has completed the challenge. Repeatable challenges can never be fully claimed, i.e., finished.
		private bool isClaimed;
		public bool IsClaimed {
			get { return isClaimed; }
			set {
				if(!IsRepeatable)
					isClaimed = value;
				ClaimedAt = TimeUtil.CurrentEpochTimeSeconds();
			}
		}
		public long ClaimedAt { get; set; }
		public bool IsRepeatable { get; set; }

		// This weak reference prevents a circular loop between checkpoint <-> challenge, allowing the GC to collect them.
		WeakReference<Checkpoint> thisCheckpoint;
		[Ignore]
		public Checkpoint ThisCheckpoint {
			get {
				if(thisCheckpoint == null)
					return null;

				Checkpoint res = null;
				if(thisCheckpoint.TryGetTarget(out res))
					return res;
				else {
					return null;
				}
			}

			set {
				if(thisCheckpoint != null)
					thisCheckpoint.SetTarget(value);
				else thisCheckpoint = new WeakReference<Checkpoint>(value);
			}
		}
		public long CheckpointId { get; set; }


		// These properties are used to display information when listed in the CheckpointsListPage.
		public string Condition {
			get {
				if(NeededMetersCycling == 0)
					return Language.CycleToShop;
				else return string.Format(Language.BikeCondition, NeededMetersCycling);
			}
		}

		public override string ToString() {
			return string.Format("[Challenge GId->{0} UserId->{1} CheckpointId->{2} Reward->{3} Checkpoint->{4} Distance->{5}]",
								 GId, UserId, ThisCheckpoint.GId, Reward, ThisCheckpoint.Name, NeededMetersCycling);
		}
	}

	/// <summary>
	/// The Reward Data Model is used to bind 
	/// a list of challenges for display in the Reward Page.
	/// </summary>
	class RewardVM {
		public IList<Challenge> Rewards { get; set; }
		public string Summary {
			get {
				int count = Rewards.Count;
				if(count == 0) {
					return Language.NoRewardsYet;
				}
				if(count != 1)
					return Language.YouHave + " " + count + " " + Language.RewardsEarned;
				else
					return Language.OneRewardEarned;
			}
		}
	}
}
