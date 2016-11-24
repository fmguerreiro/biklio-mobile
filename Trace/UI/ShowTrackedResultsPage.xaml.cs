using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;

namespace Trace {

	public partial class ShowTrackedResultsPage : ContentPage {
		public ShowTrackedResultsPage(StoreTrajectoryMap map) {
			InitializeComponent();
			// Put map dynamically in the first position of the screen.
			//Stack.Children.Insert(0, map);
			Content = Stack;
		}

		protected override bool OnBackButtonPressed() {
			Debug.WriteLine("GOT HERE!!!!");
			return base.OnBackButtonPressed();
		}
	}
}
