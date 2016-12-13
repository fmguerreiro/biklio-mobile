using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Trace {
	public partial class SettingsPage : ContentPage {

		public SettingsPage() {
			InitializeComponent();
			//list.Add($"New Spot {DateTime.Now.ToLocalTime()}");

		}

		void OnSettingChanged(object sender, EventArgs e) {
			SQLiteDB.Instance.SaveItem(User.Instance);
		}

		void OnSexChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.Sex = EnumUtil.ParseEnum<Sex>(item);
			SQLiteDB.Instance.SaveItem(User.Instance);
		}

		void OnLanguageChanged(object sender, EventArgs e) {
			var picker = (BindablePicker) sender;
			string item = picker.Items[picker.SelectedIndex];
			User.Instance.Language = EnumUtil.ParseEnum<Language>(item);
			SQLiteDB.Instance.SaveItem(User.Instance);
			// TODO: change text UI for the new language.
		}

		async void OnDeleteCache(object sender, EventArgs args) {
			var action = await DisplayActionSheet("Warning:\n This will delete all trajectories and challenges. They will NOT be saved.", "Back", "Delete");
			switch(action) {
				case ("Delete"): SQLiteDB.Instance.DropAllTables(); break;
				default: return;
			}
		}
	}
}
