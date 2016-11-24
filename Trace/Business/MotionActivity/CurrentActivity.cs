using System.ComponentModel;

namespace Trace {
	class CurrentActivity : INotifyPropertyChanged {

		ActivityType activityType;

		public event PropertyChangedEventHandler PropertyChanged;

		public ActivityType ActivityType {
			set {
				if(activityType != value) {
					activityType = value;
					if(PropertyChanged != null) {
						PropertyChanged(this, new PropertyChangedEventArgs("ActivityType"));
					}
				}
			}
			get {
				return activityType;
			}
		}
	}
}
