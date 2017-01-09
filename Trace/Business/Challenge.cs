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
		public int NeededCyclingDistance { get; set; }

		public long CreatedAt { get; set; }
		public long ExpiresAt { get; set; }

		[Ignore]
		public bool IsComplete { get; set; }
		public long CompletedAt { get; set; }

		public string CheckpointName { get; set; }
		[Ignore]
		public Checkpoint ThisCheckpoint { get; set; }
		public long CheckpointId { get; set; }

		// These properties are used to display information when listed in the ChallengesPage.
		[Ignore]
		public string Description { get { return Reward + " " + Language.At + " " + CheckpointName; } }
		[Ignore]
		public string Image { get { return ThisCheckpoint?.LogoURL ?? "images/challenge_list/default_shop.png"; } }
		[Ignore]
		public string Condition {
			get {
				if(NeededCyclingDistance == 0)
					return Language.CycleToShop;
				else return string.Format(Language.BikeCondition, NeededCyclingDistance);
			}
		}

		public override string ToString() {
			return string.Format("[Challenge GId->{0} UserId->{1} CheckpointId->{2} Reward->{3} Checkpoint->{4} Distance->{5}]",
								 GId, UserId, CheckpointId, Reward, CheckpointName, NeededCyclingDistance);
		}
	}

	/// <summary>
	/// The Challenge and Reward Visual Models are used to bind 
	/// a list of challenges for display in the Challenge Page and Reward Page.
	/// </summary>
	class ChallengeVM {
		public IList<Challenge> Challenges { get; set; }
		public string Summary {
			get {
				int count = Challenges.Count;
				if(count == 0) {
					return Language.NoChallengesFound;
				}
				if(count != 1)
					return Language.ThereAre + " " + count + " " + Language.ChallengesNear;
				else
					return Language.OneChallengeFound;
			}
		}
	}

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
