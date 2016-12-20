using System.Collections.Generic;
using SQLite;

namespace Trace {

	/// <summary>
	/// The User class stores the user's information.
	/// Implements the Singleton pattern.
	/// </summary>
	public class User : DatabaseEntityBase {

		static User instance;
		[Ignore]
		public static User Instance {
			get {
				if(instance == null) { instance = new User(); }
				return instance;
			}
			set { instance = value; }
		}

		public string Username { get; set; } = "";
		[Ignore]
		public string Password { get; set; } = "";
		public string Name { get; set; } = "";
		public string Email { get; set; } = "";
		public string PictureURL { get; set; } = "";

		private string authToken = "";
		public string AuthToken {
			get { return authToken; }
			set { if(!string.IsNullOrEmpty(value)) authToken = value; }
		}


		// The webserver stores several snapshots of the challenge and checkpoint data.
		// This value is used to tell the webserver the most recent version of the data in the device.
		public long WSSnapshotVersion { get; set; } = 0;

		// Used in calories calculations. Defaults are European averages.
		public int Age { get; set; } = 37;
		public SelectedGender Gender { get; set; } = SelectedGender.Male;
		public double Weight { get; set; } = 70.8;// kilograms
		public double Height { get; set; } = 1.78; // meters

		public int SearchRadius { get; set; } = 100; // kilometers

		public SelectedLanguage UserLanguage { get; set; } = SelectedLanguage.English;

		List<Trajectory> trajectories;
		[Ignore]
		public List<Trajectory> Trajectories {
			get { return trajectories ?? new List<Trajectory>(); }
			set { trajectories = value ?? new List<Trajectory>(); }
		}


		List<Challenge> challenges;
		[Ignore]
		public List<Challenge> Challenges {
			get { return challenges ?? new List<Challenge>(); }
			set {
				if(value != null) {
					challenges = value;
					// Don't forget to update the reference of each challenge to its checkpoint!
					foreach(Challenge challenge in challenges) {
						if(checkpoints.ContainsKey(challenge.CheckpointId)) {
							challenge.ThisCheckpoint = checkpoints[challenge.CheckpointId];
							challenge.ThisCheckpoint.Challenges.Add(challenge);
						}
					}
				}
				else challenges = new List<Challenge>();
			}
		}


		Dictionary<long, Checkpoint> checkpoints;
		[Ignore]
		public Dictionary<long, Checkpoint> Checkpoints {
			get { return checkpoints ?? new Dictionary<long, Checkpoint>(); }
			set { checkpoints = value ?? new Dictionary<long, Checkpoint>(); }
		}

		public override string ToString() {
			//string challengeString = string.Join("\n\t", Challenges) ?? "";
			//string checkpointString = string.Join("\n\t", Checkpoints) ?? "";
			return string.Format("[User Id->{0} Username->{1} Email->{2} AuthToken->{3} Radius->{4} SnapshotVersion->{5}]",
								 Id, Username, Email, AuthToken, SearchRadius, WSSnapshotVersion);
		}
	}
}
