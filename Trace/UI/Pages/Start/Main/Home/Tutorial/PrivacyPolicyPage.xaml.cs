using System.Collections.Generic;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// A simple page that shows all the details (in a carousel fashion) of the privacy policy.
	/// </summary>
	public partial class PrivacyPolicyPage : ContentPage {
		public PrivacyPolicyPage() {
			InitializeComponent();

			var policyParts = new List<PrivacyPolicyDataModel> {
				new PrivacyPolicyDataModel { Text = Language.PrivacyPolicyPart1, Indicator = "1/4" },
				new PrivacyPolicyDataModel { Text = Language.PrivacyPolicyPart2, Indicator = "2/4" },
				new PrivacyPolicyDataModel { Text = Language.PrivacyPolicyPart3, Indicator = "3/4" },
				new PrivacyPolicyDataModel { Text = Language.PrivacyPolicyPart4, Indicator = "4/4" }
			};

			policyCarouselView.ItemsSource = policyParts;
		}


		async void onConfirmation(object sender, System.EventArgs e) {
			await Navigation.PopModalAsync();
		}
	}
}
