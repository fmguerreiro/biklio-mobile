using System.Collections.Generic;

namespace Trace {
	public class Challenge {
		public string Reward { get; set; }
		public string CheckpointName { get; set; }
		public string Condition { get; set; }
		public Checkpoint ThisCheckpoint { get; set; }
	}

	class ChallengeVM {
		public List<Challenge> Challenges { get; set; }
		public string Summary {
			get {
				if(Challenges.Count == 0) {
					return "Failed to find any challenges.";
				}
				if(Challenges.Count != 1)
					return " There are " + Challenges.Count + " challenges near you.";
				else
					return "There is 1 challenge near you.";
			}
		}
	}
}
