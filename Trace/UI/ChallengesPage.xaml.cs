using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms;
using System.Linq;

namespace Trace {

	public partial class ChallengesPage : ContentPage {

		public ChallengesPage() {
			InitializeComponent();
			// Show the challenges saved on the device.
			Task.Run(() => BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges });
			// In the meantime, fetch updates from the server and show when finished.
			Task.Run(() => getChallenges());
		}


		/// <summary>
		/// Show the detailed page of the challenge's checkpoint on click.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">The challenge in the listview that was clicked.</param>
		void OnSelection(object sender, SelectedItemChangedEventArgs e) {
			//((ListView)sender).SelectedItem = null; //uncomment line if you want to disable the visual selection state.
			Checkpoint checkpoint = ((Challenge) e.SelectedItem).ThisCheckpoint;
			if(checkpoint != null) {
				Navigation.PushAsync(new CheckpointDetailsPage(checkpoint));
			}
			else {
				DisplayAlert("Error", "That challenge does not have an associated checkpoint. Probably a DB consistency issue, please report", "Ok");
			}
		}


		/// <summary>
		/// Fetches the list of challenges from the webserver and displays it in the page when finished.
		/// </summary>
		private async void getChallenges() {
			// Get current position to fetch closest challenges
			var position = await CrossGeolocator.Current.GetPositionAsync(timeoutMilliseconds: 10000);

			// Fetch challenges from Webserver
			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.fetchChallenges(position, User.Instance.SearchRadiusInKM, User.Instance.WsSyncVersion));

			if(!result.success) {
				Device.BeginInvokeOnMainThread(async () => {
					await DisplayAlert("Error fetching challenges from server", result.error, "Ok");
				});
				return;
			}

			// Updates the device's DB version to sync with the WS DB.
			// todo User.Instance.WsSyncVersion = result.payload.version;

			// Load shop information into dictionary for fast lookup.
			var checkpoints = new Dictionary<long, Checkpoint>();
			foreach(WSShop checkpoint in result.payload.shops) {
				checkpoints.Add(checkpoint.id, new Checkpoint {
					Name = checkpoint.name,
					Address = checkpoint.contacts.address,
					AvailableHours = checkpoint.details.openTime + " - " + checkpoint.details.closeTime,
					PhoneNumber = checkpoint.contacts.phone,
					WebsiteAddress = checkpoint.contacts.address,
					FacebookAddress = checkpoint.contacts.facebook,
					TwitterAddress = checkpoint.contacts.twitter,
					//BikeFacilities = checkpoint.facilities.ToString(), // todo facilities is a jArray
					Description = checkpoint.details.description
				});
			}

			// Load challenge information into list for display.
			var challenges = new List<Challenge>();
			foreach(WSChallenge challenge in result.payload.challenges) {
				// First look for this challenge's checkpoint.
				Checkpoint checkpoint = null;
				if(checkpoints.ContainsKey(challenge.shopId))
					checkpoint = checkpoints[challenge.shopId];
				else { checkpoint = new Checkpoint { Name = "Non-existing shop" }; }
				// Then create the object and add it to the list for display.
				challenges.Add(new Challenge {
					Id = challenge.id,
					UserId = User.Instance.Id,
					Reward = challenge.reward,
					ThisCheckpoint = checkpoint,
					CheckpointName = checkpoint.Name,
					Condition = challenge.conditions.distance
				});
			}

			SQLiteDB.Instance.SaveItems<Challenge>(challenges);
			User.Instance.Challenges = SQLiteDB.Instance.GetItems<Challenge>().ToList();

			// Finally, display results.
			Device.BeginInvokeOnMainThread(() => {
				BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges };
			});
		}
	}
}