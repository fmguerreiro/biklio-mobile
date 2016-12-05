using System;
using System.IO;
using Trace.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileSystem))]
namespace Trace.iOS {
	public class FileSystem : IFileSystem {

		public void SaveImage(string filename, byte[] imgArray) {
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var filePath = Path.Combine(documentsPath, filename);
			File.WriteAllBytes(filePath, imgArray);
		}


		public byte[] LoadImage(string filename) {
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var filePath = Path.Combine(documentsPath, filename);
			return File.ReadAllBytes(filePath);
		}
	}
}