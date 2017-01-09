using System;
using System.Collections.Generic;

namespace Trace {

	public class SettingsPageModel {
		public User User { get; }

		public IList<string> Genders { get; }
		public IList<string> Languages { get; }
		public IList<string> Sounds { get; }

		public SettingsPageModel() {
			User = User.Instance;
			Genders = Enum.GetNames(typeof(SelectedGender));
			Languages = Enum.GetNames(typeof(SelectedLanguage));
			Sounds = new string[] {
				"silence.wav",
				"bike_bell.wav",
				"bike_no_pedaling.aiff",
				"bike_bell.wav",
				"clapping.wav"
			};
		}

	}
}
