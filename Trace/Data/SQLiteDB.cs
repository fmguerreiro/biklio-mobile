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
			database = DependencyService.Get<SQLiteInterface>().GetConnection();
			//database.DropTable<User>();
			//database.DropTable<Challenge>();
			database.CreateTable<User>();
			//database.CreateTable<Trajectory>();
			database.CreateTable<Challenge>();
			// todo database.CreateTable<Checkpoint>();
		}


		public void CreateNewUser(string username, string email = "", string authToken = "") {
			User.Instance.Username = username;
			User.Instance.Email = email;
			User.Instance.AuthToken = authToken;
			database.Insert(User.Instance);
		}


		/// <summary>
		/// Fetches the user data from database and creates the user object. Called upon sign in.
		/// If no user with specified 'username' exists, it is created.
		/// </summary>
		/// <param name="username">Username.</param>
		public void InstantiateUser(string username) {
			var user = GetUser(username);
			if(user != null) {
				Debug.WriteLine("Signing in with user: " + user.Id);
				User.Instance.Id = user.Id;
				User.Instance.Username = user.Username;
				User.Instance.Email = user.Email;
				User.Instance.AuthToken = user.AuthToken;
				User.Instance.SearchRadiusInKM = user.SearchRadiusInKM;
				User.Instance.WsSyncVersion = user.WsSyncVersion;
				//Debug.WriteLine(GetItems<Challenge>());
				User.Instance.Challenges = GetItems<Challenge>().ToList();
				//User.Instance.Trajectories = GetItems<Trajectory>().ToList();
				//User.Instance.Checkpoints = GetItems<Checkpoint>().ToList();
			}
			else {
				CreateNewUser(username);
			}

		}


		/// <summary>
		/// Gets a user with specified Username.
		/// </summary>
		/// <param name="id">username to get</param>
		/// <returns>User or null.</returns>
		public User GetUser(string username) {
			return (from i in database.Table<User>()
					where i.Username == username
					select i).FirstOrDefault();
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
			//Debug.WriteLine("DEBUG - GetItems: " + string.Join(",", result.AsEnumerable()));
			return (System.Collections.Generic.IEnumerable<T>) result;
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
				return item.Id;
			}
			return database.Insert(item);
		}


		/// <summary>
		/// Saves a list of items calling SaveItems in 1 transaction
		/// </summary>
		/// <typeparam name="T">Type of entity to save</typeparam>
		/// <param name="items">List of items</param>
		public void SaveItems<T>(IEnumerable<T> items) where T : DatabaseEntityBase {
			database.BeginTransaction();
			foreach(T item in items) {
				var res = SaveItem(item);
				Debug.WriteLine("Rows inserted: " + res);
			}
			database.Commit();
			//Debug.WriteLine(database.Get<Challenge>(items.First<T>().Id).toString());
		}


		/// <summary>
		/// Deletes a specific item with id specified
		/// </summary>
		/// <typeparam name="T">Type of item to delete</typeparam>
		/// <param name="id">id of object to delete</param>
		public int DeleteItem<T>(int id) where T : DatabaseEntityBase, new() {
			return database.Delete<T>(new T() { Id = id });

		}

		public void DeleteAll() {
			database.BeginTransaction();
			database.DropTable<Challenge>();
			database.DropTable<Checkpoint>();
			database.DropTable<Trajectory>();
			database.Commit();
		}
	}
}
