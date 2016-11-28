using System.Collections.Generic;
using SQLite;

namespace Trace {

	/// <summary>
	/// The User class stores the user's information.
	/// Implements the Singleton pattern.
	/// </summary>
	public class User : DatabaseEntityBase {

		private const int DEFAULT_RADIUS = 10000;

		private static User instance;
		public static User Instance {
			get {
				if(instance == null) { instance = new User(); }
				return instance;
			}
		}

		public string Username { get; set; }
		public string Email { get; set; }
		public string AuthToken { get; set; }
		public int SearchRadiusInKM { get; set; } = DEFAULT_RADIUS;

		// The webserver stores several checkpoints of the challenge and store data.
		// This value is used to tell the webserver whats the most recent checkpoint version the device has.
		public long WsSyncVersion { get; set; }

		//public List<Trajectory> trajectories;
		//[OneToMany(CascadeOperations = CascadeOperation.All)]
		//public List<Trajectory> Trajectories {
		//	get {
		//		if(trajectories == null) { trajectories = new List<Trajectory>(); }
		//		return trajectories;
		//	}
		//	set { trajectories = value; }
		//}

		[Ignore]
		public List<Challenge> Challenges { get; set; }

		//public List<Checkpoint> checkpoints;
		//public List<Checkpoint> Checkpoints {
		//	get {
		//		if(checkpoints == null) { checkpoints = new List<Checkpoint>(); }
		//		return checkpoints;
		//	}
		//	set { checkpoints = value; }
		//}

		public string toString() {
			string res = "";
			res += "Username: " + Username + "\n";
			res += "Email: " + Email + "\n";
			res += "Token: " + AuthToken + "\n";
			res += "SearchRadius: " + SearchRadiusInKM + "\n";
			res += "Challenges: " + "\n";
			foreach(Challenge challenge in Challenges) res += challenge.toString() + "\n";
			res += "syncVersion: " + WsSyncVersion;
			return res;
		}
	}
}
