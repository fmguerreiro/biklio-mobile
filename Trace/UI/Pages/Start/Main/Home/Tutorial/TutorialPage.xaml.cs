using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// Page that shows a carousel view of the tutorial.
	/// </summary>
	public partial class TutorialPage : ContentPage {

		private TutorialPart lastPart;

		public TutorialPage() {
			InitializeComponent();

			var tutorialDataModel = new TutorialDataModel {
				Parts = new List<TutorialPart> {
					new TutorialPart {
						ImagePath = "tutorial/tutorial_part1.png",
						Indicator = "1/4",
						Color = "#4BB199",
						Title = "",
						Description = Language.TutorialDescription1 },
					new TutorialPart {
						ImagePath = "tutorial/tutorial_part2.png",
						Indicator = "2/4",
						Color = "#4BB166",
						Title = Language.TutorialTitle2,
						Description = Language.TutorialDescription2 },
					new TutorialPart {
						ImagePath = "tutorial/tutorial_part3.png",
						Indicator = "3/4",
						Color = "#63B14B",
						Title = Language.TutorialTitle3,
						Description = Language.TutorialDescription3 },
					new TutorialPart {
						ImagePath = "tutorial/tutorial_part4.png",
						Indicator = "4/4",
						Color = "#96b14b",
						Title = Language.TutorialTitle4,
						Description = Language.TutorialDescription4 } }
			};

			lastPart = tutorialDataModel.Parts.Last();

			BindingContext = tutorialDataModel;
		}


		// Show a 'finish' button when the user reaches the last page of the carousel view.		
		void onTutorialPartChanged(object sender, SelectedItemChangedEventArgs e) {
			var selectedPart = (TutorialPart) e.SelectedItem;
			if(selectedPart == lastPart) {
				//confirmationButton.IsVisible = true;
			}
		}


		void onPrivacyPolicyClicked(object sender, EventArgs e) {
			// todo show privacy policy page
		}

		async void onConfirmationClicked(object sender, EventArgs e) {
			await Navigation.PopModalAsync();
		}
	}
}
