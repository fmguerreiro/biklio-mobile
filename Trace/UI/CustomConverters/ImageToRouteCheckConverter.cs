using System;
using System.Globalization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// This class is called on the MyTrajectoriesPage to show a visual green mark next to trajectories
	/// that were successfully sent to the Web Server.
	/// </summary>
	public class ImageToRouteCheckConverter : IValueConverter {

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			ImageSource retSource = null;
			var wasTrackSent = (bool) value;
			if(wasTrackSent) {
				retSource = ImageSource.FromFile("images/mytrajectories_list/green_check.png");
			}
			else {
				retSource = ImageSource.FromFile("images/mytrajectories_list/cross_error.png");
			}
			return retSource;
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}
	}
}