using System.Globalization;

namespace Trace {

	/// <summary>
	/// Interface that exposes the methods to obtain the user's language preference.
	/// </summary>
	public interface ILocalize {

		CultureInfo GetCurrentCultureInfo();
		void SetLocale(CultureInfo ci);
	}
}
