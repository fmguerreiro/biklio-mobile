using System;
using System.IO;
using Android.Graphics;

[assembly: Xamarin.Forms.Dependency(typeof(Trace.Droid.ImageResizer))]

namespace Trace.Droid {

	public class ImageResizer : IImageResizer {

		public byte[] ResizeImage(byte[] imageData, float width, float height) {
			//Load the bitmap
			Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
			Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int) width, (int) height, false);

			using(MemoryStream ms = new MemoryStream()) {
				resizedImage.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
				return ms.ToArray();
			}
		}
	}
}
