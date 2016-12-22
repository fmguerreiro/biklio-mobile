using Xamarin.Forms;
using OxyPlot.Xamarin.Forms;
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

		async void onTapped(object sender, EventArgs e) {
			await Navigation.PushAsync(new DrawPlotPage());
		}

		async void onTappedDistance(object sender, EventArgs e) {
			await Navigation.PushAsync(new DrawPlotPage(DistanceAllTime, Language.DistancePerActivity));
		}

		async void onTappedCalories(object sender, EventArgs e) {
			await Navigation.PushAsync(new DrawPlotPage(CaloriesAllTime, Language.CaloriesPerActivity));
		}

		async void onTappedTime(object sender, EventArgs e) {
			// TODO just an example, delete this
			CaloriesAllTime.Cycling = 50; CaloriesAllTime.Driving = 20; caloriesAllTime.Running = 2; caloriesAllTime.Walking = 80;
			await Navigation.PushAsync(new DrawPlotPage(CaloriesAllTime, Language.TimePerActivity));
		}


		void calculateStats() {
			IList<Trajectory> trajectories = User.Instance.Trajectories;
			long now = TimeUtil.CurrentEpochTime();
			var todayWindow = (long) TimeSpan.FromDays(1).TotalSeconds;
			var weekWindow = (long) TimeSpan.FromDays(7).TotalSeconds;

			foreach(Trajectory t in trajectories) {
				// Calculate # routes. TODO routes per most common activity 
				RoutesAllTime++;
				if(wasTrajectoryDoneToday(now, todayWindow, t)) RoutesToday++;
				if(wasTrajectoryDoneThisWeek(now, weekWindow, t)) RoutesWeek++;

				// todo: rewards stats

				// Calculate distance per activity.
				var walkDistance = t.CalculateWalkingDistance();
				var runDistance = t.CalculateRunningDistance();
				var cycleDistance = t.CalculateCyclingDistance();
				var driveDistance = t.CalculateDrivingDistance();
				calculate(distanceAllTime, walkDistance, runDistance, cycleDistance, driveDistance);
				if(wasTrajectoryDoneToday(now, todayWindow, t)) calculate(distanceToday, walkDistance, runDistance, cycleDistance, driveDistance);
				if(wasTrajectoryDoneThisWeek(now, weekWindow, t)) calculate(distanceWeek, walkDistance, runDistance, cycleDistance, driveDistance);

				// Calculate time per activity.
				calculate(timeAllTime, t.TimeSpentWalking, t.TimeSpentRunning, t.TimeSpentCycling, t.TimeSpentDriving);
				if(wasTrajectoryDoneToday(now, todayWindow, t)) calculate(timeToday, t.TimeSpentWalking, t.TimeSpentRunning, t.TimeSpentCycling, t.TimeSpentDriving);
				if(wasTrajectoryDoneThisWeek(now, weekWindow, t)) calculate(timeWeek, t.TimeSpentWalking, t.TimeSpentRunning, t.TimeSpentCycling, t.TimeSpentDriving);

				// Calculate calories per activity.
				var walkCal = t.CalculateWalkingCalories();
				var runCal = t.CalculateRunningCalories();
				var cycleCal = t.CalculateCyclingCalories();
				var driveCal = t.CalculateDrivingCalories();
				calculate(caloriesAllTime, walkCal, runCal, cycleCal, driveCal);
				if(wasTrajectoryDoneToday(now, todayWindow, t)) calculate(caloriesToday, walkCal, runCal, cycleCal, driveCal);
				if(wasTrajectoryDoneThisWeek(now, weekWindow, t)) calculate(caloriesWeek, walkCal, runCal, cycleCal, driveCal);
			}
			Debug.WriteLine("DistanceAllTime.Total: " + DistanceAllTime.Total);
			Debug.WriteLine("DistanceAllTime.WalkDistance: " + DistanceAllTime.Walking);

			BindingContext = this;
		}

		static bool wasTrajectoryDoneToday(long now, long todayWindow, Trajectory t) {
			return t.StartTime > (now - todayWindow);
		}

		static bool wasTrajectoryDoneThisWeek(long now, long weekWindow, Trajectory t) {
			return t.StartTime > (now - weekWindow);
		}

		static void calculate(UnitPerActivity d, int walking, int running, int cycling, int driving) {
			d.Walking += walking;
			d.Running += running;
			d.Cycling += cycling;
			d.Driving += driving;
		}
	}
}