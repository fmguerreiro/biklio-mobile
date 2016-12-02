using System;
using System.Collections.Generic;
using CoreGraphics;
using CoreLocation;
using MapKit;
using Trace;
using Trace.iOS;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TraceMap), typeof(TraceMapRenderer))]
namespace Trace.iOS {

	/// <summary>
	/// Custom renderer that shows the challenges and draws a polyline on the map to show the user's trajectory.
	/// </summary>
	public class TraceMapRenderer : MapRenderer {

		MKPolylineRenderer polylineRenderer;

		UIView customPinView;
		List<ChallengePin> customPins;


		protected override void OnElementChanged(ElementChangedEventArgs<View> e) {
			base.OnElementChanged(e);

			if(e.OldElement != null) {
				var nativeMap = Control as MKMapView;
				//nativeMap.GetViewForAnnotation = null;
				//nativeMap.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
				//nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
				//nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;

				nativeMap.OverlayRenderer = null;
			}

			if(e.NewElement != null) {
				var formsMap = (TraceMap) e.NewElement;
				var nativeMap = Control as MKMapView;
				customPins = formsMap.ChallengePins;
				//nativeMap.GetViewForAnnotation = GetViewForAnnotation;
				//nativeMap.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
				//nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
				//nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;

				nativeMap.OverlayRenderer = GetOverlayRenderer;

				CLLocationCoordinate2D[] coords = new CLLocationCoordinate2D[formsMap.RouteCoordinates.Count];

				int index = 0;
				foreach(var position in formsMap.RouteCoordinates) {
					coords[index] = new CLLocationCoordinate2D(position.Latitude, position.Longitude);
					index++;
				}

				var routeOverlay = MKPolyline.FromCoordinates(coords);
				// todo fit map over trajectory - nativeMap.MapRectThatFits(routeOverlay.BoundingMapRect);
				nativeMap.AddOverlay(routeOverlay);
			}
		}


		/// <summary>
		/// This method is called when the location of the pin becomes visible on the map.
		/// </summary>
		/// <returns>The view for annotation.</returns>
		/// <param name="mapView">Map view.</param>
		/// <param name="annotation">Annotation.</param>
		MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation) {
			MKAnnotationView annotationView = null;

			if(annotation is MKUserLocation)
				return null;

			var anno = annotation as MKPointAnnotation;
			var customPin = GetCustomPin(anno);
			if(customPin == null) {
				throw new Exception("Custom pin not found");
			}

			annotationView = mapView.DequeueReusableAnnotation(customPin.Id);
			if(annotationView == null) {
				annotationView = new MKAnnotationView(annotation, customPin.Id);
				annotationView.Image = UIImage.FromFile("default_shop.png");
				annotationView.CalloutOffset = new CGPoint(0, 0);
				//annotationView.LeftCalloutAccessoryView = new UIImageView(UIImage.FromFile("green_check.png"));
				//annotationView.RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure);
				//((CustomMKAnnotationView) annotationView).Id = customPin.Id;
				//((CustomMKAnnotationView) annotationView).ImageURL = customPin.ImageURL;
				//((CustomMKAnnotationView) annotationView).Checkpoint = customPin.Checkpoint;
			}
			annotationView.CanShowCallout = true;

			return annotationView;
		}


		void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e) {
			var customView = e.View as CustomMKAnnotationView;
			customPinView = new UIView();

			if(customView.Id == "") {
				customPinView.Frame = new CGRect(0, 0, 200, 84);
				var image = new UIImageView(new CGRect(0, 0, 200, 84));
				image.Image = UIImage.FromFile("default_shop.png"); // TODO get specific checkpoint icon
				customPinView.AddSubview(image);
				customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));
				e.View.AddSubview(customPinView);
			}
		}

		// Called when clicking the 'information' button on the annotation/pin window.
		// TODO navigate to the checkpoint details
		void OnCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e) {
			var customView = e.View as CustomMKAnnotationView;
			//Navigation. customView.Checkpoint
		}


		void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e) {
			if(!e.View.Selected) {
				customPinView.RemoveFromSuperview();
				customPinView.Dispose();
				customPinView = null;
			}
		}


		ChallengePin GetCustomPin(MKPointAnnotation annotation) {
			var position = new Position(annotation.Coordinate.Latitude, annotation.Coordinate.Longitude);
			foreach(var pin in customPins) {
				if(pin.Pin.Position == position) {
					return pin;
				}
			}
			return null;
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