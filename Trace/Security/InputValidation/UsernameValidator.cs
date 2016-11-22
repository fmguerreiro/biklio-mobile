using System;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace Trace {

	public class UsernameValidator : Behavior<Entry> {
		const string usernameRegex = "^(?=.{6,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$";

		static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(UsernameValidator), false);

		public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

		public bool IsValid {
			get { return (bool) GetValue(IsValidProperty); }
			private set { SetValue(IsValidPropertyKey, value); }
		}

		protected override void OnAttachedTo(Entry bindable) {
			bindable.TextChanged += HandleTextChanged;
		}

		void HandleTextChanged(object sender, TextChangedEventArgs e) {
			IsValid = (Regex.IsMatch(e.NewTextValue, usernameRegex, RegexOptions.None, TimeSpan.FromMilliseconds(250)));
			((Entry) sender).TextColor = IsValid ? Color.Default : Color.Red;
		}

		protected override void OnDetachingFrom(Entry bindable) {
			bindable.TextChanged -= HandleTextChanged;

		}
	}
}
