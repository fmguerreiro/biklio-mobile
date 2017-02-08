using System;
using System.IO;
using Trace.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(Trace.Droid.SQLite))]
namespace Trace.Droid {

	public class SQLite : ISQLite {

		public global::SQLite.SQLiteConnection GetConnection() {
			var sqliteFilename = "TraceSQLite.db3";
			string documentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal); // Documents folder
			var path = Path.Combine(documentsPath, sqliteFilename);

			// Create the connection
			var conn = new global::SQLite.SQLiteConnection(path);

			// Return the database connection
			return conn;
		}
	}
}