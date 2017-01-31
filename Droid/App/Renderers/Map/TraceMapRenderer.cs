using System;
using System.Collections.Generic;
using System.ComponentModel;
using Android.Content;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Graphics;
using Trace;
using Trace.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(TraceMap), typeof(TraceMapRenderer))]
namespace Trace.Droid {

	/// <summary>
	/// Custom renderer that shows the challenges and draws a polyline on the map to show the user's trajectory.
	/// Example taken from: https://github.com/xamarin/xamarin-forms-samples/blob/master/CustomRenderers/Map/Droid/CustomMapRenderer.cs
	/// </summary>
	public class TraceMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback {

		GoogleMap map;
		List<CustomPin> customPins;
		List<Plugin.Geolocator.Abstractions.Position> routeCoordinates;
		bool isDrawn;

		protected override void OnElementChanged(ElementChangedEventArgs<Map> e) {
			base.OnElementChanged(e);

			if(e.OldElement != null) {
				// Unsubscribe
			}

			if(e.NewElement != null) {
				var formsMap = (TraceMap) e.NewElement;
				routeCoordinates = formsMap.RouteCoordinates;
				customPins = formsMap.CustomPins;
				((MapView) Control).GetMapAsync(this);
			}
		}

		public void OnMapReady(GoogleMap googleMap) {
			map = googleMap;
			map.UiSettings.ZoomControlsEnabled = false;
			map.UiSettings.MyLocationButtonEnabled = false;
			map.UiSettings.CompassEnabled = false;

			// Set Pin event handler to show window on click.
			map.InfoWindowClick += OnInfoWindowClick;
			map.SetInfoWindowAdapter(this);

			// Create and display the trajectory polyline in case there is one.
			var polylineOptions = new PolylineOptions();
			polylineOptions.InvokeColor(0x66FF0000);

			var builder = new LatLngBounds.Builder();
			var nPoints = 0;
			foreach(var position in routeCoordinates) {
				nPoints++;
				var point = new LatLng(position.Latitude, position.Longitude);
				builder.Include(point);
				polylineOptions.Add(point);
			}

			// Center map to contain all points in trajectory.
			if(nPoints > 1) {
				LatLngBounds bounds = builder.Build();
				int padding = 20; // offset from edges of the map in pixels
				CameraUpdate cu = CameraUpdateFactory.NewLatLngBounds(bounds, padding);
				map.AnimateCamera(cu);

				map.AddPolyline(polylineOptions);
			}
			else {
				// Update map to show user location when it's calculated, and then remove it again so user can scroll map.
				EventHandler<GoogleMap.MyLocationChangeEventArgs> didUpdateUserLocationHandler = null;
				didUpdateUserLocationHandler = (object sender, GoogleMap.MyLocationChangeEventArgs userLoc) => {
					System.Diagnostics.Debug.WriteLine($"didUpdateUserLocationHandler");
					map.CameraPosition.Target.Latitude = userLoc.Location.Latitude;
					map.CameraPosition.Target.Longitude = userLoc.Location.Longitude;
					map.MyLocationChange -= didUpdateUserLocationHandler;
				};
				map.MyLocationChange += didUpdateUserLocationHandler;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e) {
			base.OnElementPropertyChanged(sender, e);

			if(e.PropertyName.Equals("VisibleRegion") && !isDrawn) {
				map?.Clear();

				var defaultImage = BitmapDescriptorFactory.FromBitmap(BitmapFactory.DecodeResource(Context.Resources, Resource.Drawable.challengelist__default_shop_20px));
				foreach(CustomPin pin in customPins) {
					var marker = new MarkerOptions();
					marker.SetPosition(new LatLng(pin.Pin.Position.Latitude, pin.Pin.Position.Longitude));
					marker.SetTitle(pin.Pin.Label);
					marker.SetSnippet(pin.Pin.Address);
					if(!string.IsNullOrEmpty(pin.Checkpoint.PinLogoPath)) {
						byte[] imageBytes = DependencyService.Get<IFileSystem>().LoadImage(pin.Checkpoint.PinLogoPath);
						var bitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
						marker.SetIcon(BitmapDescriptorFactory.FromBitmap(bitmap));
						//marker.SetIcon(BitmapDescriptorFactory.FromPath(imagePath));
					}
					else {
						marker.SetIcon(defaultImage);
					}
					try {
						map?.AddMarker(marker);
					}
					catch(Java.Lang.NullPointerException npE) {
						System.Diagnostics.Debug.WriteLine("Error drawing pin: " + npE.Message);
					}
				}
				isDrawn = true;
			}
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b) {
			base.OnLayout(changed, l, t, r, b);
			isDrawn &= !changed;
		}

		void OnInfoWindowClick(object sender, GoogleMap.InfoWindowClickEventArgs e) {
			var customPin = GetCustomPin(e.Marker);
			if(customPin == null) {
				throw new Exception("Custom pin not found");
			}

			//if(!string.IsNullOrWhiteSpace(customPin.ImageURL)) {
			//	var url = Android.Net.Uri.Parse(customPin.ImageURL);
			//	var intent = new Intent(Intent.ActionView, url);
			//	intent.AddFlags(ActivityFlags.NewTask);
			//	Android.App.Application.Context.StartActivity(intent);
			//}
		}

		public Android.Views.View GetInfoContents(Marker marker) {
			//var inflater = Android.App.Application.Context.GetSystemService(Context.LayoutInflaterService) as Android.Views.LayoutInflater;
			//if(inflater != null) {
			//	Android.Views.View view;

			//	var customPin = GetCustomPin(marker);
			//	if(customPin == null) {
			//		throw new Exception("Custom pin not found");
			//	}

			//	if(customPin.Id == "Xamarin") {
			//		view = inflater.Inflate(Resource.Layout.XamarinMapInfoWindow, null);
			//	}
			//	else {
			//		view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);
			//	}

			//	var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
			//	var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);

			//	if(infoTitle != null) {
			//		infoTitle.Text = marker.Title;
			//	}
			//	if(infoSubtitle != null) {
			//		infoSubtitle.Text = marker.Snippet;
			//	}

			//	return view;
			//}
			return null;
		}

		public Android.Views.View GetInfoWindow(Marker marker) {
			return null;
		}

		CustomPin GetCustomPin(Marker annotation) {
			foreach(var pin in customPins) {
				if(pin.Id == annotation.Id) {
					return pin;
				}
			}
			return null;
		}
	}
}