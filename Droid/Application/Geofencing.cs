
using System.Diagnostics;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.Droid.Geofencing))]
namespace Trace.Droid {

	// TODO implement android geofencing
	public class Geofencing : GeofencingBase {

		//public static CLLocationManager LocMgr;

		protected override int MaxGeofences { get; set; } = 100;


		public override void AddMonitoringRegion(double lon, double lat, string id) {

			//var region = new CLCircularRegion(new CLLocationCoordinate2D(latitude: lat, longitude: lon), REGION_RADIUS_M, id);

			//bool isGeofencingAvailable = CLLocationManager.LocationServicesEnabled &&
			//							 CLLocationManager.Status != CLAuthorizationStatus.Denied &&
			//							 CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion));
			//if(isGeofencingAvailable) {

			//	LocMgr.RegionEntered += onRegionEnter;

			//	LocMgr.StartMonitoring(region);

			//}
			//else {
			//	Debug.WriteLine("This app requires region monitoring, which is unavailable on this device");
			//}
		}


		//void onRegionEnter(object sender, CLRegionEventArgs region) {

		//	Debug.WriteLine($"Just entered region: {region.Region}");

		//	var id = long.Parse(region.Region.Identifier);

		//	Checkpoint checkpoint;
		//	User.Instance.Checkpoints.TryGetValue(id, out checkpoint);
		//	if(checkpoint != null) {
		//		// TODO remove debug notification
		//		new NotificationMessage().Send("regionEnter", "RegionEnter", checkpoint.Name, 1);
		//		// TODO ...
		//	}
		//	else { Debug.WriteLine($"Could not find checkpoint with id: {id}"); }
		//}
	}
}
