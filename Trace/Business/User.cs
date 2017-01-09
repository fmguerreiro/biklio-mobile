using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

		// Token from the third-party OAuth provider that uniquely identifies the user.
		private string idToken = "";
		public string IDToken {
			get { return idToken; }
			set { if(!string.IsNullOrEmpty(value)) idToken = value; }
		}


		// Token received from the WebServer upon login to perform privileged operations.
		[Ignore]
		public string SessionToken { get; set; }


		// Position where the user last obtained challenges,
		// when user gets too far from here, reset WSSnapshotVersion
		public double PrevLongitude = 0.0;
		public double PrevLatitude = 0.0;


		// The webserver stores several snapshots of the challenge and checkpoint data.
		// This value is used to tell the webserver the most recent version of the data in the device.
		public long WSSnapshotVersion { get; set; } = 0;


		// Used in calories calculations. Defaults are European averages.
		public int Age { get; set; } = 37;
		public SelectedGender Gender { get; set; } = SelectedGender.Male;
		public double Weight { get; set; } = 70.8;// kilograms
		public double Height { get; set; } = 1.78; // meters


		public int SearchRadius { get; set; } = 100; // kilometers


		public string BackgroundIneligibleSoundSetting { get; set; } = "bike_bell.wav";
		public string BycicleEligibleSoundSetting { get; set; } = "bike_no_pedaling.aiff";
		public string CongratulatorySoundSetting { get; set; } = "bike_bell.wav";
		public string NoLongerEligibleSoundSetting { get; set; } = "clapping.wav";


		// TODO delete. user language is taken from phone settings
		public SelectedLanguage UserLanguage { get; set; } = SelectedLanguage.English;


		List<Trajectory> trajectories;
		[Ignore]
		public List<Trajectory> Trajectories {
			get { if(trajectories == null) { trajectories = new List<Trajectory>(); } return trajectories; }
			set { trajectories = value ?? new List<Trajectory>(); }
		}


		List<Challenge> challenges;
		[Ignore]
		public List<Challenge> Challenges {
			get { if(challenges == null) { challenges = new List<Challenge>(); } return challenges; }
			set {
				if(value != null) {
					challenges = value;
					// Update the reference of each challenge to its checkpoint and vice-versa.
					foreach(Challenge challenge in challenges) {
						if(checkpoints.ContainsKey(challenge.CheckpointId)) {
							var checkpoint = checkpoints[challenge.CheckpointId];
							challenge.ThisCheckpoint = checkpoint;
							checkpoint.Challenges.Add(challenge);
						}
					}
				}
				else challenges = new List<Challenge>();
			}
		}


		Dictionary<long, Checkpoint> checkpoints;
		[Ignore]
		public Dictionary<long, Checkpoint> Checkpoints {
			get { if(checkpoints == null) { checkpoints = new Dictionary<long, Checkpoint>(); } return checkpoints; }
			set { checkpoints = value ?? new Dictionary<long, Checkpoint>(); }
		}


		IList<Campaign> subscribedCampaigns;
		[Ignore]
		public IList<Campaign> SubscribedCampaigns {
			get { if(subscribedCampaigns == null) { subscribedCampaigns = new List<Campaign>(); } return subscribedCampaigns; }
			set { subscribedCampaigns = value ?? new List<Campaign>(); }
		}


		List<KPI> kpis;
		[Ignore]
		public List<KPI> KPIs {
			get { if(kpis == null) { kpis = new List<KPI>(); } return kpis; }
			set { kpis = value ?? new List<KPI>(); }
		}

		private void AddKPI(KPI newKPIs) {
			KPIs.Insert(0, newKPIs);
		}

		public KPI GetCurrentKPI() {
			var curr = KPIs.FirstOrDefault();
			if(curr == null || curr.IsKPIExpired()) {
				curr?.StoreKPI();
				curr = new KPI { Date = TimeUtil.CurrentEpochTimeSeconds() };
				AddKPI(curr);
			}
			return curr;
		}

		public IEnumerable<KPI> GetFinishedKPIs() {
			// Updates list in case this is the first access.
			GetCurrentKPI();
			// Returns all but the first element in the list.
			var tail = KPIs.Skip(1);
			Debug.WriteLine("User.GetFinishedKPIs(): total=" + KPIs.Count() + " finished=" + tail.Count());
			return tail;
		}


		public override string ToString() {
			//string challengeString = string.Join("\n\t", Challenges) ?? "";
			//string checkpointString = string.Join("\n\t", Checkpoints) ?? "";
			return string.Format("[User Id->{0} Username->{1} Email->{2} AuthToken->{3} Radius->{4} SnapshotVersion->{5}]",
								 Id, Username, Email, IDToken, SearchRadius, WSSnapshotVersion);
		}
	}
}
