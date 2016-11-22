using Xamarin.Forms;

namespace Trace {
	public class NumberValidator : Behavior<Entry> {

		static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(NumberValidator), false);

		public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

		public bool IsValid {
			get { return (bool) base.GetValue(IsValidProperty); }
			private set { base.SetValue(IsValidPropertyKey, value); }
		}

		protected override void OnAttachedTo(Entry bindable) {
			bindable.TextChanged += bindable_TextChanged;
		}

		private void bindable_TextChanged(object sender, TextChangedEventArgs e) {
			double result;
			IsValid = double.TryParse(e.NewTextValue, out result);
			((Entry) sender).TextColor = IsValid ? Color.Default : Color.Red;

		}

		protected override void OnDetachingFrom(Entry bindable) {
			bindable.TextChanged -= bindable_TextChanged;
		}
	}
}
