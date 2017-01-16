using System.ComponentModel;

namespace Trace {
	class CurrentActivity : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		ActivityType activityType;
		public ActivityType ActivityType {
			set {
				if(activityType != value) {
					activityType = value;
					if(PropertyChanged != null) {
						PropertyChanged(this, new PropertyChangedEventArgs("LocalizedActivity"));
					}
				}
			}
			get {
				return activityType;
			}
		}

		public string LocalizedActivity { get { return activityType.ToLocalizedString(); } }
	}
}
