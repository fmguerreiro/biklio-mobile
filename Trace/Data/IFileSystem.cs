namespace Trace {

	public interface IFileSystem {

		void SaveImage(string filename, byte[] imgArray);

		byte[] LoadImage(string filename);
	}
}
