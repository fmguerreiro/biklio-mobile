using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using SQLite;
using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// A Checkpoint represents a shop/company participant in the TRACE project.
	/// </summary>
	public class Checkpoint : UserItemBase {

		public string Name { get; set; }
		public string Address { get; set; }
		public string AvailableHours { get; set; }
		public long OwnerId { get; set; }
		// Describes the type of checkpoint it is, e.g., Restaurant, Shop, Gym, etc.
		public string Type { get; set; }

		private string logoURL;
		public string LogoURL { get { return logoURL ?? "images/challenge_list/default_shop.png"; } set { logoURL = value ?? logoURL; } }

		// The stored logo image filepath for map pin image display.
		private string pinLogoPath;
		public string PinLogoPath { get { return pinLogoPath; } set { pinLogoPath = value; } }

		private string mapImageURL;
		public string MapImageURL { get { return mapImageURL ?? "images/challenge_list/location_unknown.png"; } set { mapImageURL = value ?? mapImageURL; } }

		public string Email { get; set; }
		public string PhoneNumber { get; set; }
		public string WebsiteAddress { get; set; }
		public string FacebookAddress { get; set; }
		public string TwitterAddress { get; set; }
		public string BikeFacilities { get; set; }
		public string Description { get; set; }

		List<Challenge> challenges;
		[Ignore]
		public List<Challenge> Challenges {
			get { if(challenges == null) { challenges = new List<Challenge>(); } return challenges; }
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
				// Resize image to 20x20 pixels to save space.
				res = DependencyService.Get<IImageResizer>().ResizeImage(res, 20, 20);
				// Store bytes as img in file system.
				var filePath = GId.ToString();
				DependencyService.Get<IFileSystem>().SaveImage(filePath, res);
				// Updates filepath property in SQLite.
				PinLogoPath = filePath;
				SQLiteDB.Instance.SaveItem(this);
				Debug.WriteLine("Img: " + url + " downloaded.");
				return;
			}
			Debug.WriteLine("Download for img: " + url + " failed.");
		}


		public override string ToString() {
			return string.Format("[Checkpoint GId->{0} UserId->{1} Name->{2} LogoURL->{3} Longitude->{4} Latitude->{5}]",
					 			 GId, UserId, Name, LogoURL, Longitude, Latitude);
		}
	}
}
