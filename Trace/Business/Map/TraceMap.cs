using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Trace {

	/// <summary>
	/// The Map for our custom application.
	/// It stores the challenge information and the user position over time 
	/// which are displayed by platform-specific renderers.
	/// </summary>
	public class TraceMap : Map {

		public List<Position> RouteCoordinates { get; set; }
		public List<ChallengePin> ChallengePins { get; set; }

		public TraceMap() {
			RouteCoordinates = new List<Position>();
			ChallengePins = new List<ChallengePin>();
		}
	}
}