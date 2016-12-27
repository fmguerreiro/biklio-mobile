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
			BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges };
			//Task.Run(() => BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges });
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

			await checkIfUserRequiresNewChallenges();

			// Fetch challenges from Webserver
			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.FetchChallenges(position, User.Instance.SearchRadius, User.Instance.WSSnapshotVersion));

			if(!result.success) {
				Device.BeginInvokeOnMainThread(() => {
					DisplayAlert(Language.ErrorFetchingChallenges, result.error, Language.Ok);
				});
				ChallengeListView.IsRefreshing = false;
				PullUpHintLabel.IsVisible = false;
				return;
			}

			// Load shop information into dictionary for fast lookup.
			var checkpoints = new Dictionary<long, Checkpoint>();
			loadCheckpoints(result, checkpoints);

			// Load challenge information into list for display.
			var challenges = new List<Challenge>();
			loadChallenges(result, checkpoints, challenges);

			// Update or create the received challenges and checkpoints.
			IEnumerable<Checkpoint> checkpointList = checkpoints.Values;
			SQLiteDB.Instance.SaveItems(checkpointList);
			SQLiteDB.Instance.SaveItems(challenges);
			deleteInvalidatedItems(result);

			// Update the in-memory challenge list for display.
			User.Instance.Checkpoints = SQLiteDB.Instance.GetItems<Checkpoint>().ToDictionary(key => key.GId, val => val);
			User.Instance.Challenges = SQLiteDB.Instance.GetItems<Challenge>().ToList();

			// Now that all changes are safely stored, update the device's snapshot version 
			// to indicate it is in sync with the WebServer version.
			User.Instance.WSSnapshotVersion = result.payload.version;
			SQLiteDB.Instance.SaveItem(User.Instance);

			Task.Run(() => checkForRewards()).DoNotAwait();

			// Finally, display results.
			Device.BeginInvokeOnMainThread(() => {
				BindingContext = new ChallengeVM { Challenges = User.Instance.Challenges };
				mapPage.UpdatePins();
			});
		}


		void loadChallenges(WSResult result, Dictionary<long, Checkpoint> checkpoints, List<Challenge> challenges) {
			foreach(WSChallenge challenge in result.payload.challenges) {
				// First look for this challenge's checkpoint.
				Checkpoint checkpoint = null;
				if(checkpoints.ContainsKey(challenge.shopId))
					checkpoint = checkpoints[challenge.shopId];
				else { checkpoint = new Checkpoint(); }
				// Then create the object and add it to the list for displaying later.
				challenges.Add(createChallenge(challenge, checkpoint));
			}
		}


		Challenge createChallenge(WSChallenge challenge, Checkpoint checkpoint) {
			return new Challenge {
				GId = challenge.id,
				UserId = User.Instance.Id,
				CheckpointId = challenge.shopId,
				Reward = challenge.reward,
				ThisCheckpoint = checkpoint,
				CheckpointName = checkpoint.Name,
				CreatedAt = challenge.createdAt,
				ExpiresAt = challenge.expiresAt,
				NeededCyclingDistance = (int) challenge.conditions.distance * 1000
			};
		}


		void loadCheckpoints(WSResult result, Dictionary<long, Checkpoint> checkpoints) {
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
		}


		Checkpoint createCheckpoint(WSShop checkpoint) {
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


		void deleteInvalidatedItems(WSResult result) {
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
		}


		/// <summary>
		/// Checks if user requires new challenges.
		/// A user requires new challenges if he is a new area, far from the place he got the previous challenges.
		/// </summary>
		private async Task checkIfUserRequiresNewChallenges() {
			var initPos = new Plugin.Geolocator.Abstractions.Position {
				Longitude = User.Instance.PrevLongitude,
				Latitude = User.Instance.PrevLatitude
			};
			var currPos = await GeoUtils.GetCurrentUserLocation();
			if(GeoUtils.DistanceBetweenPoints(initPos, currPos) > User.Instance.SearchRadius / 2) {
				User.Instance.WSSnapshotVersion = 0;
				User.Instance.PrevLongitude = currPos.Longitude;
				User.Instance.PrevLatitude = currPos.Latitude;
			}
		}


		/// <summary>
		/// Check if the user has met the conditions for any received challenge.
		/// </summary>
		private void checkForRewards() {
			var now = TimeUtil.CurrentEpochTimeSeconds();
			int rewardCounter = 0;
			foreach(Challenge c in User.Instance.Challenges) {
				if(!c.IsComplete) {
					var start = c.CreatedAt;
					var end = c.ExpiresAt;
					if(TimeUtil.IsWithinPeriod(now, start, end)) {
						// Check if distance cycled meets the criteria.
						if(c.NeededCyclingDistance <= RewardEligibilityManager.CycledDistanceBetween(start, end)) {
							c.IsComplete = true;
							c.CompletedAt = now;
							rewardCounter++;
							// Record challenge completed event. TODO calculate claimedAt ...
							User.Instance.GetCurrentKPI().AddChallengeConditionCompletedEvent(c.GId,
																							  TimeUtil.CurrentEpochTimeSeconds(),
																							  0);
						}
					}
				}
			}
			if(rewardCounter > 0) {
				const string NOTIFICATION_ID = "getChallengesNotification";
				DependencyService.Get<INotificationMessage>().Send(NOTIFICATION_ID, Language.RewardsUnlocked, string.Format(Language.RewardsUnlockedMessage, rewardCounter), rewardCounter);
			}
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