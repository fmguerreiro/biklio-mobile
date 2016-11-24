using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace Trace {

	public partial class ChallengesPage : ContentPage {

		public ChallengesPage() {
			InitializeComponent();

			Task.Run(() => getChallenges());

			/*
			var monkeys = new List<Challenge> {
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"}
			};
			BindingContext = new ChallengeVM { Challenges = monkeys }; */
		}


		void OnSelection(object sender, SelectedItemChangedEventArgs e) {

			DisplayAlert("Item Selected", ((Challenge) e.SelectedItem).Reward, "Ok");
			//((ListView)sender).SelectedItem = null; //uncomment line if you want to disable the visual selection state.
		}

		private async void getChallenges() {
			// Get current position to fetch closest challenges
			var position = await CrossGeolocator.Current.GetPositionAsync(timeoutMilliseconds: 10000);

			// Fetch challenges from Webserver
			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.fetchChallenges(position, User.SearchRadiusInKM, WebServerConstants.VERSION));

			if(!result.success) {
				Device.BeginInvokeOnMainThread(async () => {
					await DisplayAlert("Error fetching challenges from server", result.error, "Ok");
				});
				return;
			}

			// Load shop information into dictionary for fast lookup.
			var checkpoints = new Dictionary<long, Checkpoint>();
			foreach(WSShop checkpoint in result.payload.shops) {
				checkpoints.Add(checkpoint.id, new Checkpoint {
					Name = checkpoint.name,
					Address = checkpoint.contacts.address,
					AvailableHours = checkpoint.details.openTime + " - " + checkpoint.details.closeTime,
					WebsiteAddress = checkpoint.contacts.address
					// todo rest of information ...
				});
			}

			// Load challenge information into list for display.
			var challenges = new List<Challenge>();
			foreach(WSChallenge challenge in result.payload.challenges) {
				Checkpoint checkpoint = null;
				if(checkpoints.ContainsKey(challenge.shopId))
					checkpoint = checkpoints[challenge.shopId];
				else { checkpoint = new Checkpoint { Name = "Non-existing shop" }; }
				challenges.Add(new Challenge {
					Reward = challenge.reward,
					ThisCheckpoint = checkpoint,
					CheckpointName = checkpoint.Name,
					Condition = challenge.conditions.distance
				});
			}

			// Finally, display results.
			Device.BeginInvokeOnMainThread(() => {
				BindingContext = new ChallengeVM { Challenges = challenges };
			});
		}
	}
}
