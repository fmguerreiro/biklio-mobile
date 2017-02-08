using System;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(Trace.Droid.SQLiteConnector))]
namespace Trace.Droid {

	public class SQLiteConnector : ISQLite {

		public global::SQLite.SQLiteConnection GetConnection() {
			var sqliteFilename = "TraceSQLite.db3";
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
			var path = Path.Combine(documentsPath, sqliteFilename);

			// Create the connection
			var conn = new global::SQLite.SQLiteConnection(path);

			// Return the database connection
			return conn;
		}
	}
}