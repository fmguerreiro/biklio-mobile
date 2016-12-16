namespace Trace {
	public class UnitPerActivity {
		public int Walking { get; set; }
		public int Running { get; set; }
		public int Cycling { get; set; }
		public int Driving { get; set; }
		public int Total { get { return Walking + Running + Cycling + Driving; } }
	}
}
