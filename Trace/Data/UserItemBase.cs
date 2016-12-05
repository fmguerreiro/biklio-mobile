using SQLite;

namespace Trace {

	/// <summary>
	/// All objects associated with a user, such as challenges, checkpoints and trajectories derive from this.
	/// Allows the abstraction and simplification of database access functions.
	/// </summary>
	public class UserItemBase : DatabaseEntityBase {

		public long UserId { get; set; }

		// Id used by the Webserver.
		[Indexed]
		public long GId { get; set; }
	}
}
