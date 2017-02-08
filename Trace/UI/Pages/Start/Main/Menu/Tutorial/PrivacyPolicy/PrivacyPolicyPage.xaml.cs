using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// A simple page that shows all the details (in a carousel fashion) of the privacy policy.
	/// </summary>
	public partial class PrivacyPolicyPage : ContentPage {
		PrivacyPolicyModel lastPart;

		public PrivacyPolicyPage() {
			InitializeComponent();

			var policyParts = new List<PrivacyPolicyModel> {
				new PrivacyPolicyModel { Text = Language.PrivacyPolicyPart1, Indicator = "1/4" },
				new PrivacyPolicyModel { Text = Language.PrivacyPolicyPart2, Indicator = "2/4" },
				new PrivacyPolicyModel { Text = Language.PrivacyPolicyPart3, Indicator = "3/4" },
				new PrivacyPolicyModel { Text = Language.PrivacyPolicyPart4, Indicator = "4/4" }
			};

			lastPart = policyParts.Last();

			policyCarouselView.ItemsSource = policyParts;
		}


		// Show a 'finish' button when the user reaches the last page of the privacy policy.		
		void onPolicyPartChanged(object sender, SelectedItemChangedEventArgs e) {
			var selectedPart = (PrivacyPolicyModel) e.SelectedItem;
			if(selectedPart == lastPart) {
				confirmationButton.IsVisible = true;
			}
		}


		async void onConfirmation(object sender, EventArgs e) {
			var arePermissionsGranted = await requestPermissions();
			if(arePermissionsGranted)
				await Navigation.PopModalAsync();
		}


		async Task<bool> requestPermissions() {

			try {
				// Get Location permission.
				var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
				var res = true;
				if(status != PermissionStatus.Granted) {
					if(await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Location)) {
						await DisplayAlert("Need location", "Gunna need that location son", "OK");
					}

					var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Location });
					Debug.WriteLine($"Location permission: {results[Permission.Location]}");
					res &= results[Permission.Location] == PermissionStatus.Granted;
				}

				// Get Storage permission.
				status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
				if(status != PermissionStatus.Granted) {
					if(await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Storage)) {
						await DisplayAlert("Need storage", "Gunna need that sweet storage space son", "OK");
					}

					var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Storage });
					Debug.WriteLine($"Location permission: {results[Permission.Storage]}");
					res &= results[Permission.Storage] == PermissionStatus.Granted;
				}

				return res;
			}
			catch(Exception ex) {
				Debug.WriteLine(ex.ToString()); return false;
			}
		}
	}
}
