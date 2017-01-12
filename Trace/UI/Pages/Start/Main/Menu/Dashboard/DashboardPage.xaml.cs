using Xamarin.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Trace.Localization;
using System.Threading.Tasks;

namespace Trace {
	public partial class DashboardPage : ContentPage {

		public int RoutesToday { get; private set; }
		public int RoutesWeek { get; private set; }
		public int RoutesAllTime { get; private set; }

		public int RewardsToday { get; private set; }
		public int RewardsWeek { get; private set; }
		public int RewardsAllTime { get; private set; }

		// In meters.
		DistancePerActivity distanceToday; DistancePerActivity distanceWeek; DistancePerActivity distanceAllTime;
		public DistancePerActivity DistanceToday { get { return distanceToday; } }
		public DistancePerActivity DistanceWeek { get { return distanceWeek; } }
		public DistancePerActivity DistanceAllTime { get { return distanceAllTime; } }

		// In seconds.
		TimePerActivity timeToday; TimePerActivity timeWeek; TimePerActivity timeAllTime;
		public TimePerActivity TimeToday { get { return timeToday; } }
		public TimePerActivity TimeWeek { get { return timeWeek; } }
		public TimePerActivity TimeAllTime { get { return timeAllTime; } }

		// In calories.
		CaloriesPerActivity caloriesToday; CaloriesPerActivity caloriesWeek; CaloriesPerActivity caloriesAllTime;
		public CaloriesPerActivity CaloriesToday { get { return caloriesToday; } }
		public CaloriesPerActivity CaloriesWeek { get { return caloriesWeek; } }
		public CaloriesPerActivity CaloriesAllTime { get { return caloriesAllTime; } }


		public DashboardPage() {
			timeToday = new TimePerActivity(); timeWeek = new TimePerActivity(); timeAllTime = new TimePerActivity();
			distanceToday = new DistancePerActivity(); distanceWeek = new DistancePerActivity(); distanceAllTime = new DistancePerActivity();
			caloriesToday = new CaloriesPerActivity(); caloriesWeek = new CaloriesPerActivity(); caloriesAllTime = new CaloriesPerActivity();
			Task.Run(() => calculateStats());
			InitializeComponent();
		}


		async void onTappedDistance(object sender, EventArgs e) {
			var textCellTapped = ((TextCell) sender);
			string title = Language.DistancePerActivity + " {0} s";
			switch(textCellTapped.ClassId) {
				case "today":
					title = string.Format(title, string.Format(Language.TodayStats, DistanceToday.Total));
					await Navigation.PushAsync(new DrawPlotPage(DistanceToday, title));
					break;
				case "week":
					title = string.Format(title, string.Format(Language.ThisWeekStats, DistanceWeek.Total));
					await Navigation.PushAsync(new DrawPlotPage(DistanceWeek, title));
					break;
				case "alltime":
					title = string.Format(title, string.Format(Language.AllTimeStats, DistanceAllTime.Total));
					await Navigation.PushAsync(new DrawPlotPage(DistanceAllTime, title));
					break;
			}
		}


		async void onTappedCalories(object sender, EventArgs e) {
			var textCellTapped = ((TextCell) sender);
			string title = Language.CaloriesPerActivity + " {0} s";
			switch(textCellTapped.ClassId) {
				case "today":
					title = string.Format(title, string.Format(Language.TodayStats, CaloriesToday.Total));
					await Navigation.PushAsync(new DrawPlotPage(CaloriesToday, title));
					break;
				case "week":
					title = string.Format(title, string.Format(Language.ThisWeekStats, CaloriesWeek.Total));
					await Navigation.PushAsync(new DrawPlotPage(CaloriesWeek, title));
					break;
				case "alltime":
					title = string.Format(title, string.Format(Language.AllTimeStats, CaloriesAllTime.Total));
					await Navigation.PushAsync(new DrawPlotPage(CaloriesAllTime, title));
					break;
			}
		}


