using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;

namespace Trace {

	/// <summary>
	/// Interface for adding a geofence to a checkpoint location.
	/// When the user enters the checkpoint region, it activates the CycleToShop reward.
	/// </summary>
	public abstract class GeofencingBase {

		protected const double REGION_RADIUS_M = 40;

		protected abstract int GeofencesLeft { get; set; }
		protected bool IsProcessing { get; set; }


		public abstract void Init();


		public abstract void AddMonitoringRegion(double lon, double lat, string id);


		public async Task PlaceAllGeofences() {
			Debug.WriteLine($"PlaceAllGeofences() - maxGeofences {GeofencesLeft}");

			// If the user has yet to log in, wait for instantiation.
			if(!WebServerLoginManager.IsOfflineLoggedIn) {
				await Task.Delay(5000);
			}

			IsProcessing = true;
			var favoriteCheckpoints = await User.Instance.GetFavoriteCheckpointsAsync();
			foreach(var c in favoriteCheckpoints) {
				if(GeofencesLeft > 0) {
					Debug.WriteLine($"Trying to add geofence: {c.GId}");
					AddMonitoringRegion(lon: c.Longitude, lat: c.Latitude, id: c.GId.ToString());
				}
				else break;
			}

			if(GeofencesLeft > 0) {
				var orderedCheckpoints = await User.Instance.GetOrderedCheckpointsAsync();
				foreach(var c in orderedCheckpoints) {
					if(GeofencesLeft > 0) {
						AddMonitoringRegion(lon: c.Longitude, lat: c.Latitude, id: c.GId.ToString());
					}
				}
			}
			IsProcessing = false;
		}


		public abstract void RemoveAllGeofences();


		public void RefreshGeofences(Position position, bool shouldRecalculatePositions = true) {
			// First calculate all closest checkpoints.
			if(shouldRecalculatePositions) {
				var newOrderedList = User.Instance.Checkpoints.Values.ToList();
				foreach(Checkpoint c in newOrderedList) {
					c.DistanceToUser = GeoUtils.DistanceBetweenPoints(
						c.Position,
						new Plugin.Geolocator.Abstractions.Position {
							Longitude = position.Longitude,
							Latitude = position.Latitude
						}
					);
				}
			}
			// Next restart geofencing with updated positions.
			RemoveAllGeofences();
			Init();
			Task.Run(() => PlaceAllGeofences());
		}
	}
}