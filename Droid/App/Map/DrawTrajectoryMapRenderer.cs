using System.Collections.Generic;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Trace;
using Trace.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(StoreTrajectoryMap), typeof(DrawTrajectoryMapRenderer))]
namespace Trace.Droid {
	public class DrawTrajectoryMapRenderer : MapRenderer, IOnMapReadyCallback {
		GoogleMap map;
		List<Position> routeCoordinates;

		protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<View> e) {
			base.OnElementChanged(e);

			if(e.OldElement != null) {
				// Unsubscribe
			}

			if(e.NewElement != null) {
				var formsMap = (StoreTrajectoryMap) e.NewElement;
				routeCoordinates = formsMap.RouteCoordinates;

				((MapView) Control).GetMapAsync(this);
			}
		}

		public void OnMapReady(GoogleMap googleMap) {
			map = googleMap;

			var polylineOptions = new PolylineOptions();
			polylineOptions.InvokeColor(0x66FF0000);

			foreach(var position in routeCoordinates) {
				polylineOptions.Add(new LatLng(position.Latitude, position.Longitude));
			}

			map.AddPolyline(polylineOptions);
		}
	}
}