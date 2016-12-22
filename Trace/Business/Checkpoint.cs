using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;

namespace Trace {
	public class Checkpoint : UserItemBase {

		public string Name { get; set; }
		public string Address { get; set; }
		public string AvailableHours { get; set; }
		public long OwnerId { get; set; }

		private string logoURL;
		public string LogoURL {
			get {
				return logoURL;
			}
			set {
				logoURL = value ?? logoURL;
			}
		}

		// The image is read-only to other contexts. It should only be updated by the FetchImageAsync function.
		// However, it needs a 'set' to be inserted into SQLite.
		public string LogoImageFilePath { get; set; }

		public string PhoneNumber { get; set; }
		public string WebsiteAddress { get; set; }
		public string FacebookAddress { get; set; }
		public string TwitterAddress { get; set; }
		public string BikeFacilities { get; set; }
		public string Description { get; set; }

		List<Challenge> challenges;
		[Ignore]
		public List<Challenge> Challenges {
			get { return challenges ?? new List<Challenge>(); }
			set { challenges = value ?? new List<Challenge>(); }
		}

		// 'double' instead of 'Position' because SQLite only supports basic types. 
		public double Longitude { get; set; }
		public double Latitude { get; set; }


		/// <summary>
		/// Fetches and updates the image of this checkpoint.
		/// Also writes the image to the filesystem and updates SQLite with the filepath.
		/// </summary>
		/// <param name="url">The image URL.</param>
		public async Task FetchImageAsync(string url) {
			Debug.WriteLine("Downloading checkpoint img: " + url);
			byte[] res = await new HTTPClientBase().DownloadImageAsync(url);
			if(res != null) {
				// Store bytes as img in file system.
				var filePath = this.GId.ToString();
				DependencyService.Get<IFileSystem>().SaveImage(filePath, res);
				// Updates filepath property in SQLite.
				LogoImageFilePath = filePath;
				SQLiteDB.Instance.SaveItem<Checkpoint>(this);
				Debug.WriteLine("Img: " + url + " downloaded.");
				return;
			}
			Debug.WriteLine("Download for img: " + url + " failed.");
		}


		public override string ToString() {
			return string.Format("[Checkpoint Id->{0} UserId->{1} Name->{2} LogoImageFilePath->{3} Longitude->{4} Latitude->{5}]",
					 			 Id, UserId, Name, LogoImageFilePath, Longitude, Latitude);
		}
	}
}
