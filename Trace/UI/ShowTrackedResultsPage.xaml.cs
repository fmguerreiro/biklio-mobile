using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Trace {

	public partial class ShowTrackedResultsPage : ContentPage {
		public ShowTrackedResultsPage(StoreTrajectoryMap map) {
			InitializeComponent();
			Content = map;
		}
	}
}
