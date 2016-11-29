using SQLite;

namespace Trace {

	public interface ISQLite {

		SQLiteConnection GetConnection();
	}
}
