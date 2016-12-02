using Xamarin.Forms.Maps;

namespace Trace {
	/// <summary>
	/// Our pins which are customized to display specific information on the map.
	/// </summary>
	public class ChallengePin {

		public Pin Pin { get; set; }
		public string Id { get; set; }
		public string ImageURL { get; set; }
		public Checkpoint Checkpoint { get; set; }
	}
}
