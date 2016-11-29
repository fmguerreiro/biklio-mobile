using System;
using System.IO;
using Trace.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(Trace.iOS.SQLite))]
namespace Trace.iOS {

	public class SQLite : ISQLite {

		public global::SQLite.SQLiteConnection GetConnection() {
			var sqliteFilename = "TraceSQLite.db3";
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
			string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
			var path = Path.Combine(libraryPath, sqliteFilename);

			// Create the connection
			var conn = new global::SQLite.SQLiteConnection(path);

			// Return the database connection
			return conn;
		}
	}
}
