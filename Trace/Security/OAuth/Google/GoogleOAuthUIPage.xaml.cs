using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

namespace Trace {
	public partial class GoogleOAuthUIPage : ContentPage {

		public GoogleOAuthUIPage() {
			InitializeComponent();
			if(Device.OS == TargetPlatform.Android) { loadingWheel.Scale = 0.5; }
		}
	}
}
