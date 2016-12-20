using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Trace.Localization {

	// You exclude the 'Extension' suffix when using in Xaml markup
	[ContentProperty("Text")]
	/// <summary>
	/// This class is responsible for text translation in XAML.
	/// Example usage in XAML:
	///  - header: xmlns:language="clr-namespace:Trace.Localization;assembly=Trace"
	///  - item: <Button Text="{language:Translate AddButton}" />
	/// Example usage in C#: myLabel.Text = Language.NotesLabel;
	/// </summary>
	public class Translate : IMarkupExtension {

		readonly CultureInfo ci;
		const string ResourceId = "Trace.Localization.Language";

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
					string.Format("Key '{0}' was not found in resources '{1}' for culture '{2}'.", Text, ResourceId, ci.Name), "Text");
#else
                translation = Text; // HACK: returns the key, which GETS DISPLAYED TO THE USER
#endif
			}
			return translation;
		}
	}
}