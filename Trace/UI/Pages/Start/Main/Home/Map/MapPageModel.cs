namespace Trace {

	/// <summary>
	/// Model used to display trajectory information in the grid display after the tracking finishes.
	/// </summary>
	public class MapPageModel {
		public string MainActivity { get; set; }

		public int Calories { get; set; }

		public int Distance { get; set; }

		public string Duration { get; set; }

		public float AvgSpeed { get; set; }
	}
}
