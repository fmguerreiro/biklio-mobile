using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// A page showing the reward and a button enabling the user to claim the reward.
	/// When the button is pressed, a timer starts to prevent abuse.
	/// </summary>
	public partial class ClaimRewardPage : ContentPage {

		public ClaimRewardPage(Challenge challenge) {
			InitializeComponent();
			BindingContext = challenge;
		}
	}
}
