using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// The main campaign page showing two tabs. 
	/// One for the new campaigns, and another for the currently registered campaigns.
	/// </summary>
	public class CampaignsMainPage : TabbedPage {

		CampaignsPage newCampaignsPage;
		CampaignsPage curCampaignsPage;

		public CampaignsMainPage() {
			Title = Language.Campaigns;

			// Add tabs to the page.
			newCampaignsPage = new CampaignsPage(new ObservableCollection<Campaign>());
			curCampaignsPage = new CampaignsPage(User.Instance.SubscribedCampaigns);
			newCampaignsPage.Title = Language.NewCampaigns;
			curCampaignsPage.Title = Language.CurrentCampaigns;
			newCampaignsPage.Icon = "new_campaign.png";
			curCampaignsPage.Icon = "subscribed_campaigns.png";
			Children.Add(newCampaignsPage);
			Children.Add(curCampaignsPage);
			Task.Run(fetchNewCampaign);
		}


		async Task fetchNewCampaign() {
			var webserverClient = new WebServerClient();
			var result = await webserverClient.GetNearestCampaign();
			if(result.success) {
				var payload = result.payload;
				Campaign newCampaign = createCampaign(payload);

				// Check to see if the user already has this campaign before displaying.
				if(User.Instance.SubscribedCampaigns.SingleOrDefault((c) => c.GId == newCampaign.GId) != null)
					return;

				newCampaignsPage.UpdateCampaignList(newCampaign);
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