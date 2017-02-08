using Foundation;
using UIKit;

namespace Trace.iOS {

	// TODO not used yet, may be useful later for customizing the callout presentation.
	public class CustomCallout : UIViewController, IUIPopoverPresentationControllerDelegate, IUIAdaptivePresentationControllerDelegate {

		public CustomCallout() : base("CustomCallout", null) {
			ModalPresentationStyle = UIModalPresentationStyle.Popover;
			PopoverPresentationController.Delegate = this;
			PreferredContentSize = new CoreGraphics.CGSize(300, 100);
		}

		[Export("adaptivePresentationStyleForPresentationController:traitCollection:")]
		public UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller, UITraitCollection traitCollection) {
			return UIModalPresentationStyle.None;
		}
	}
}