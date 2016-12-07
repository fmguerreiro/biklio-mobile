using System.Threading.Tasks;

namespace Trace {

	/// <summary>
	/// A simple class that adds the DoNotAwait function to Tasks that removes the compiler warning 
	/// for tasks which I do not want to wait for the result.
	/// </summary>
	public static class TaskExtensions {

		public static void DoNotAwait(this Task task) { }
	}
}
