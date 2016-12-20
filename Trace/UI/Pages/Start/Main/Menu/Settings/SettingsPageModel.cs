using System;
using System.Collections.Generic;

namespace Trace {

	public class SettingsPageModel {
		public User User { get; }

		public IEnumerable<String> Genders { get; }
		public IEnumerable<String> Languages { get; }

		public SettingsPageModel() {
			User = User.Instance;
			Genders = Enum.GetNames(typeof(SelectedGender));
			Languages = Enum.GetNames(typeof(SelectedLanguage));
		}

	}
}
