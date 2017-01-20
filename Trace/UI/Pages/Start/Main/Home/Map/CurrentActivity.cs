using System.ComponentModel;

namespace Trace {
	/// <summary>
	/// Used for displaying the current activity in real-time on the map screen.
	/// Mainly for debugging, but left here if that feature is requested.
	/// The XAML code for displaying the activity text is:
	//<Label
	//x:Name="currentActivityLabel"
	//Text="{Binding LocalizedActivity}"
	//HorizontalTextAlignment="Center"
	//Margin="0"
	//IsVisible="true"
	//BackgroundColor="{StaticResource SecondaryColor}"
	//Grid.Row="0"
	//Grid.Column="0"
	//Grid.ColumnSpan="2"
	//TextColor="{StaticResource SecondaryTextColor}"
	//VerticalTextAlignment="Center">
	//</Label>
	//<Label.BindingContext>
	// <locals:CurrentActivity />
	//</Label.BindingContext>
	/// </summary>
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
