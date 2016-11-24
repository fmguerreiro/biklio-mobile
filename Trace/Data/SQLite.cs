using System.Collections.Generic;
using SQLite;
using Xamarin.Forms;
using System.Linq;

namespace Trace {
	public class TRACEStore {

		private readonly SQLiteConnection database;
		static object locker = new object();

		/// <summary>
		/// Initializes a new instance of the Database. 
		/// if the database doesn't exist, it will create the database and all the tables.
		/// </summary>
		public TRACEStore() {
			database = DependencyService.Get<SQLiteInterface>().GetConnection();
			database.CreateTable<User>();
		}

		public IEnumerable<User> GetUser() {
			lock(locker) {
				//var user =
				//	from i in database.Table<User>()
				//	where i.Username == User.Username
				//	select i;
				return null;
				//return (from i in database.Table<User>() select i).ToList();
			}
		}
		//public IEnumerable<TodoItem> GetItemsNotDone() {
		//	return database.Query<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
		//}
		//public TodoItem GetItem(int id) {
		//	return database.Table<TodoItem>().FirstOrDefault(x => x.ID == id);
		//}
		//public int DeleteItem(int id) {
		//	return database.Delete<TodoItem>(id);
		//}
	}
}
