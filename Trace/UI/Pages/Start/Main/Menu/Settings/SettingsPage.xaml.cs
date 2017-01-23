using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	public partial class SettingsPage : ContentPage {

		public SettingsPage() {
			InitializeComponent();
			soundSettingSwitch.IsToggled = User.Instance.IsBackgroundAudioEnabled;
			//list.Add($"New Spot {DateTime.Now.ToLocalTime()}");
		}

		async void onLogout(object sender, EventArgs e) {
			bool isLogout = await DisplayAlert(Language.Logout, Language.AreYouSure, Language.Yes, Language.No);

			// TODO remove DEBUG_ActivityLog on logout and from mapPage.
			await DisplayAlert("Activity Results", App.DEBUG_ActivityLog, "Ok");
			App.DEBUG_ActivityLog = "";

			if(isLogout) {
				WebServerLoginManager.PrepareLogout();
				//await Task.Delay(1000);
				Application.Current.MainPage = SignInPage.CreateSignInPage();
			}
		}

		// TODO: need to perform input validation -- need a settings model that gets the values first before storing them in user after validation
		void onSettingChanged(object sender, EventArgs e) {
			SQLiteDB.Instance.SaveUser(User.Instance);
		}

		void onGenderChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.Gender = EnumUtil.ParseEnum<SelectedGender>(item);
			SQLiteDB.Instance.SaveUser(User.Instance);
		}

		void onSoundSettingChanged(object sender, ToggledEventArgs e) {
			User.Instance.IsBackgroundAudioEnabled = soundSettingSwitch.IsToggled;
			Debug.WriteLine($"IsBackgroundAudioEnabled = {User.Instance.IsBackgroundAudioEnabled}");
			SQLiteDB.Instance.SaveUser(User.Instance);
			// Restart eligibility state machine: null -> upon next call to Instance, a new Manager is instantiated.
			RewardEligibilityManager.Instance = null;
		}

		void onIneligibleSoundChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			Debug.WriteLine("onIneligibleSoundChanged(): " + item);
			User.Instance.BackgroundIneligibleSoundSetting = item;
			SQLiteDB.Instance.SaveUser(User.Instance);
		}

		void onEligibleSoundChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.BycicleEligibleSoundSetting = item;
			SQLiteDB.Instance.SaveUser(User.Instance);
		}

		void onCongratulatorySoundChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.CongratulatorySoundSetting = item;
			SQLiteDB.Instance.SaveUser(User.Instance);
		}

		void onNoLongerEligibleSoundChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.NoLongerEligibleSoundSetting = item;
			SQLiteDB.Instance.SaveUser(User.Instance);
		}

		async void onDeleteEverything(object sender, EventArgs args) {
			var isDelete = await DisplayAlert(Language.Warning, Language.DeleteEverythingWarning, Language.Delete, Language.Back);
			if(isDelete) {
				Debug.WriteLine("DELETING ALL USER INFO");
				SQLiteDB.Instance.DeleteAllUserItems();
				User.Instance.WSSnapshotVersion = 0;
				SQLiteDB.Instance.SaveUser(User.Instance);
				User.Instance.SubscribedCampaigns = null;
				User.Instance.Challenges = null;
				User.Instance.Checkpoints = null;
				User.Instance.Trajectories = null;
			}
		}

		// TODO OnLicensesClicked
		async void onLicensesClicked(object sender, EventArgs e) {
			await DisplayAlert("Error", "Not yet implemented.", "Ok");
		}

		// TODO OnTermsOfServiceClicked
		async void onTermsOfServiceClicked(object sender, EventArgs e) {
			await DisplayAlert("Error", "Not yet implemented.", "Ok");
		}


		async void onPrivacyPolicyClicked(object sender, EventArgs e) {
			await Navigation.PushModalAsync(new PrivacyPolicyPage());
		}

		// TODO OnAboutUsClicked
		async void onAboutUsClicked(object sender, EventArgs e) {
			await DisplayAlert("Error", "Not yet implemented.", "Ok");
		}
	}
}
