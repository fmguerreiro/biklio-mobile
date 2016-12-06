using System;
using System.IO;
using Trace.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileSystem))]
namespace Trace.Droid {
	public class FileSystem : IFileSystem {

		public void SaveImage(string filename, byte[] imgArray) {
			string filePath = getFilePath(filename);
			File.WriteAllBytes(filePath, imgArray);
		}


		public void DeleteImage(string filename) {
			string filePath = getFilePath(filename);
			File.Delete(filePath);
		}


		public byte[] LoadImage(string filename) {
			var filePath = getFilePath(filename);
			return File.ReadAllBytes(filePath);
		}


		public bool Exists(string filename) {
			var filePath = getFilePath(filename);
			return File.Exists(filePath);
		}


		private string getFilePath(string filename) {
			var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			var filePath = Path.Combine(documentsPath, filename);
			return filePath;
		}
	}
}