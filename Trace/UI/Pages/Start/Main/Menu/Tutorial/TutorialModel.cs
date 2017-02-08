using System.Collections.Generic;

namespace Trace {

	/// <summary>
	/// The model for the data displayed on the Tutorial Page.
	/// The page will display a list of TutorialParts, where each part holds
	/// an image, an indicator for the page the user is in and its background color.
	/// In addition, each part holds a title at the top of the page, and a longer description near the bottom.
	/// </summary>
	public class TutorialModel {

		public IList<TutorialPart> Parts { get; set; }
	}

	public class TutorialPart {

		public string ImagePath { get; set; }
		public string Indicator { get; set; }
		public string Color { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}
}
