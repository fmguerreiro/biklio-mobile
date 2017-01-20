using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Linq;
using System;
using Trace.Localization;
using Plugin.Geolocator.Abstractions;

namespace Trace {

	/// <summary>
	/// Page that displays the list of checkpoints ordered by distance to User.
	/// </summary>
	public partial class CheckpointListPage : ContentPage {

		// Reference to the map page in order to update pins when challenges are updated.
		MapPage mapPage;

		//static readonly object updateListLock = new object();
		bool shouldFilterByFavorites = false;


		public CheckpointListPage(MapPage mapPage) {
			InitializeComponent();
			if(Device.OS == TargetPlatform.iOS) { Icon = "challengelist__trophy.png"; }
			this.mapPage = mapPage;

			// Show the checkpoints saved on the device. Initialize the list in the background to lighten load on UI thread.
			Task.Run(async () => {
				var orderedCheckpointList = await User.Instance.GetOrderedCheckpointsAsync();
				BindingContext = new CheckpointListModel { Checkpoints = orderedCheckpointList.Select((x) => new CheckpointViewModel(x)).ToList() };
			}).DoNotAwait();
		}


		/// <summary>
		/// On pull-to-refresh, fetch updates from the server and show when finished.
		/// When finished, update the loading icon and remove the label.
		/// </summary>
		/// <param name="sender">The listview.</param>
		/// <param name="e">E.</param>
		void onRefresh(object sender, EventArgs e) {
			Task.Run(() => getCheckpoints()).DoNotAwait();
		}


		/// <summary>
		/// Fetches the list of challenges and checkpoints 
		/// from the webserver, stores them and displays it in the page when finished.
		/// </summary>
		private async Task getCheckpoints() {
			// Get current position to fetch closest challenges
			var position = await GeoUtils.GetCurrentUserLocation();

			// If the user got too far from the previous position where he got the last set of challenges, reset Snapshot point.
			await checkIfUserRequiresNewCheckpoints();

			// Fetch checkpoints from Webserver.
			var client = new WebServerClient();
			WSResult result = await Task.Run(() => client.FetchCheckpoints(position, User.Instance.SearchRadius, User.Instance.WSSnapshotVersion));

			if(!result.success) {
				// If the user is offline or WS is down, simply update distance to user for each checkpoint.
				var newOrderedList = User.Instance.Checkpoints.Values.ToList();
				foreach(Checkpoint c in newOrderedList) {
					c.DistanceToUser = GeoUtils.DistanceBetweenPoints(c.Position, User.Instance.Position);
				}
				newOrderedList = newOrderedList.OrderBy((x) => x.DistanceToUser).ToList();

				Device.BeginInvokeOnMainThread(() => {
					//DisplayAlert(Language.ErrorFetchingChallenges, result.error, Language.Ok);
					checkpointListView.IsRefreshing = false;
					PullUpHintLabel.IsVisible = false;
					BindingContext = new CheckpointListModel { Checkpoints = newOrderedList.Select((x) => new CheckpointViewModel(x)).ToList() };
				});
				return;
			}

			// Load checkpoint information into dictionary for fast lookup.
			var checkpoints = new Dictionary<long, Checkpoint>();
			loadCheckpoints(result, checkpoints);

			var deletedChallenges = deleteInvalidatedCheckpoints(result, checkpoints);

			// Load challenge information into list for display.
			var challenges = new List<Challenge>();
			loadChallenges(result, checkpoints, challenges);

			deleteInvalidatedChallenges(deletedChallenges, challenges);
			checkClaimedChallenges(challenges);

			// Update or create the received challenges and checkpoints.
			IEnumerable<Checkpoint> checkpointList = checkpoints.Values;
			SQLiteDB.Instance.SaveItems(checkpointList);
			SQLiteDB.Instance.SaveItems(challenges);

			// Update the in-memory challenge list for display.
			User.Instance.Checkpoints = SQLiteDB.Instance.GetItems<Checkpoint>().ToDictionary(key => key.GId, val => val);
			User.Instance.Challenges = SQLiteDB.Instance.GetItems<Challenge>().ToList();

			// Now that all changes are safely stored, update the device's snapshot version 
			// to indicate it is in sync with the WebServer version.
			User.Instance.WSSnapshotVersion = result.payload.version;
			SQLiteDB.Instance.SaveUser(User.Instance);

			Task.Run(() => fetchPinImages(User.Instance.Challenges)).DoNotAwait();

			Task.Run(() => checkForRewards()).DoNotAwait();

			// Finally, display available challenges.
			//var unclaimedChallenges = User.Instance.Challenges.FindAll((x) => !x.IsClaimed);
			var orderedResult = await User.Instance.GetOrderedCheckpointsAsync();
			Device.BeginInvokeOnMainThread(() => {
				BindingContext = new CheckpointListModel { Checkpoints = orderedResult.Select((x) => new CheckpointViewModel(x)).ToList() };
				mapPage.UpdatePins();
				checkpointListView.IsRefreshing = false;
				PullUpHintLabel.IsVisible = false;
			});
		}


