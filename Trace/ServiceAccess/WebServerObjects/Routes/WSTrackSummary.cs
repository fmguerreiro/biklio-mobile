﻿namespace Trace {

	/// <summary>
	/// Before sending a trajectory to the server, a summary is sent.
	/// </summary>
	public class WSTrackSummary {

		public string session { get; set; }

		public long startedAt { get; set; } // In milliseconds.

		public long endedAt { get; set; } // In milliseconds.

		public int elapsedTime { get; set; } // In seconds.

		public double elapsedDistance { get; set; } // In meters.

		public float avgSpeed { get; set; } // In m/s.

		public float topSpeed { get; set; } // In m/s.

		public int points { get; set; } // Number of points in trajectory.

		public int modality { get; set; } // Most common activity.
	}
}
