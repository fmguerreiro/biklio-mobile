using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// Page showing the user's currently registered campaigns, as well as a new campaign.
	/// The new campaign is the closest one based on the user's current location.
	/// NOTE: This page uses custom viewcells with no optimization because the cells shown will be very few (2 or 3 at most).
	///       Do not copy this for longer lists (such as in the ChallengePage).
	///       Instead, use a custom renderer with cell-reuse caching: https://developer.xamarin.com/guides/xamarin-forms/custom-renderer/viewcell/
	/// </summary>
	public partial class CampaignsPage : ContentPage {

		public ObservableCollection<Campaign> campaigns { get; set; }

		public CampaignsPage(ObservableCollection<Campaign> collection) {
			InitializeComponent();
			listView.ItemsSource = campaigns = collection;
		}


		public void UpdateCampaignList(Campaign campaign) {
			Device.BeginInvokeOnMainThread(() => {
				campaigns.Add(campaign);
			});
		}


		async void onCampaignClicked(object sender, SelectedItemChangedEventArgs e) {
			var campaign = e.SelectedItem as Campaign;
			listView.SelectedItem = null;

			bool isNewCampaignPage = !campaigns.Equals(User.Instance.SubscribedCampaigns);

			bool isAffirmative = false;
			// Show UI dialog asking if the user wants to (un)subscribe the campaign.
			if(isNewCampaignPage) {
				isAffirmative = await DisplayAlert(Language.NewCampaign, Language.NewCampaignMsg, Language.Yes, Language.No);
			}
			else {
				isAffirmative = await DisplayAlert(Language.UnsubscribeCampaign, Language.UnsubscribeCampaignMsg, Language.Yes, Language.No);
			}

			// Send request to webserver.
			if(isAffirmative) {
				var webserverClient = new WebServerClient();

				WSResult result = null;
				if(isNewCampaignPage) {
					result = await webserverClient.SubscribeCampaign(campaign.GId);
					if(result.success) {
						await DisplayAlert(Language.Result, Language.YouHaveSubscribedTo + " " + campaign.Name + ".", Language.Ok);
						// Delete from new list and insert into subscribed list.
						campaigns.Remove(campaign);
						campaign.UserId = User.Instance.Id;
						User.Instance.SubscribedCampaigns.Add(campaign);
						SQLiteDB.Instance.SaveItem(campaign);
						return;
					}
				}
				else {
					result = await webserverClient.UnsubscribeCampaign(campaign.GId);
					if(result.success) {
						await DisplayAlert(Language.Result, Language.YouHaveUnsubscribedFrom + " " + campaign.Name + ".", Language.Ok);
						// Delete from new list and insert into subscribed list.
						campaigns.Remove(campaign);
						SQLiteDB.Instance.DeleteItem<Campaign>(campaign.Id);
						return;
					}
				}

				await DisplayAlert(Language.Error, result.error, Language.Ok);
			}
		}
	}
}
