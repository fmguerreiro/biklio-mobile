using System;
using System.Collections.Generic;

namespace Trace {

	public class SettingsPageModel {
		public User User { get; }

		public IList<string> Genders { get; }
		public IList<string> Sounds { get; }

		// TODO use dict to show user-friendly names for sound files
		//public Dictionary<string, string> SoundDict = new Dictionary<string, string> {
		//	{ "Silence", "silence.wav" },
		//	{ "Bike bell", "bike_bell.wav" },
		//	{ "Bike chain", "bike_no_pedaling.aiff" },
		//	{ "Clapping", "clapping.wav" }
		//};

		public SettingsPageModel() {
			User = User.Instance;
			Genders = Enum.GetNames(typeof(SelectedGender));
			Sounds = new string[] {
				"silence.wav",
				"bike_bell.wav",
				"bike_no_pedaling.aiff",
				"clapping.wav"
			};
		}

	}
}
