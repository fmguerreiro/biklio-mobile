using System.Collections.Generic;
using Xamarin.Forms.Maps;

namespace Trace {

	/// <summary>
	/// The Map for our custom application.
	/// It stores the challenge information and the user position over time 
	/// which are displayed by platform-specific renderers.
	/// </summary>
	public class TraceMap : Map {

		private List<Plugin.Geolocator.Abstractions.Position> routeCoordinates;
		public List<Plugin.Geolocator.Abstractions.Position> RouteCoordinates {
			get {
				if(routeCoordinates == null) {
					routeCoordinates = new List<Plugin.Geolocator.Abstractions.Position>();
				}
				return routeCoordinates;
			}
			set { routeCoordinates = value ?? new List<Plugin.Geolocator.Abstractions.Position>(); }
		}

		public List<CustomPin> CustomPins { get; set; }

		public TraceMap() {
			//RouteCoordinates = new List<Plugin.Geolocator.Abstractions.Position>();
			CustomPins = new List<CustomPin>();
		}
	}
}