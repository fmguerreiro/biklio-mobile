using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// A page showing the reward and a button enabling the user to claim the reward.
	/// When the button is pressed, a timer starts to prevent abuse.
	/// </summary>
	public partial class ClaimRewardPage : ContentPage {

		private Challenge challenge;


		public ClaimRewardPage(Challenge challenge) {
			InitializeComponent();
			BindingContext = this.challenge = challenge;
		}


		void claimChallengeOnClick(object sender, System.EventArgs e) {
			challenge.IsClaimed = true;
			SQLiteDB.Instance.SaveItem(challenge);
		}
	}
}
