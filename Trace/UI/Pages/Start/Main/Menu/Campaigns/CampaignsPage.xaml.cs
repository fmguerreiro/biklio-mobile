using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

		public CampaignsPage() {
			InitializeComponent();
			listView.ItemsSource = campaigns = new ObservableCollection<Campaign>(User.Instance.SubscribedCampaigns);
		}


		async void removeCampaignOnClick(object sender, SelectedItemChangedEventArgs e) {
			var campaign = e.SelectedItem as Campaign;
			listView.SelectedItem = null;

			// Show UI dialog asking if the user wants to (un)subscribe the campaign.
			bool isAffirmative = await DisplayAlert(Language.UnsubscribeCampaign, Language.UnsubscribeCampaignMsg, Language.Yes, Language.No);

			// Send request to webserver.
			if(isAffirmative) {
				var webserverClient = new WebServerClient();

				WSResult result = await webserverClient.UnsubscribeCampaign(campaign.GId);
				if(result.success) {
					await DisplayAlert(Language.Result, Language.YouHaveUnsubscribedFrom + " " + campaign.Name + ".", Language.Ok);
					campaigns.Remove(campaign);
					User.Instance.SubscribedCampaigns.Remove(campaign);
					SQLiteDB.Instance.DeleteItem<Campaign>(campaign.Id);
				}
				else {
					await DisplayAlert(Language.Error, result.error, Language.Ok);
				}
			}
		}


		async void fetchNewCampaignOnClick(object sender, EventArgs e) {
			var webserverClient = new WebServerClient();
			var result = await webserverClient.GetNearestCampaign();
			if(result.success) {
				var payload = result.payload;
				Campaign newCampaign = createCampaign(payload);

				// Check to see if the user already has this campaign before displaying.
				var subbedCampaign = User.Instance.SubscribedCampaigns.FirstOrDefault((c) => c.GId == newCampaign.GId);
				if(subbedCampaign != null) {
					await DisplayAlert(subbedCampaign.Name, Language.AlreadySubscribedError, Language.Ok);
					return;
				}

				// TODO popup with img: https://github.com/rotorgames/Rg.Plugins.Popup
				var wantsToSubscribe = await DisplayAlert(newCampaign.Name, newCampaign.Description + "\n" + Language.NewCampaignMsg, Language.Subscribe, Language.Cancel);
				if(wantsToSubscribe) {
					// Let WS know that user wants to subscribe.
					result = await webserverClient.SubscribeCampaign(newCampaign.GId);
					if(result.success) {
						await DisplayAlert(Language.Result, Language.YouHaveSubscribedTo + " " + newCampaign.Name + ".", Language.Ok);
						newCampaign.UserId = User.Instance.Id;
						campaigns.Add(newCampaign);
						User.Instance.SubscribedCampaigns.Add(newCampaign);
						SQLiteDB.Instance.SaveItem(newCampaign);
					}
					else {
						await DisplayAlert(Language.Error, result.error, Language.Ok);
					}
				}
			}
		}



		Campaign createCampaign(WSPayload payload) {
			return new Campaign {
				GId = Convert.ToInt64(payload.id),
				Name = payload.name,
				IsSubscribed = false,
				Website = payload.website,
				Start = payload.start,
				End = payload.end,
				Description = payload.description,
				ImageURL = payload.image,
				//NElongitude = (float) payload.bounds.northeast.longitude, TODO execution hangs here and never returns
				//NElatitude = (float) payload.bounds.northeast.latitude,
				//SWlongitude = (float) payload.bounds.southwest.longitude,
				//SWlatitude = (float) payload.bounds.southwest.latitude
			};
		}
	}
}
