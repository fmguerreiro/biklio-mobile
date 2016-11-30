using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SQLite;
using Xamarin.Forms;

namespace Trace {
	public class SQLiteDB {

		private readonly SQLiteConnection database;
		//static readonly object locker = new object();

		private static SQLiteDB instance;
		public static SQLiteDB Instance {
			get {
				if(instance == null) { instance = new SQLiteDB(); }
				return instance;
			}
		}


		/// <summary>
		/// Initializes a new instance of the Database. 
		/// if the database doesn't exist, it will create the database and all the tables.
		/// </summary>
		private SQLiteDB() {
			database = DependencyService.Get<ISQLite>().GetConnection();
			//DropAllTables();
			database.CreateTable<User>();
			database.CreateTable<Trajectory>();
			database.CreateTable<Challenge>();
			database.CreateTable<Checkpoint>();
		}


		/// <summary>
		/// Fetches the user data from database and creates the user object. Called upon sign in.
		/// If no user with specified 'username' exists, it is created.
		/// </summary>
		/// <param name="username">Username.</param>
		public void InstantiateUser(string username) {
			var user = GetUser(username);
			if(user != null) {
				User.Instance = user;
				Debug.WriteLine("Signing in with user: " + user);
				User.Instance.Challenges = GetItems<Challenge>().ToList();
				User.Instance.Trajectories = GetItems<Trajectory>().ToList();
				User.Instance.Checkpoints = GetItems<Checkpoint>().ToDictionary(key => key.Id, val => val);
			}
			else {
				User.Instance = new User { Username = username };
				User.Instance.Id = SaveItem<User>(User.Instance);
			}
			Debug.WriteLine("Signing in with user: " + User.Instance);
		}


		/// <summary>
		/// Gets a user with specified Username.
		/// </summary>
		/// <param name="id">username to get</param>
		/// <returns>User or null.</returns>
		public User GetUser(string username) {
			return (from i in database.Table<User>()
					where i.Username.Equals(username)
					select i).FirstOrDefault();
			//var query = string.Format("select * from User where Username = \"{0}\"", username);
			//Debug.WriteLine("GetUser(Query): " + query);
			//IEnumerable<User> result = database.Query<User>(query);
			//Debug.WriteLine("GetUser(Result): " + result.First());
			//return result.FirstOrDefault();
		}


		/// <summary>
		/// Gets a specific item of type T with specified ID
		/// </summary>
		/// <typeparam name="T">Type of item to get</typeparam>
		/// <param name="id">ID to get</param>
		/// <returns>Item type T or null.</returns>
		public T GetItem<T>(int id) where T : DatabaseEntityBase, new() {
			return (from i in database.Table<T>()
					where i.Id == id
					select i).FirstOrDefault();
		}


		/// <summary>
		/// Gets all items of type T belonging to User.
		/// </summary>
		/// <typeparam name="T">Type of item to get</typeparam>
		/// <returns></returns>
		public IEnumerable<T> GetItems<T>() where T : UserItemBase, new() {
			var result =
				from i in database.Table<T>()
				where i.UserId == User.Instance.Id
				select i;
			//var query = string.Format("select * from \"{0}\" where UserId = {1}", typeof(T).Name, User.Instance.Id);
			//IEnumerable<T> result = database.Query<T>(query);
			//Debug.WriteLine("GetItems: " + string.Join(",", result.AsEnumerable()));
			return result;
		}


		/// <summary>
		/// Save and update item of type T. If ID is set then will update the item, else creates new and returns the id.
		/// </summary>
		/// <typeparam name="T">Type of entity</typeparam>
		/// <param name="item">Item to save or update</param>
		/// <returns>ID of item</returns>
		public long SaveItem<T>(T item) where T : DatabaseEntityBase {
			if(item.Id != 0) {
				database.Update(item);
				Debug.WriteLine("SaveItem(Update): " + item);
				return item.Id;
			}
			//string getLastInsertedId = @"select last_insert_rowid()";
			//long lastId = database.ExecuteScalar<long>(getLastInsertedId);
			//Debug.WriteLine("SaveItem(LastId): " + lastId);
			database.Insert(item);
			Debug.WriteLine("SaveItem(Insert): " + item);
			return item.Id;
		}


		/// <summary>
		/// Saves a list of items calling SaveItems in 1 transaction
		/// </summary>
		/// <typeparam name="T">Type of entity to save</typeparam>
		/// <param name="items">List of items</param>
		public void SaveItems<T>(IEnumerable<T> items) where T : DatabaseEntityBase {
			database.BeginTransaction();
			foreach(T item in items) {
				SaveItem(item);
			}
			database.Commit();
		}


		/// <summary>
		/// Deletes a specific item with id specified.
		/// </summary>
		/// <typeparam name="T">Type of item to delete</typeparam>
		/// <param name="id">id of object to delete</param>
		public int DeleteItem<T>(long id) where T : DatabaseEntityBase, new() {
			var deletedId = database.Delete<T>(new T() { Id = id });
			Debug.WriteLine("DeleteItem: id->" + deletedId);
			return deletedId;
		}

		/// <summary>
		/// Deletes all items with the ids specified.
		/// </summary>
		/// <typeparam name="T">Type of item to delete</typeparam>
		/// <param name="id">ids of objects to delete</param>
		public int DeleteItems<T>(long[] ids) where T : DatabaseEntityBase, new() {
			int rowsDeleted = database.Execute(string.Format("delete from \"{0}\" where Id in ({1})", typeof(T).Name, string.Join(", ", ids)));
			Debug.WriteLine("DeletedItems: " + rowsDeleted);
			return rowsDeleted;
		}

		/// <summary>
		/// Deletes all tables in the DB.
		/// </summary>
		public void DropAllTables() {
			database.DropTable<User>();
			database.DropTable<Challenge>();
			database.DropTable<Checkpoint>();
			database.DropTable<Trajectory>();
		}
	}
}