		void loadChallenges(WSResult result, Dictionary<long, Checkpoint> checkpoints, List<Challenge> challenges) {
			foreach(WSChallenge challenge in result.payload.challenges) {
				// Only create the challenge if it has a valid checkpoint.
				Checkpoint checkpoint = null;
				if(checkpoints.ContainsKey(challenge.shopId)) {
					checkpoint = checkpoints[challenge.shopId];
					var newChallenge = createChallenge(challenge, checkpoint);
					checkpoint.Challenges.Add(newChallenge);
					challenges.Add(newChallenge);
				}
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
				NeededCyclingDistance = (int) challenge.conditions.distance,
				IsRepeatable = challenge.repeatable
			};
		}


		void loadCheckpoints(WSResult result, Dictionary<long, Checkpoint> checkpoints) {
			foreach(WSShop checkpoint in result.payload.shops) {
				Checkpoint newCheckpoint = createCheckpoint(checkpoint);
				// Check if logo is already downloaded into filesystem.
				//if(DependencyService.Get<IFileSystem>().Exists(newCheckpoint.GId.ToString())) {
				//	newCheckpoint.LogoImageFilePath = newCheckpoint.GId.ToString();
				//}
				//// If it isn't, download it in the background.
				//else if(newCheckpoint.LogoURL != null) {
				//	Task.Run(() => newCheckpoint.FetchImageAsync(newCheckpoint.LogoURL)).DoNotAwait();
				//}
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
				Type = checkpoint.details.type.description,
				Address = checkpoint.contacts.address,
				LogoURL = checkpoint.logoURL,
				AvailableHours = checkpoint.details.openTime + " - " + checkpoint.details.closeTime,
				PhoneNumber = checkpoint.contacts.phone,
				Email = checkpoint.contacts.email,
				WebsiteAddress = checkpoint.contacts.website,
				FacebookAddress = checkpoint.contacts.facebook,
				TwitterAddress = checkpoint.contacts.twitter,
				Longitude = checkpoint.longitude,
				Latitude = checkpoint.latitude,
				DistanceToUser = GeoUtils.DistanceBetweenPoints(User.Instance.Position, new Position { Longitude = checkpoint.longitude, Latitude = checkpoint.latitude }),
				MapImageURL = checkpoint.mapURL,
				//BikeFacilities = checkpoint.facilities.ToString(), // todo facilities is a jArray
				Description = checkpoint.details.description
			};
			Debug.WriteLine($"{newCheckpoint.DistanceToUser}");
			return newCheckpoint;
		}


		/// <summary>
		/// Delete invalidated checkpoints (i.e., ids in 'canceled' payload field) and their 
		/// </summary>
		/// <param name="result">Result.</param>
		long[] deleteInvalidatedCheckpoints(WSResult result, Dictionary<long, Checkpoint> checkpointsDict) {
			long[] canceledCheckpointsIds = result.payload.canceled;
			long[] canceledChallengesIds = result.payload.canceledChallenges;
			var updatedCancelledChallenges = new HashSet<long>(canceledChallengesIds ?? new long[0]);
			if(canceledCheckpointsIds.Length > 0) {
				SQLiteDB.Instance.DeleteItems<Checkpoint>(canceledCheckpointsIds);
			}
			foreach(long id in canceledCheckpointsIds) {
				DependencyService.Get<IFileSystem>().DeleteImage(id.ToString());
				Checkpoint checkpoint = null;
				// Check this checkpoint's challenges, and invalidade them as well.
				User.Instance.Checkpoints.TryGetValue(id, out checkpoint);
				if(checkpoint != null) {
					var invalidatedChallenges = checkpoint.Challenges.Select(item => item.Id).ToList();
					if(invalidatedChallenges != null)
						updatedCancelledChallenges.UnionWith(invalidatedChallenges);
				}
				checkpointsDict.Remove(id);
			}
			return updatedCancelledChallenges.ToArray();
		}


