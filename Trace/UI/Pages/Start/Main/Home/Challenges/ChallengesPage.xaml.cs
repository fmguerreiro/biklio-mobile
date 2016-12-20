using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System;
using Trace.Localization;

namespace Trace {

	/// <summary>
	/// Page that displays the list of incomplete challenges.
	/// </summary>
	public partial class ChallengesPage : ContentPage {

		// Reference to the map page in order to update pins when challenges are updated.
		MapPage mapPage;

		public ChallengesPage(MapPage mapPage) {
			InitializeComponent();
			this.mapPage = mapPage;
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
			PullUpHintLabel.IsVisible = false;
		}


		/// <summary>
		/// Fetches the list of challenges and checkpoints 
		/// from the webserver, stores them and displays it in the page when finished.
		/// </summary>
		private async Task getChallenges() {
			// Get current position to fetch closest challenges
			var position = await GeoUtils.GetCurrentUserLocation();

			// Fetch challenges from Webserver
			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.FetchChallenges(position, User.Instance.SearchRadius, User.Instance.WSSnapshotVersion));

			if(!result.success) {
				Device.BeginInvokeOnMainThread(() => {
					DisplayAlert(Language.ErrorFetchingChallenges, result.error, Language.Ok);
				});
				return;
			}

			// Load shop information into dictionary for fast lookup.
			var checkpoints = new Dictionary<long, Checkpoint>();
			foreach(WSShop checkpoint in result.payload.shops) {
				Checkpoint newCheckpoint = createCheckpoint(checkpoint);
				// Check if logo is already downloaded into filesystem.
				if(DependencyService.Get<IFileSystem>().Exists(newCheckpoint.GId.ToString())) {
					newCheckpoint.LogoImageFilePath = newCheckpoint.GId.ToString();
				}
				// If it isn't, download it in the background.
				else if(newCheckpoint.LogoURL != null) {
					Task.Run(() => newCheckpoint.FetchImageAsync(newCheckpoint.LogoURL)).DoNotAwait();
				}
				checkpoints.Add(checkpoint.id, newCheckpoint);
			}
			User.Instance.Checkpoints = checkpoints;

			// Load challenge information into list for display.
			var challenges = new List<Challenge>();
			foreach(WSChallenge challenge in result.payload.challenges) {
				// First look for this challenge's checkpoint.
				Checkpoint checkpoint = null;
				if(checkpoints.ContainsKey(challenge.shopId))
					checkpoint = checkpoints[challenge.shopId];
				else { checkpoint = new Checkpoint(); }
				// Then create the object and add it to the list for displaying later.
				challenges.Add(createChallenge(challenge, checkpoint));
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

			// Delete invalidated checkpoints (i.e., ids in 'canceled' payload field) and their images.
			long[] canceledCheckpointsIds = result.payload.canceled;
			if(canceledCheckpointsIds.Length > 0) {
				SQLiteDB.Instance.DeleteItems<Checkpoint>(canceledCheckpointsIds);
			}
			foreach(long id in canceledCheckpointsIds) {
				DependencyService.Get<IFileSystem>().DeleteImage(id.ToString());
			}

			// Update the in-memory challenge list for display.
			User.Instance.Checkpoints = SQLiteDB.Instance.GetItems<Checkpoint>().ToDictionary(key => key.GId, val => val);
			User.Instance.Challenges = SQLiteDB.Instance.GetItems<Challenge>().ToList();

			// Now that all changes are safely stored, update the device's snapshot version 
			// to indicate it is in sync with the WwbServer version.
			User.Instance.WSSnapshotVersion = result.payload.version;
			SQLiteDB.Instance.SaveItem<User>(User.Instance);

			// Finally, display results.
			Device.BeginInvokeOnMainThread(() => {
				BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges };
				mapPage.UpdatePins();
			});
		}


		static Challenge createChallenge(WSChallenge challenge, Checkpoint checkpoint) {
			return new Challenge {
				GId = challenge.id,
				UserId = User.Instance.Id,
				CheckpointId = challenge.shopId,
				Reward = challenge.reward,
				ThisCheckpoint = checkpoint,
				CheckpointName = checkpoint.Name,
				CreatedAt = challenge.createdAt,
				ExpiresAt = challenge.expiresAt,
				NeededCyclingDistance = (int) challenge.conditions.distance * 1000 // TODO check if this is correct
			};
		}


		static Checkpoint createCheckpoint(WSShop checkpoint) {
			var newCheckpoint = new Checkpoint {
				GId = checkpoint.id,
				UserId = User.Instance.Id,
				OwnerId = checkpoint.ownerId,
				Name = checkpoint.name,
				Address = checkpoint.contacts.address,
				LogoURL = checkpoint.logoURL,
				AvailableHours = checkpoint.details.openTime + " - " + checkpoint.details.closeTime,
				PhoneNumber = checkpoint.contacts.phone,
				WebsiteAddress = checkpoint.contacts.website,
				FacebookAddress = checkpoint.contacts.facebook,
				TwitterAddress = checkpoint.contacts.twitter,
				Longitude = checkpoint.longitude,
				Latitude = checkpoint.latitude,
				//BikeFacilities = checkpoint.facilities.ToString(), // todo facilities is a jArray
				Description = checkpoint.details.description
			};
			return newCheckpoint;
		}


		static void checkForRewards() {
			// TODO check which challenges are already met
		}


		/// <summary>
		/// Show the detailed page of the challenge's checkpoint on click.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">The challenge in the listview that was clicked.</param>
		void OnSelection(object sender, SelectedItemChangedEventArgs e) {
			Checkpoint checkpoint = ((Challenge) e.SelectedItem).ThisCheckpoint;
			if(checkpoint != null) {
				Navigation.PushAsync(new CheckpointDetailsPage(checkpoint));
			}
			else {
				DisplayAlert(Language.Error, Language.ChallengeWithoutCheckpointError, Language.Ok);
			}
		}
	}
}