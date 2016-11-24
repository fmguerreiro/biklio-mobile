using System;
using System.IO;
using Trace.iOS;
using Xamarin.Forms;

[assembly: Dependency(typeof(SQLite_iOS))]
namespace Trace.iOS {

	public class SQLite_iOS : SQLiteInterface {

		public SQLite.SQLiteConnection GetConnection() {
			var sqliteFilename = "TraceSQLite.db3";
			string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal); // Documents folder
			string libraryPath = Path.Combine(documentsPath, "..", "Library"); // Library folder
			var path = Path.Combine(libraryPath, sqliteFilename);

			// Create the connection
			var conn = new SQLite.SQLiteConnection(path);

			// Return the database connection
			return conn;
		}
	}
}
