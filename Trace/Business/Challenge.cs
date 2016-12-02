using System.Collections.Generic;
using SQLite;

namespace Trace {

	public class Challenge : UserItemBase {

		public string Reward { get; set; }
		public string CheckpointName { get; set; }
		public string Condition { get; set; }

		[Ignore]
		public Checkpoint ThisCheckpoint { get; set; }
		public long CheckpointId { get; set; }

		// These properties are used at runtime to display information when listed in the ChallengesPage.
		[Ignore]
		public string Description { get { return Reward + " at " + CheckpointName; } }
		[Ignore]
		public string Image { get { return ThisCheckpoint.LogoURL ?? "default_shop.png"; } }

		public override string ToString() {
			return string.Format("[Challenge Id->{0} UserId->{1} CheckpointId->{2} Reward->{3} Checkpoint->{4} Condition->{5}]",
								 Id, UserId, CheckpointId, Reward, CheckpointName, Condition);
		}
	}

	class ChallengeVM {
		public List<Challenge> Challenges { get; set; }
		public string Summary {
			get {
				if(Challenges.Count == 0) {
					return "No challenges found.";
				}
				if(Challenges.Count != 1)
					return "There are " + Challenges.Count + " challenges near you.";
				else
					return "There is 1 challenge near you.";
			}
		}
	}
}
