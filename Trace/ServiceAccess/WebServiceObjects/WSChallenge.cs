using System;

namespace Trace {

	public class WSChallenge {

		public long id { get; set; }

		public long shopId { get; set; }

		public WSCondition conditions { get; set; }

		public string reward { get; set; }

		public long createdAt { get; set; }

		public long expiresAt { get; set; }

		public bool repeatable { get; set; }
	}
}
