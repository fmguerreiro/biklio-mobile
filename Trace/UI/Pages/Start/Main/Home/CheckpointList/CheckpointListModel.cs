using System.Collections.Generic;
using System.ComponentModel;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// The CheckpointListModel is used to display the list of checkpoints in the CheckpointListPage. 
	/// It also has a header 'Summary' showing a message telling the user how many checkpoints were found.
	/// </summary>
	public class CheckpointListModel {
		public IList<CheckpointViewModel> Checkpoints { get; set; }
		public string Summary {
			get {
				int count = Checkpoints.Count;
				if(count == 0) {
					return Language.NoCheckpointsFound;
				}
				if(count != 1)
					return Language.ThereAre + " " + count + " " + Language.CheckpointsNear;
				else
					return Language.OneCheckpointFound;
			}
		}
	}

	/// <summary>
	/// The CheckpointViewModel is used to display the checkpoint information in the CheckpointListPage.
	/// When the checkpoint is favorited/unfavorited in the CheckpointDetailsPage, issue an event to modify the background color 
	/// in the CheckpointListPage.
	/// </summary>
	public class CheckpointViewModel : INotifyPropertyChanged {

		public event PropertyChangedEventHandler PropertyChanged;

		public Checkpoint Checkpoint { get; }

		bool isUserFavorite;
		public bool IsUserFavorite {
			set {
				if(isUserFavorite != value) {
					isUserFavorite = value;
					Checkpoint.IsUserFavorite = value;
					if(PropertyChanged != null) {
						PropertyChanged(this, new PropertyChangedEventArgs("BackgroundColor"));
					}
				}
			}
			get {
				return isUserFavorite;
			}
		}

		public CheckpointViewModel(Checkpoint checkpoint) {
			Checkpoint = checkpoint;
			isUserFavorite = checkpoint.IsUserFavorite;
		}

		public string FavoriteImage {
			get {
				if(IsUserFavorite)
					return "checkpointdetails__star.png";
				return "checkpointdetails__star_outline.png";
			}
		}

		public Color BackgroundColor {
			get {
				if(IsUserFavorite)
					return (Color) Application.Current.Resources["PrimaryLightColor"];
				return Color.White;
			}
		}
	}
}
