using SQLite;

namespace Trace {

	public interface SQLiteInterface {

		SQLiteConnection GetConnection();
	}
}
