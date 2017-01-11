using System;

namespace Trace {

	/// <summary>
	/// Interface for securely storing data in an account store that's backed by Keychain services in iOS,
	/// and the KeyStore class in Android.
	/// </summary>
	public interface ICredentialsStore {

		string Username { get; }

		string GetPassword(string username);

		bool Exists(string username);

		void SaveCredentials(string username, string password);

		void DeleteAllCredentials();

		void DeleteCredentials(string username);
	}
}