		async void onTappedTime(object sender, EventArgs e) {
			var textCellTapped = ((TextCell) sender);
			string title = Language.TimePerActivity + " {0} s";
			switch(textCellTapped.ClassId) {
				case "today":
					title = string.Format(title, string.Format(Language.TodayStats, TimeToday.Total));
					await Navigation.PushAsync(new DrawPlotPage(TimeToday, title));
					break;
				case "week":
					title = string.Format(title, string.Format(Language.ThisWeekStats, TimeWeek.Total));
					await Navigation.PushAsync(new DrawPlotPage(TimeWeek, title));
					break;
				case "alltime":
					title = string.Format(title, string.Format(Language.AllTimeStats, TimeAllTime.Total));
					await Navigation.PushAsync(new DrawPlotPage(TimeAllTime, title));
					break;
			}
		}


		void calculateStats() {
			IList<Challenge> challenges = User.Instance.Challenges;
			IList<Trajectory> trajectories = User.Instance.Trajectories;
			long now = TimeUtil.CurrentEpochTimeSeconds();
			var aDayAgo = now - (long) TimeSpan.FromDays(1).TotalSeconds;
			var aWeekAgo = now - (long) TimeSpan.FromDays(7).TotalSeconds;

			// Calculate rewards unlocked.
			foreach(Challenge c in challenges) {
				if(c.IsComplete) {
					RewardsAllTime++;
					if(TimeUtil.IsWithinPeriod(c.CompletedAt, aDayAgo, now)) RewardsWeek++;
					if(TimeUtil.IsWithinPeriod(c.CompletedAt, aWeekAgo, now)) RewardsWeek++;
				}
			}

			// Calculate routes, their distance, time and calories per activity.
			foreach(Trajectory t in trajectories) {
				// Calculate # routes. TODO routes per most common activity 
				RoutesAllTime++;
				if(TimeUtil.IsWithinPeriod(t.StartTime, aDayAgo, now)) RoutesToday++;
				if(TimeUtil.IsWithinPeriod(t.StartTime, aWeekAgo, now)) RoutesWeek++;

				// Calculate distance per activity.
				var walkDistance = t.CalculateWalkingDistance();
				var runDistance = t.CalculateRunningDistance();
				var cycleDistance = t.CalculateCyclingDistance();
				var driveDistance = t.CalculateDrivingDistance();
				accumulate(distanceAllTime, walkDistance, runDistance, cycleDistance, driveDistance);
				if(TimeUtil.IsWithinPeriod(t.StartTime, aDayAgo, now)) accumulate(distanceToday, walkDistance, runDistance, cycleDistance, driveDistance);
				if(TimeUtil.IsWithinPeriod(t.StartTime, aWeekAgo, now)) accumulate(distanceWeek, walkDistance, runDistance, cycleDistance, driveDistance);

				// Calculate time per activity.
				accumulate(timeAllTime, t.TimeSpentWalking, t.TimeSpentRunning, t.TimeSpentCycling, t.TimeSpentDriving);
				if(TimeUtil.IsWithinPeriod(t.StartTime, aDayAgo, now)) accumulate(timeToday, t.TimeSpentWalking, t.TimeSpentRunning, t.TimeSpentCycling, t.TimeSpentDriving);
				if(TimeUtil.IsWithinPeriod(t.StartTime, aWeekAgo, now)) accumulate(timeWeek, t.TimeSpentWalking, t.TimeSpentRunning, t.TimeSpentCycling, t.TimeSpentDriving);

				// Calculate calories per activity.
				var walkCal = t.CalculateWalkingCalories();
				var runCal = t.CalculateRunningCalories();
				var cycleCal = t.CalculateCyclingCalories();
				var driveCal = t.CalculateDrivingCalories();
				accumulate(caloriesAllTime, walkCal, runCal, cycleCal, driveCal);
				if(TimeUtil.IsWithinPeriod(t.StartTime, aDayAgo, now)) accumulate(caloriesToday, walkCal, runCal, cycleCal, driveCal);
				if(TimeUtil.IsWithinPeriod(t.StartTime, aWeekAgo, now)) accumulate(caloriesWeek, walkCal, runCal, cycleCal, driveCal);
			}
			Debug.WriteLine("DistanceAllTime.Total: " + DistanceAllTime.Total);
			Debug.WriteLine("DistanceAllTime.WalkDistance: " + DistanceAllTime.Walking);

			BindingContext = this;
		}


		static void accumulate(UnitPerActivity d, int walking, int running, int cycling, int driving) {
			d.Walking += walking;
			d.Running += running;
			d.Cycling += cycling;
			d.Driving += driving;
		}
	}
}