using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Trace {

	public class PasswordValidator : Behavior<Entry> {

		// Must have at least 8 chars, 1 Alphabet and 1 Number
		const string passRegex = "^(?=.*[A-Za-z])(?=.*\\d)[A-Za-z\\d]{8,}$";

		static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(PasswordValidator), false);

		public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

		public bool IsValid {
			get { return (bool) base.GetValue(IsValidProperty); }
			private set { base.SetValue(IsValidPropertyKey, value); }
		}

		protected override void OnAttachedTo(Entry bindable) {
			bindable.TextChanged += HandleTextChanged;
		}

		void HandleTextChanged(object sender, TextChangedEventArgs e) {
			IsValid = (Regex.IsMatch(e.NewTextValue, passRegex, RegexOptions.None, TimeSpan.FromMilliseconds(250)));
			((Entry) sender).TextColor = IsValid ? Color.Default : Color.Red;
		}

		protected override void OnDetachingFrom(Entry bindable) {
			bindable.TextChanged -= HandleTextChanged;

		}
	}
}