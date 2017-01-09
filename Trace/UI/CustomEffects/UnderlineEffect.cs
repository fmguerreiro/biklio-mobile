using Xamarin.Forms;

namespace Trace {
	/// <summary>
	/// The FontAttributes contain only None, Bold and Italic.
	/// This custom effect is overwritten in the platform-specific code and allows us to underline labels.
	/// Code taken from: http://smstuebe.de/2016/08/29/underlinedlabel.xamarin.forms/
	/// </summary>
	public class UnderlineEffect : RoutingEffect {

		public const string EffectNamespace = "Effects";

		public UnderlineEffect() : base($"{EffectNamespace}.{nameof(UnderlineEffect)}") {
		}
	}
}