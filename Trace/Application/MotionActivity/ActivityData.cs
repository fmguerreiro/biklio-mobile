namespace Trace {
	/// <summary>
	/// Motion data is constantly being received in the background.
	/// In order to not fill the main memory after too much time is passed, it is stored in persistent memory.
	/// </summary>
	public class ActivityData : UserItemBase {

		public int Type { get; set; }
		public int Confidence { get; set; }
		public long Timestamp { get; set; }
	}
}
