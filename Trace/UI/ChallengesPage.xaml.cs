using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms;
using System.Linq;
using System;

namespace Trace {

	public partial class ChallengesPage : ContentPage {

		public ChallengesPage() {
			InitializeComponent();
			// Show the challenges saved on the device.
			Task.Run(() => BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges });
		}


		/// <summary>
		/// On pull-to-refresh, fetch updates from the server and show when finished.
		/// </summary>
		/// <param name="sender">The listview.</param>
		/// <param name="e">E.</param>
		async void OnRefresh(object sender, EventArgs e) {
			var list = (ListView) sender;
			await Task.Run(() => getChallenges());
			list.IsRefreshing = false;
		}


		/// <summary>
		/// Fetches the list of challenges and checkpoints 
		/// from the webserver, stores them and displays it in the page when finished.
		/// </summary>
		private async Task getChallenges() {
			// Get current position to fetch closest challenges
			var position = await CrossGeolocator.Current.GetPositionAsync(timeoutMilliseconds: 10000);

			// Fetch challenges from Webserver
			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.fetchChallenges(position, User.Instance.SearchRadiusInKM, User.Instance.WSSnapshotVersion));

			if(!result.success) {
				Device.BeginInvokeOnMainThread(() => {
					DisplayAlert("Error fetching challenges from server", result.error, "Ok");
				});
				return;
			}

			// Load shop information into dictionary for fast lookup.
			var checkpoints = new Dictionary<long, Checkpoint>();
			foreach(WSShop checkpoint in result.payload.shops) {
				checkpoints.Add(checkpoint.id, new Checkpoint {
					//Id = checkpoint.id,
					UserId = User.Instance.Id,
					// TODO owner id
					Name = checkpoint.name,
					Address = checkpoint.contacts.address,
					AvailableHours = checkpoint.details.openTime + " - " + checkpoint.details.closeTime,
					PhoneNumber = checkpoint.contacts.phone,
					WebsiteAddress = checkpoint.contacts.address,
					FacebookAddress = checkpoint.contacts.facebook,
					TwitterAddress = checkpoint.contacts.twitter,
					Longitude = checkpoint.longitude,
					Latitude = checkpoint.latitude,
					//BikeFacilities = checkpoint.facilities.ToString(), // todo facilities is a jArray
					Description = checkpoint.details.description
				});
			}
			User.Instance.Checkpoints = checkpoints;

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
					//Id = challenge.id,
					UserId = User.Instance.Id,
					Reward = challenge.reward,
					ThisCheckpoint = checkpoint,
					CheckpointName = checkpoint.Name,
					Condition = challenge.conditions.distance
				});
			}

			// Update or create the received challenges and checkpoints.
			IEnumerable<Checkpoint> checkpointList = checkpoints.Values;
			SQLiteDB.Instance.SaveItems<Checkpoint>(checkpointList);
			SQLiteDB.Instance.SaveItems<Challenge>(challenges);

			// Delete invalidated challenges (i.e., ids in 'canceledChallenges' payload field).
			long[] canceledChallengeIds = result.payload.canceledChallenges;
			if(canceledChallengeIds.Length > 0) {
				SQLiteDB.Instance.DeleteItems<Challenge>(canceledChallengeIds);
			}

			// Delete invalidated checkpoints (i.e., ids in 'canceled' payload field).
			long[] canceledCheckpointsIds = result.payload.canceled;
			if(canceledCheckpointsIds.Length > 0) {
				SQLiteDB.Instance.DeleteItems<Checkpoint>(canceledCheckpointsIds);
			}

			// Update the in-memory challenge list for display.
			// We leave invalidated checkpoints in memory to save on the extra processing.
			User.Instance.Challenges = SQLiteDB.Instance.GetItems<Challenge>().ToList();

			// Now that all changes are safely stored, update the device's snapshot version 
			// to indicate it is in sync with the WwbServer version.
			User.Instance.WSSnapshotVersion = result.payload.version;
			SQLiteDB.Instance.SaveItem<User>(User.Instance);

			// Finally, display results.
			Device.BeginInvokeOnMainThread(() => {
				BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges };
			});
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
	}
}