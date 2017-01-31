using System;
using System.Diagnostics;
using CoreLocation;
using Foundation;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.iOS.Geofencing))]
namespace Trace.iOS {

	public class Geofencing : GeofencingBase {

		public static CLLocationManager LocMgr { get; set; }

		protected override int GeofencesLeft { get; set; } = 20;


		public override void Init() {
			LocMgr.RegionEntered -= onRegionEnter;
			LocMgr.RegionEntered += onRegionEnter;
		}


		public override void AddMonitoringRegion(double lon, double lat, string id) {

			bool isGeofencingAvailable = CLLocationManager.LocationServicesEnabled &&
										 CLLocationManager.Status != CLAuthorizationStatus.Denied &&
										 CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion));
			if(isGeofencingAvailable) {
				var newRegion = new CLCircularRegion(new CLLocationCoordinate2D(latitude: lat, longitude: lon), REGION_RADIUS_M, id);

				if(GeofencesLeft > 0) {
					Debug.WriteLine($"Registering for region: {newRegion.Description}");
					LocMgr.StartMonitoring(newRegion);
					GeofencesLeft--;
				}
				else {
					Checkpoint c = null;
					try {
						c = User.Instance.Checkpoints[long.Parse(id)];
					}
					catch(Exception e) { Debug.WriteLine($"AddMonitoringRegion - Error parsing id or fetching checkpoint {id}:\n{e.Message}"); return; }
					// If it is a favorite, try to find room (remove a non-favorite).
					if(c.IsUserFavorite) {
						foreach(var r in LocMgr.MonitoredRegions) {
							var checkpointId = long.Parse(((CLRegion) r).Identifier);
							if(!User.Instance.Checkpoints[checkpointId].IsUserFavorite) {
								LocMgr.StopMonitoring((CLRegion) r);
								LocMgr.StartMonitoring(newRegion, REGION_RADIUS_M);
								return;
							}
						}
					}
				}
			}
			else {
				Debug.WriteLine("This app requires region monitoring, which is unavailable on this device");
			}
		}


		public override void RemoveAllGeofences() {
			foreach(var region in LocMgr.MonitoredRegions) {
				Debug.WriteLine($"removing region: {region.GetType()} {region}");
				LocMgr.StopMonitoring((CLRegion) region);
			}
			GeofencesLeft = 20;
		}


		static void onRegionEnter(object sender, CLRegionEventArgs region) {
			Debug.WriteLine($"Just entered region: {region.Region}");

			var id = long.Parse(region.Region.Identifier);

			Checkpoint checkpoint;
			User.Instance.Checkpoints.TryGetValue(id, out checkpoint);
			if(checkpoint != null) {
				// TODO remove debug notification
				new NotificationMessage().Send("regionEnter", "RegionEnter", checkpoint.Name, 1);

				foreach(Challenge c in checkpoint.Challenges) {
					if(RewardEligibilityManager.IsUserEligible() && !c.IsClaimed && c.NeededCyclingDistance == 0) {
						c.IsComplete = true;
						c.CompletedAt = NSDateConverter.ToDateTime(NSDate.Now).DatetimeToEpochSeconds();
					}
					else {
						Debug.WriteLine($"Challenge {c.GId} not earned. isEligible? {RewardEligibilityManager.IsUserEligible()}, isClaimed? {c.IsClaimed}, isCycleToShop? {c.NeededCyclingDistance == 0}");
					}
				}
			}
			else { Debug.WriteLine($"Could not find checkpoint with id: {id}"); }
		}
	}
}
