using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// This class is called on the Challenges page list to display the stored checkpoint image for that challenge.
	/// In case no image exists, a default image is shown.
	/// </summary>
	public class ByteArrayToChallengeImageConverter : IValueConverter {


		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
			ImageSource retSource = null;
			var imagePath = (string) value;
			Debug.WriteLine("ByteArrayToImageConverter: imgPath=" + imagePath);
			if(imagePath != null) {
				var imageAsBytes = DependencyService.Get<IFileSystem>().LoadImage(imagePath);
				retSource = ImageSource.FromStream(() => new MemoryStream(imageAsBytes));
			}
			else {
				retSource = ImageSource.FromFile("default_shop.png");
			}
			return retSource;
		}


		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
			return null;
		}
	}
}