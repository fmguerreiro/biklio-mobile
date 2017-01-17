using MapKit;

namespace Trace.iOS {
	/// <summary>
	/// This is just a wrapper for the iOS annotation, adding our custom parameters.
	/// An annotation is Apple's way of saying "pin".
	/// </summary>
	public class CustomMKAnnotationView : MKAnnotationView {
		public string Id { get; set; }
		public Checkpoint Checkpoint { get; set; }

		public CustomMKAnnotationView(IMKAnnotation annotation, string id)
			: base(annotation, id) {
		}
	}
}
