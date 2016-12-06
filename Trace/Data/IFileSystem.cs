namespace Trace {

	public interface IFileSystem {

		void SaveImage(string filename, byte[] imgArray);

		void DeleteImage(string filename);

		byte[] LoadImage(string filename);

		bool Exists(string filename);
	}
}
