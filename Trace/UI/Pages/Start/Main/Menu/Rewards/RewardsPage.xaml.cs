using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// The rewards page shows the list of completed challenges.
	/// </summary>
	public partial class RewardsPage : ContentPage {
		public RewardsPage() {
			InitializeComponent();

			IList<Challenge> rewards = SQLiteDB.Instance.GetRewards().ToList();
			var testReward = User.Instance.Challenges.FirstOrDefault();
			rewards.Add(testReward);
			BindingContext = new RewardVM { Rewards = rewards };
		}

		/// <summary>
		/// Show the claim reward page.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">The challenge in the listview that was clicked.</param>
		void OnSelection(object sender, SelectedItemChangedEventArgs e) {
			var challenge = ((Challenge) e.SelectedItem);
			if(challenge != null) {
				Navigation.PushAsync(new ClaimRewardPage(challenge));
			}
		}
	}
}
