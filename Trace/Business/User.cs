using System.Collections.Generic;
using SQLite;

namespace Trace {

	/// <summary>
	/// The User class stores the user's information.
	/// Implements the Singleton pattern.
	/// </summary>
	public class User {

		private const int DEFAULT_RADIUS = 10000;

		private static User instance;

		public static User Instance {
			get {
				if(instance == null) { instance = new User(); }
				return instance;
			}
		}

		[PrimaryKey]
		public string Username { get; set; }
		public string Email { get; set; }
		public string AuthToken { get; set; }
		public int SearchRadiusInKM { get; set; } = DEFAULT_RADIUS;

		//public List<Trajectory> trajectories;
		//public List<Trajectory> Trajectories {
		//	get {
		//		if(trajectories == null) { trajectories = new List<Trajectory>(); }
		//		return trajectories;
		//	}
		//	set { trajectories = value; }
		//}

		//public List<Challenge> challenges;
		//public List<Challenge> Challenges {
		//	get {
		//		if(challenges == null) { challenges = new List<Challenge>(); }
		//		return challenges;
		//	}
		//	set { challenges = value; }
		//}

		//public List<Checkpoint> checkpoints;
		//public List<Checkpoint> Checkpoints {
		//	get {
		//		if(checkpoints == null) { checkpoints = new List<Checkpoint>(); }
		//		return checkpoints;
		//	}
		//	set { checkpoints = value; }
		//}
	}
}
