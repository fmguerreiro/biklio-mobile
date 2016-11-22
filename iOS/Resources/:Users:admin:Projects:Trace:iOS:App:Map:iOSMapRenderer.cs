using CoreLocation;
using MapKit;
using Trace;
using Trace.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;

[assembly: ExportRenderer(typeof(DrawTrajectoryMap), typeof(CustomMapRenderer))]
namespace Trace.iOS {

	public class iOSMapRenderer : MapRenderer {
		MKPolylineRenderer polylineRenderer;

		protected override void OnElementChanged(ElementChangedEventArgs<View> e) {
			base.OnElementChanged(e);

			if(e.OldElement != null) {
				var nativeMap = Control as MKMapView;
				nativeMap.OverlayRenderer = null;
			}

			if(e.NewElement != null) {
				var formsMap = (DrawTrajectoryMap) e.NewElement;
				var nativeMap = Control as MKMapView;

				nativeMap.OverlayRenderer = GetOverlayRenderer;

				CLLocationCoordinate2D[] coords = new CLLocationCoordinate2D[formsMap.RouteCoordinates.Count];

				int index = 0;
				foreach(var position in formsMap.RouteCoordinates) {
					coords[index] = new CLLocationCoordinate2D(position.Latitude, position.Longitude);
					index++;
				}

				var routeOverlay = MKPolyline.FromCoordinates(coords);
				nativeMap.AddOverlay(routeOverlay);
			}
		}
	}
}