		/// <summary>
		/// Delete invalidated challenges (i.e., ids in 'canceledChallenges' payload field).
		/// </summary>
		/// <param name="canceledChallengeIds">Canceled challenge identifiers.</param>
		/// <param name="challengeList">Challenge list.</param>
		void deleteInvalidatedChallenges(long[] canceledChallengeIds, List<Challenge> challengeList) {
			if(canceledChallengeIds.Length > 0) {
				SQLiteDB.Instance.DeleteItems<Challenge>(canceledChallengeIds);
				challengeList.RemoveAll(x => canceledChallengeIds.Contains(x.GId));
			}
		}


		/// <summary>
		/// Deletes straggling challenges that lost their checkpoint.
		/// </summary>
		/// <param name="result">Result.</param>
		void deleteChallengesWithInvalidCheckpoints(WSResult result, List<Challenge> challengeList) {
			long[] canceledChallengeIds = result.payload.canceledChallenges;
			if(canceledChallengeIds.Length > 0) {
				SQLiteDB.Instance.DeleteItems<Challenge>(canceledChallengeIds);
				challengeList.RemoveAll(x => canceledChallengeIds.Contains(x.GId));
			}
		}


		/// <summary>
		/// Checks for challenges the user has already claimed, 
		/// so that they are not overwritten by the same challenges and reset.
		/// </summary>
		/// <param name="newChallenges">New challenge list.</param>
		void checkClaimedChallenges(List<Challenge> newChallenges) {
			var claimedChallenges = User.Instance.Challenges.FindAll((x) => x.IsClaimed);
			if(claimedChallenges.Count > 0) {
				var newChallengeHash = newChallenges.ToDictionary((x) => x.Id);
				foreach(var claimed in claimedChallenges) {
					var newChallenge = newChallengeHash[claimed.Id];
					newChallenge.IsClaimed = true;
					newChallenge.CompletedAt = claimed.CompletedAt;
					newChallenge.ClaimedAt = claimed.ClaimedAt;
				}
			}
		}


		/// <summary>
		/// Checks if user requires new challenges.
		/// A user requires new challenges if he is a new area, far from the place he got the previous challenges.
		/// </summary>
		async Task checkIfUserRequiresNewCheckpoints() {
			var initPos = new Position {
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
		/// Fetch all the images for the map pin in the background.
		/// </summary>
		/// <param name="challenges">Challenges.</param>
		private void fetchPinImages(List<Challenge> challenges) {
			Debug.WriteLine("STARTING fetchPinImages()");
			foreach(Challenge c in challenges) {
				var url = c.ThisCheckpoint.LogoURL;
				if(string.IsNullOrEmpty(c.ThisCheckpoint.PinLogoPath) && !url.Equals("challengelist__default_shop.png")) {
					Task.Run(() => c.ThisCheckpoint.FetchImageAsync(url)).DoNotAwait();
				}
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
						var cycledDistance = RewardEligibilityManager.CycledDistanceBetween(start, end);
						if(cycledDistance >= c.NeededCyclingDistance) {
							c.IsComplete = true;
							c.CompletedAt = now;
							rewardCounter++;
							// Record challenge completed event. TODO KPI calculate claimedAt ...
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
		void onTapped(object sender, ItemTappedEventArgs e) {
			var checkpoint = (CheckpointViewModel) e.Item;
			if(checkpoint != null) {
				Navigation.PushAsync(new CheckpointDetailsPage(checkpoint));
			}
			else {
				DisplayAlert(Language.Error, Language.ChallengeWithoutCheckpointError, Language.Ok);
			}
		}

		async void onFilterFavorites(object sender, EventArgs e) {

			IList<Checkpoint> checkpoints = null;
			shouldFilterByFavorites = !shouldFilterByFavorites;
			if(shouldFilterByFavorites)
				checkpoints = await User.Instance.GetFavoriteCheckpointsAsync();
			else
				checkpoints = await User.Instance.GetOrderedCheckpointsAsync();

			BindingContext = new CheckpointListModel { Checkpoints = checkpoints.Select((x) => new CheckpointViewModel(x)).ToList() };

		}
	}
}
