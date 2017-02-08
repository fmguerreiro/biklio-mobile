using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CoreLocation;
using Foundation;
using Plugin.Geolocator.Abstractions;
using Trace.Localization;
using UIKit;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.iOS.Geofencing))]
namespace Trace.iOS {

	public class Geofencing : GeofencingBase {

		public static CLLocationManager LocMgr { get; set; }

		protected override int GeofencesLeft { get; set; } = 19;

		public static Position ReferencePosition;
		private static CLRegion referenceRegion;
		public static string REFERENCE_ID = "refPos";


		public override void Init() {
			LocMgr.RegionEntered -= onArrivingAtCheckpoint;
			LocMgr.RegionEntered += onArrivingAtCheckpoint;

			LocMgr.RegionLeft -= onLeavingReferenceRegion;
			LocMgr.RegionLeft += onLeavingReferenceRegion;
		}


		public override void AddMonitoringRegion(double lon, double lat, string id) {

			bool isGeofencingAvailable = CLLocationManager.LocationServicesEnabled &&
										 CLLocationManager.Status != CLAuthorizationStatus.Denied &&
										 CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion));
			if(isGeofencingAvailable) {
				var newRegion = new CLCircularRegion(new CLLocationCoordinate2D(latitude: lat, longitude: lon), REGION_RADIUS_M, id);

				if(id == REFERENCE_ID) {
					referenceRegion = newRegion;
				}

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


		/// <summary>
		/// When the user gets close to a shop, check for cycle-to-shop rewards and deliver a message.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="region">Region.</param>
		static void onArrivingAtCheckpoint(object sender, CLRegionEventArgs region) {
			Debug.WriteLine($"Just entered region: {region.Region}");

			// Update timers for timeout transitions to happen since a lot of time may have passed between updates.
			RewardEligibilityManager.Instance.Input(ActivityType.Unknown);

			var id = long.Parse(region.Region.Identifier);

			Checkpoint checkpoint;
			User.Instance.Checkpoints.TryGetValue(id, out checkpoint);
			if(checkpoint != null) {
				// TODO remove debug notification
				//new NotificationMessage().Send("regionEnter", "RegionEnter", checkpoint.Name + $"\n{Geolocator.CumulativeAvgSpeed}", 1);

				var msg = "";
				var nRewards = 0;
				foreach(Challenge c in checkpoint.Challenges) {
					if(RewardEligibilityManager.IsUserEligible() && !c.IsClaimed && c.NeededMetersCycling == 0) {
						c.IsComplete = true;
						c.CompletedAt = NSDateConverter.ToDateTime(NSDate.Now).DatetimeToEpochSeconds();
						msg += c.Reward + "\n";
						nRewards++;
					}
					else {
						Debug.WriteLine($"Challenge {c.GId} not earned. isEligible? {RewardEligibilityManager.IsUserEligible()}, isClaimed? {c.IsClaimed}, isCycleToShop? {c.NeededMetersCycling == 0}");
					}
				}
				if(msg != "") {
					msg = Language.YouJustEarned + ":\n" + msg;
					DependencyService.Get<INotificationMessage>().Send("regionEnter", checkpoint.Name, msg, nRewards);
				}
			}
			else { Debug.WriteLine($"Could not find checkpoint with id: {id}"); }
		}


		/// <summary>
		/// This is used primarily for the geofence around the user.
		/// When the user leaves this area (after about 200 m), check pedometer data to see if user is using bycicle.
		/// </summary>
		static void onLeavingReferenceRegion(object sender, CLRegionEventArgs args) {
			if(args.Region.Identifier != REFERENCE_ID)
				return;

			nint taskID = UIApplication.SharedApplication.BeginBackgroundTask(() => { });
			new Task(async () => {
				var firstPos = ReferencePosition;
				var secondPos = await GeoUtils.GetCurrentUserLocation();

				double distance = GeoUtils.DistanceBetweenPoints(firstPos, secondPos);
				double time = Math.Abs((secondPos.Timestamp - firstPos.Timestamp).TotalSeconds);

				var speed = distance / time;

				DependencyService.Get<IMotionActivityManager>().CurrentAvgSpeed = speed;

				ReferencePosition = secondPos;

				// TODO DEBUG notification -- remove
				DependencyService.Get<INotificationMessage>().Send(
					"debug_bg_gps",
					"onLeavingUpdateRegion",
					$"distance {distance}\ntime {time} -> speed {speed}", 0
				);

				// Replace reference region with new one.
				LocMgr.StopMonitoring(referenceRegion);

				var newRegion = new CLCircularRegion(
					new CLLocationCoordinate2D(latitude: secondPos.Latitude, longitude: secondPos.Longitude),
					REGION_RADIUS_M,
					REFERENCE_ID
				);
				referenceRegion = newRegion;
				LocMgr.StartMonitoring(newRegion);

				await DependencyService.Get<IMotionActivityManager>().QueryHistoricalData(
					firstPos.Timestamp.DateTime, secondPos.Timestamp.DateTime
				);

				UIApplication.SharedApplication.EndBackgroundTask(taskID);
			}).Start();
		}
	}
}
