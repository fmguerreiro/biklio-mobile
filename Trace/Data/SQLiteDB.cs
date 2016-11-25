using System;
using System.Collections.Generic;
using SQLite;
using Xamarin.Forms;

namespace Trace {
	public class SQLiteDB {

		private readonly SQLiteConnection database;
		static readonly object locker = new object();

		/// <summary>
		/// Initializes a new instance of the Database. 
		/// if the database doesn't exist, it will create the database and all the tables.
		/// </summary>
		public SQLiteDB() {
			database = DependencyService.Get<SQLiteInterface>().GetConnection();
			database.CreateTable<User>();
			//database.CreateTable<Trajectory>();
			//database.CreateTable<Challenge>();
			// todo database.CreateTable<Checkpoint>();
		}


		public void CreateNewUser(string username, string email, string authToken) {
			lock(locker) {
				User.Instance.Username = username;
				User.Instance.Email = email;
				User.Instance.AuthToken = authToken;
				database.Insert(User.Instance);
			}
		}


		public void InstantiateUser(string username) {
			lock(locker) {
				var queryResult =
					from i in database.Table<User>()
					where i.Username == username
					select i;

				var user = queryResult.First();
				User.Instance.Username = user.Username;
				User.Instance.Email = user.Email;
				User.Instance.AuthToken = user.AuthToken;
				User.Instance.SearchRadiusInKM = user.SearchRadiusInKM;
				// todo check if this works or have to do more queries to get the lists!
				//User.Instance.Trajectories = user.Trajectories;
				//User.Instance.Challenges = user.Challenges;
				//User.Instance.Checkpoints = user.Checkpoints;
			}
		}

		//public IEnumerable<TodoItem> GetItemsNotDone ()
		//{
		//	lock (locker) {
		//		return database.Query<TodoItem>("SELECT * FROM [TodoItem] WHERE [Done] = 0");
		//	}
		//}

		//public TodoItem GetItem (int id) 
		//{
		//	lock (locker) {
		//		return database.Table<TodoItem>().FirstOrDefault(x => x.ID == id);
		//	}
		//}

		//public int SaveItem (TodoItem item) 
		//{
		//	lock (locker) {
		//		if (item.ID != 0) {
		//			database.Update(item);
		//			return item.ID;
		//		} else {
		//			return database.Insert(item);
		//		}
		//	}
		//}

		//public int DeleteItem(int id)
		//{
		//	lock (locker) {
		//		return database.Delete<TodoItem>(id);
		//	}
		//}
	}
}
