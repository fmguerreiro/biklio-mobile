using System;
using System.Diagnostics;

namespace Trace {
	/// <summary>
	/// Adding AsyncErrorHandler allows us to handle async task exceptions, such as when using network requests.
	/// This is "weaved" in post-compile code automatically by Fody in a global way, to ensure async exceptions don't terminate our app.
	/// </summary>
	public static class AsyncErrorHandler {

		public static void HandleException(Exception exception) {
			Debug.WriteLine(exception.Message);
		}
	}
}