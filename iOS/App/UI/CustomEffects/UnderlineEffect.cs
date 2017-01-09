using System;
using System.Diagnostics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using Trace;

[assembly: ResolutionGroupName(UnderlineEffect.EffectNamespace)]
[assembly: ExportEffect(typeof(UnderlineEffect), nameof(UnderlineEffect))]

namespace Trace.iOS {

	public class UnderlineEffect : PlatformEffect {

		protected override void OnAttached() {
			SetUnderline(true);
		}

		protected override void OnDetached() {
			SetUnderline(false);
		}

		protected override void OnElementPropertyChanged(System.ComponentModel.PropertyChangedEventArgs args) {
			base.OnElementPropertyChanged(args);

			if(args.PropertyName == Label.TextProperty.PropertyName || args.PropertyName == Label.FormattedTextProperty.PropertyName) {
				SetUnderline(true);
			}
		}

		private void SetUnderline(bool underlined) {
			try {
				var label = (UILabel) Control;
				var text = (NSMutableAttributedString) label.AttributedText;
				var range = new NSRange(0, text.Length);

				if(underlined) {
					text.AddAttribute(UIStringAttributeKey.UnderlineStyle, NSNumber.FromInt32((int) NSUnderlineStyle.Single), range);
				}
				else {
					text.RemoveAttribute(UIStringAttributeKey.UnderlineStyle, range);
				}
			}
			catch(Exception ex) {
				Debug.WriteLine("Cannot underline Label. Error: ", ex.Message);
			}
		}
	}
}