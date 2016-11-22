using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Trace {
	public partial class ChallengesPage : ContentPage {
		public ChallengesPage() {
			InitializeComponent();

			var monkeys = new List<Challenge> {
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"},
				new Challenge {Name="Xander", Description="Writer"},
				new Challenge {Name="Rupert", Description="Engineer"},
				new Challenge {Name="Tammy", Description="Designer"},
				new Challenge {Name="Blue", Description="Speaker"}
			};
			BindingContext = new ChallengeVM { Challenges = monkeys };
		}


		void OnSelection(object sender, SelectedItemChangedEventArgs e) {

			DisplayAlert("Item Selected", ((Challenge) e.SelectedItem).Name, "Ok");
			//((ListView)sender).SelectedItem = null; //uncomment line if you want to disable the visual selection state.
		}
	}
}
