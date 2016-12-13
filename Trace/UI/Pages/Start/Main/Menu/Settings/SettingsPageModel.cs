using System;
using System.Collections.Generic;

namespace Trace {

	public class SettingsPageModel {
		public User User { get; }

		public IEnumerable<String> Sexes { get; }
		public IEnumerable<String> Languages { get; }

		public SettingsPageModel() {
			User = User.Instance;
			Sexes = Enum.GetNames(typeof(Sex));
			Languages = Enum.GetNames(typeof(Language));
		}

	}
}
