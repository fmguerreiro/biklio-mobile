using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Trace {

	// You exclude the 'Extension' suffix when using in Xaml markup
	[ContentProperty("Text")]
	/// <summary>
	/// This class is responsible for text translation in XAML.
	/// Example usage:
	///  - XAML header: xmlns:i18n="clr-namespace:Localization.Trace;assembly=Trace"
	///  - item: <Button Text="{i18n:Translate AddButton}" />
	/// </summary>
	public class Translate : IMarkupExtension {

		readonly CultureInfo ci;
		const string ResourceId = "Trace.Language";

		public Translate() {
			ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
		}

		public string Text { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider) {
			if(Text == null)
				return "";

			var resmgr = new ResourceManager(ResourceId, typeof(Translate).GetTypeInfo().Assembly);

			var translation = resmgr.GetString(Text, ci);

			if(translation == null) {
#if DEBUG
				throw new ArgumentException(
					String.Format("Key '{0}' was not found in resources '{1}' for culture '{2}'.", Text, ResourceId, ci.Name),
					"Text");
#else
                translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
			}
			return translation;
		}
	}
}