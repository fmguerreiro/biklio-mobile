using System.Diagnostics;
using Trace.Localization;
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
			this.challenge = challenge;
			BindingContext = new ClaimRewardModel { Challenge = challenge };
			Debug.WriteLine($"ClaimRewardPage-> isRepeatable: {challenge.IsRepeatable}, isClaimed: {challenge.IsClaimed}");
			// If the challenge has already claimed, do not show the claim button, show the grid.
			if(challenge.IsClaimed) {
				claimRewardButton.IsVisible = false;
				claimRewardGrid.IsVisible = true;
			}
		}


		async void claimChallengeOnClick(object sender, System.EventArgs e) {
			if(challenge.IsClaimed) {
				await DisplayAlert(Language.Error, Language.RewardAlreadyClaimedError, Language.Ok);
				return;
			}
			challenge.IsClaimed = true;
			SQLiteDB.Instance.SaveItem(challenge);

			// Remove button & display grid with timestamps.
			claimRewardButton.IsVisible = false;
			claimRewardGrid.IsVisible = true;
		}
	}
}
