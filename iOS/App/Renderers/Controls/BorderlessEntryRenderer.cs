using System;
using Trace;
using Trace.iOS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(BorderlessEntry), typeof(BorderlessEntryRenderer))]
namespace Trace.iOS {

	public class BorderlessEntryRenderer : EntryRenderer {

		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e) {
			base.OnElementChanged(e);

			if(Control != null) {
				Control.BorderStyle = UIKit.UITextBorderStyle.None;
			}
		}
	}
}
