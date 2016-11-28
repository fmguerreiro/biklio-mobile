using SQLite;

namespace Trace {

	/// <summary>
	/// Class from which all objects in the database derive.
	/// </summary>
	public abstract class DatabaseEntityBase {

		[PrimaryKey, AutoIncrement]
		public long Id { get; set; }
	}
}
