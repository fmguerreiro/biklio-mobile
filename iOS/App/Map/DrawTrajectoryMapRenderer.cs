using CoreLocation;
using MapKit;
using Trace;
using Trace.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(StoreTrajectoryMap), typeof(DrawTrajectoryMapRenderer))]
namespace Trace.iOS {

	/// <summary>
	/// Custom renderer that draws a polyline on the map to show the user's trajectory.
	/// </summary>
	public class DrawTrajectoryMapRenderer : MapRenderer {

		MKPolylineRenderer polylineRenderer;

		protected override void OnElementChanged(ElementChangedEventArgs<View> e) {
			base.OnElementChanged(e);

			if(e.OldElement != null) {
				var nativeMap = Control as MKMapView;
				nativeMap.OverlayRenderer = null;
			}

			if(e.NewElement != null) {
				var formsMap = (StoreTrajectoryMap) e.NewElement;
				var nativeMap = Control as MKMapView;

				nativeMap.OverlayRenderer = GetOverlayRenderer;

				CLLocationCoordinate2D[] coords = new CLLocationCoordinate2D[formsMap.RouteCoordinates.Count];

				int index = 0;
				foreach(var position in formsMap.RouteCoordinates) {
					coords[index] = new CLLocationCoordinate2D(position.Latitude, position.Longitude);
					index++;
				}

				var routeOverlay = MKPolyline.FromCoordinates(coords);
				// todo fit map - nativeMap.MapRectThatFits(routeOverlay.BoundingMapRect);
				nativeMap.AddOverlay(routeOverlay);
			}
		}

		MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlay) {
			if(polylineRenderer == null) {
				polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline);
				polylineRenderer.FillColor = UIColor.Red;
				polylineRenderer.StrokeColor = UIColor.Blue;
				polylineRenderer.LineWidth = 3;
				polylineRenderer.Alpha = 0.6f;
			}
			return polylineRenderer;
		}
	}
}