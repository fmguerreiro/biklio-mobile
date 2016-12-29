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
			BindingContext = new ChallengeVM { Challenges = rewards };
		}

		/// <summary>
		/// Show the detailed page of the challenge's checkpoint on click.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">The challenge in the listview that was clicked.</param>
		void OnSelection(object sender, SelectedItemChangedEventArgs e) {
			Checkpoint checkpoint = ((Challenge) e.SelectedItem).ThisCheckpoint;
			if(checkpoint != null) {
				Navigation.PushAsync(new CheckpointDetailsPage(checkpoint));
			}
			else {
				DisplayAlert("Error", "That challenge does not have an associated checkpoint. Probably a DB consistency issue, please report", "Ok");
			}
		}
	}
}
