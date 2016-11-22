using System;

namespace Trace {

	/// <summary>
	/// Interface for securely storing data in an account store that's backed by Keychain services in iOS,
	/// and the KeyStore class in Android.
	/// </summary>
	public interface StoreCredentialsInterface {

		string Username { get; }

		string Password { get; }

		void SaveCredentials(string username, string password);

		void DeleteCredentials();
	}
}
