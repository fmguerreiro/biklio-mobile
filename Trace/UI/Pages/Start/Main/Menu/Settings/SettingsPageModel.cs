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
				"silence.mp3",
				"bike_bell.mp3",
				"bike_no_pedaling.mp3",
				"bike_pedal.mp3",
				"clapping.mp3",
				"forest_birds.mp3",
				"light_rain.mp3",
				"running_leaves.mp3",
				"running_pavement.mp3",
				"spaceship_idle.mp3",
				"walking_grass.mp3",
				"walking_pavement.mp3"
			};
		}

	}
}
