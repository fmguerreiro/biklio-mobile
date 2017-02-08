using Xamarin.Forms;

namespace Trace {

	public partial class FacebookOAuthUIPage : ContentPage {

		/// <summary>
		///  A custom renderer that is used to display the authentication UI through OAuth Webkit.
		/// </summary>
		public FacebookOAuthUIPage() {
			InitializeComponent();
			if(Device.OS == TargetPlatform.Android) { loadingWheel.Scale = 0.5; }
		}
	}
}
