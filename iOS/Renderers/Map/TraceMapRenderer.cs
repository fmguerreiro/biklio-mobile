using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using CoreGraphics;
using CoreLocation;
using Foundation;
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
	/// Custom renderer that shows the checkpoints on the map
	/// and draws a polyline to show the user's trajectory (if the map has points recorded by user).
	/// </summary>
	public class TraceMapRenderer : MapRenderer {

		MKMapView map;
		MKPolylineRenderer polylineRenderer;

		UIView customPinView;
		List<CustomPin> customPins;


		protected override void OnElementChanged(ElementChangedEventArgs<View> e) {
			base.OnElementChanged(e);

			if(e.OldElement != null) {
				var nativeMap = Control as MKMapView;
				//nativeMap.GetViewForAnnotation = null;
				nativeMap.CalloutAccessoryControlTapped -= onCalloutAccessoryControlTapped;
				//nativeMap.DidSelectAnnotationView -= OnDidSelectAnnotationView;
				//nativeMap.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;

				nativeMap.OverlayRenderer = null;
			}

			if(e.NewElement != null) {
				var formsMap = (TraceMap) e.NewElement;
				var nativeMap = map = Control as MKMapView;

				nativeMap.ShowsCompass = false;
				if(!formsMap.IsShowingUser) {
					nativeMap.ShowsUserLocation = false;
					nativeMap.UserInteractionEnabled = false;
				}

				customPins = formsMap.CustomPins;
				nativeMap.GetViewForAnnotation = GetViewForAnnotation;
				nativeMap.CalloutAccessoryControlTapped += onCalloutAccessoryControlTapped;
				//nativeMap.DidSelectAnnotationView += OnDidSelectAnnotationView;
				//nativeMap.DidDeselectAnnotationView += OnDidDeselectAnnotationView;

				nativeMap.OverlayRenderer = GetOverlayRenderer;

				CLLocationCoordinate2D[] coords = new CLLocationCoordinate2D[formsMap.RouteCoordinates.Count];

				int index = 0;
				foreach(var position in formsMap.RouteCoordinates) {
					coords[index] = new CLLocationCoordinate2D(position.Latitude, position.Longitude);
					index++;
				}

				// Draw trajectory over map if it has one and zoom on it.
				if(index != 0) {
					var routeOverlay = MKPolyline.FromCoordinates(coords);
					nativeMap.AddOverlay(routeOverlay);
					nativeMap.SetVisibleMapRect(fitRegionToPolyline(routeOverlay), false);
				}
				// Or, show user location, and then remove it again so user can scroll map.
				else {
					EventHandler<MKUserLocationEventArgs> didUpdateUserLocationHandler = null;
					didUpdateUserLocationHandler = (object sender, MKUserLocationEventArgs userLoc) => {
						if(MapPage.ShouldCenterOnUser) {
							Debug.WriteLine($"didUpdateUserLocationHandler");
							nativeMap.CenterCoordinate = userLoc.UserLocation.Coordinate;
						}
						MapPage.ShouldCenterOnUser = true;
						nativeMap.DidUpdateUserLocation -= didUpdateUserLocationHandler;
					};
					nativeMap.DidUpdateUserLocation += didUpdateUserLocationHandler;
				}
			}
		}

		/// <summary>
		/// Called when the annotation gets into view.
		/// Changes the display of the annotation/pin from the system default.
		/// In our case, it replaces it with the checkpoint/shop icon.
		/// </summary>
		/// <returns>The view for annotation.</returns>
		/// <param name="mapView">Map view.</param>
		/// <param name="annotation">Annotation.</param>
		MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation) {
			MKAnnotationView annotationView = null;

			// This is to prevent the user indicator (which is also considered an annotation) from being modified.
			if(!(annotation is MKPointAnnotation)) {
				return null;
			}


			var anno = annotation as MKPointAnnotation;
			var customPin = GetCustomPin(anno);
			if(customPin == null) {
				throw new Exception("Custom pin not found");
			}

			annotationView = mapView.DequeueReusableAnnotation(customPin.Id);

			// Load checkpoint image from URL if it exists, else use the default image.
			UIImage image = null;
			if(!string.IsNullOrEmpty(customPin.Checkpoint.PinLogoPath)) {
				byte[] imageBytes = DependencyService.Get<IFileSystem>().LoadImage(customPin.Checkpoint.PinLogoPath);
				image = UIImage.LoadFromData(NSData.FromArray(imageBytes));
			}
			else {
				// TODO speed this up.
				string mapIcon = "map__" + customPin.Checkpoint.LogoURL.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[1];
				image = UIImage.FromFile(mapIcon);
			}
			//var maxWidth = 20f;
			//var maxHeight = maxWidth;
			//image = maxResizeImage(image, maxWidth, maxHeight);
			if(annotationView == null) {
				annotationView = new MKAnnotationView(annotation, customPin.Id);
				annotationView.Image = image;
				//annotationView.Callout
				annotationView.CalloutOffset = new CGPoint(0, 0);
				annotationView.RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure);

				//((CustomMKAnnotationView) annotationView).Id = customPin.Id;
				//((CustomMKAnnotationView) annotationView).ImageURL = customPin.ImageURL;
				//((CustomMKAnnotationView) annotationView).Checkpoint = customPin.Checkpoint;
			}
			annotationView.CanShowCallout = true;
			image.Dispose();

			return annotationView;
		}

		// TODO
		//void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e) {

		//	if(!(e.View.Annotation is MKPointAnnotation)) {
		//		return;
		//	}

		//	customPinView = new UIView();
		//	customPinView.Frame = new CGRect(0, 0, 200, 84);
		//	var image = new UIImageView(new CGRect(0, 0, 200, 84));
		//	image.Image = UIImage.FromFile("checkpointdetails__star.png");
		//	customPinView.AddSubview(image);
		//	//UIButton.FromType(UIButtonType.DetailDisclosure);
		//	customPinView.Center = new CGPoint(0, -(e.View.Frame.Height + 75));

		//	e.View.CanShowCallout = false;
		//	e.View.AddSubview(customPinView);

		//}

		// Called when clicking the 'information' button on the annotation/pin window.
		void onCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e) {
			var customView = e.View.Annotation as MKPointAnnotation;
			var customPin = GetCustomPin(customView);
			MapPage.MoveToCheckpointDetailsAction(customPin.Checkpoint);
		}


		//void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e) {

		//	if(!e.View.Selected) {
		//		customPinView.RemoveFromSuperview();
		//		customPinView.Dispose();
		//		customPinView = null;
		//	}
		//}


		CustomPin GetCustomPin(MKPointAnnotation annotation) {
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
				polylineRenderer = new MKPolylineRenderer(ObjCRuntime.Runtime.GetNSObject(overlay.Handle) as MKPolyline);
				polylineRenderer.FillColor = UIColor.Red;
				polylineRenderer.StrokeColor = UIColor.Blue;
				polylineRenderer.LineWidth = 3;
				polylineRenderer.Alpha = 0.6f;
			}
			return polylineRenderer;
		}


		public void CenterOnUser() {
			Debug.WriteLine("do i get here=");
			var userLoc = map?.UserLocation?.Coordinate;
			if(userLoc != null) {
				map.SetCenterCoordinate(map.UserLocation.Coordinate, true);
			}
			else {
				Debug.WriteLine("CenterOnUser() -> User location is still null");
			}
			//var userLoc = map.UserLocation;
			//var x = userLoc.Coordinate.Longitude;
			//var y = userLoc.Coordinate.Latitude;
			//var size = 1000; // m?
			//var mapRect = new MKMapRect(x, y, size, size);
			//map.SetVisibleMapRect(mapRect, false);
		}

		/// <summary>
		/// Resize the annotation image to be contained within a maximum width and height, keeping aspect ratio.
		/// </summary>
		/// <returns>The resize image.</returns>
		/// <param name="sourceImage">Source image.</param>
		/// <param name="maxWidth">Max width.</param>
		/// <param name="maxHeight">Max height.</param>
		UIImage maxResizeImage(UIImage sourceImage, float maxWidth, float maxHeight) {
			var sourceSize = sourceImage.Size;
			var maxResizeFactor = Math.Min(maxWidth / sourceSize.Width, maxHeight / sourceSize.Height);
			if(maxResizeFactor > 1) return sourceImage;
			var width = (float) (maxResizeFactor * sourceSize.Width);
			var height = (float) (maxResizeFactor * sourceSize.Height);
			UIGraphics.BeginImageContextWithOptions(new SizeF(width, height), false, 2.0f);
			sourceImage.Draw(new CGRect(0, 0, width, height));
			var resultImage = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			return resultImage;
		}


		/// <summary>
		/// Sets the region to display the entire polyline, along with 12.5% padding on each side.
		/// </summary>
		/// <returns>The region to polyline.</returns>
		/// <param name="polyline">Polyline.</param>
		MKMapRect fitRegionToPolyline(MKPolyline polyline) {
			MKMapRect region = polyline.BoundingMapRect;
			var wPadding = region.Size.Width * 0.25;
			var hPadding = region.Size.Height * 0.25;

			//Add padding to the region
			region.Size.Width += wPadding;
			region.Size.Height += hPadding;

			//Center the region on the line
			region.Origin.X -= wPadding / 2;
			region.Origin.Y -= hPadding / 2;
			return region;
		}
	}
}