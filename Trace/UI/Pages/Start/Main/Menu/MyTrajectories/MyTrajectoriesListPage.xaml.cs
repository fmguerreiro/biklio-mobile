using System.Linq;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// Page displaying the list of trajectories that the user produced.
	/// An image is displayed next to each item, indicating whether the trajectory was uploaded to the server or not.
	/// </summary>
	public partial class MyTrajectoriesListPage : ContentPage {

		public MyTrajectoriesListPage() {
			InitializeComponent();
			BindingContext = new MyTrajectoriesViewModel {
				Trajectories = User.Instance.Trajectories.Select((x) => new TrajectoryViewModel(x)).ToList()
			};
		}


		/// <summary>
		/// Show the details of the specified trajectory on click.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">The trajectory in the listview that was clicked.</param>
		void onTapped(object sender, ItemTappedEventArgs e) {
			((ListView) sender).SelectedItem = null; // Disable the visual selection state.
			TrajectoryViewModel trajectory = ((TrajectoryViewModel) e.Item);
			Navigation.PushAsync(new TrajectoryDetailsPage(trajectory));
		}
	}
}
