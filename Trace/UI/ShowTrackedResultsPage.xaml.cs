using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xamarin.Forms;

namespace Trace {

	public partial class TrajectoryDetailsPage : ContentPage {
		public TrajectoryDetailsPage(Trajectory trajectory) {
			InitializeComponent();
			// Put map dynamically in the first position of the screen.
			//Stack.Children.Insert(0, map);
			//Content = Stack;
		}
	}
}
