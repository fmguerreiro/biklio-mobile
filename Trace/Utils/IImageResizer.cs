namespace Trace {

	/// <summary>
	/// A simple dependency service that can downsample images.
	/// </summary>
	public interface IImageResizer {

		byte[] ResizeImage(byte[] imageData, float width, float height);
	}
}