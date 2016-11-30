using System.Collections.Generic;
using SQLite;

namespace Trace {

	public class Challenge : UserItemBase {

		public string Reward { get; set; }
		public string CheckpointName { get; set; }
		public string Condition { get; set; }

		[Ignore]
		public Checkpoint ThisCheckpoint { get; set; }
		public string Description { get { return Reward + " at " + CheckpointName; } }

		public override string ToString() {
			return string.Format("[Challenge Id->{0} UserId->{1} Reward->{2} Checkpoint->{3} Condition->{4}]",
								 Id, UserId, Reward, CheckpointName, Condition);
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
