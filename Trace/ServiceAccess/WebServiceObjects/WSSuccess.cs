using System;

namespace Trace {

	public class WSSuccess {

		// Indicates whether the operation was succesful or not.
		public bool success { get; set; }

		// Authentication token.
		public string token { get; set; }

		// Error message in case the operation is not successful.
		public string error { get; set; }
	}
}
