using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;
using SQLite;

namespace Trace {

	/// <summary>
	/// The User class stores the user's information.
	/// This information includes personal information, as well as information 
	/// obtained while using the device, i.e., the other business classes.
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
		public string Email { get; set; } = "";
		public string PictureURL { get; set; } = "";

		// Private Info
		public string Name { get; set; } = "";
		public string Phone { get; set; } = "";
		public string Address { get; set; } = "";

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
		public double PrevLongitude { get; set; }
		public double PrevLatitude { get; set; }
		[Ignore]
		public Position Position { get { return new Position { Latitude = PrevLatitude, Longitude = PrevLongitude }; } }


		// The webserver stores several snapshots of the challenge and checkpoint data.
		// This value is used to tell the webserver the most recent version of the data in the device.
		public long WSSnapshotVersion { get; set; } = 0;


		// Used in calories calculations. Defaults are European averages.
		public int Age { get; set; } = 37;
		public SelectedGender Gender { get; set; } = SelectedGender.Male;
		public double Weight { get; set; } = 70.8;// kilograms
		public double Height { get; set; } = 1.78; // meters


		public int SearchRadius { get; set; } = 100; // kilometers
		public int MaxChallenges { get; set; } = 50; // TODO not yet used


		public string WalkingSoundSetting { get; set; } = "walking_pavement.mp3";
		public string RunningSoundSetting { get; set; } = "running_pavement.mp3";
		public string CyclingSoundSetting { get; set; } = "bike_no_pedaling.mp3";
		public string VehicularSoundSetting { get; set; } = "spaceship_idle.mp3";
		public string StationarySoundSetting { get; set; } = "silence.mp3";

		public string CongratulatorySoundSetting { get; set; } = "bike_bell.mp3";
		public string NoLongerEligibleSoundSetting { get; set; } = "clapping.mp3";


		public bool IsFirstLogin { get; set; } = true;
		public bool IsBackgroundAudioEnabled { get; set; } = false;


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


		public List<Challenge> GetRewards() {
			return Challenges.FindAll((x) => x.IsComplete && !x.IsClaimed);
		}


		Dictionary<long, Checkpoint> checkpoints;
		[Ignore]
		public Dictionary<long, Checkpoint> Checkpoints {
			get { if(checkpoints == null) { checkpoints = new Dictionary<long, Checkpoint>(); } return checkpoints; }
			set { checkpoints = value ?? new Dictionary<long, Checkpoint>(); }
		}


		public async Task<IList<Checkpoint>> GetOrderedCheckpointsAsync() {
			return await Task.Run(() => Checkpoints.Values.OrderBy((x) => x.DistanceToUser).ToList());
		}

		// Returns a list of the user's favorite checkpoints ordered by distance to the user.
		public async Task<IList<Checkpoint>> GetFavoriteCheckpointsAsync() {
			return await Task.Run(() => Checkpoints.Values.Where((x) => x.IsUserFavorite).OrderBy((x) => x.DistanceToUser).ToList());
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
