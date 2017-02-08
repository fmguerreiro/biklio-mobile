using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// This class maintains the state of the several conditions (i.e. guards) that are used to determine
	/// if the state machine should transition or not. 
	/// </summary>
	public class RewardEligibilityManager {
		static RewardEligibilityStateMachine stateMachine;
		Dictionary<State, Action> transitionGuards;

		//Task checkNearbyCheckpointsTask;

		// Events recorded for KPI.
		long cyclingEventStart;

		// Timeout threshold between 'cyclingIneligible' to 'cyclingEligible' in s. <=> 1,5 min.
		private const int CYCLING_INELIGIBLE_TIMEOUT = 90;
		// Timeout threshold between 'unknownEligible' to 'ineligible' in s. <=> 12 h.
		private const int UNKNOWN_ELIGIBLE_TIMEOUT = 12 * 60 * 60;
		// Timeout threshold between 'inAVehicle' to 'ineligible' in s. <=> 5 min.
		private const int VEHICULAR_TIMEOUT = 5 * 60;

		// When the user allows for constant background execution (either audio or GPS tracking),
		// use timers to determine when states should transition.
		//Timer timer;
		//Timer vehicularTimer;

		// Otherwise, when working in the background with periodic execution time (GSM updates),
		// these values keep track of the time and are updated whenever the app is allowed to run.
		private long cyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
		private long unknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;
		private long vehicularTimeAlloted = VEHICULAR_TIMEOUT;

		// This value indicates whether the state machine should start timers or use the 'timeAlloted' counters.
		//private const int USING_REALTIME_MOTION_FLAG = -1;

		// Minimum distance between user and checkpoints for reward eligibility.
		//private readonly double CKPT_DISTANCE_THRESHOLD = 120;
		// Minimum time between verification attempts.
		//private readonly int CHECK_NEARBY_TIMEOUT = 90;
		//private Timer checkNearbyCheckpointsTimer;

		// Counters for the number of CONSECUTIVE times an activity is obtained.
		// When one reaches THRESHOLD, play activity sound and transition between states (if transition guard allows).
		int walkingCount;
		int runningCount;
		int cyclingCount;
		int stationaryCount;
		int vehicularCount;

		int nonCyclingCount;
		int nonVehicularCount;

		// Successive count threshold for transitioning between states.
		// On iOS, it throttles activity reports after 3 consecutive activities if it does not change.
		private const int THRESHOLD = 4;

		// Time of the last activity recorded which is used to see how much time elapsed between activities.
		private long lastActivityTimestamp {
			get {
				if(Application.Current.Properties.ContainsKey("prev_activity_time")) {
					return (long) Application.Current.Properties["prev_activity_time"];
				}
				else {
					return (long) (Application.Current.Properties["prev_activity_time"] = TimeUtil.CurrentEpochTimeSeconds());
				}
			}
			set {
				Application.Current.Properties["prev_activity_time"] = value;
			}
		}

		public bool IsEligible {
			get {
				return stateMachine.CurrentState == State.CyclingEligible ||
					   stateMachine.CurrentState == State.UnknownEligible ||
					   stateMachine.CurrentState == State.Vehicular;
			}
		}

		static RewardEligibilityManager instance;
		public static RewardEligibilityManager Instance {
			get {
				if(instance == null) { instance = new RewardEligibilityManager(); }
				return instance;
			}
			set { instance = value; }
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="T:Trace.RewardEligibilityManager"/> class.
		/// The 'transitionGuards' is a dictionary that provides constant look-up for the next action to call based on the current state.
		/// </summary>
		public RewardEligibilityManager() {
			stateMachine = new RewardEligibilityStateMachine();
			transitionGuards = new Dictionary<State, Action> {
				{ State.Ineligible, new Action(ineligibleStateGuards) },
				{ State.CyclingIneligible, new Action(cyclingIneligibleStateGuards) },
				{ State.CyclingEligible, new Action(cyclingEligibleStateGuards) },
				{ State.UnknownEligible, new Action(unknownEligibleStateGuards) },
				{ State.Vehicular, new Action(vehicularStateGuards) }
			};
		}


		/// <summary>
		/// Destructor called upon dereferencing (i.e. when user logs out).
		/// </summary>
		~RewardEligibilityManager() {
			//timer?.Dispose();
			//vehicularTimer?.Dispose();
			//checkNearbyCheckpointsTimer?.Dispose();
			stateMachine = null;
		}


		/// <summary>
		/// The feedback from the motion detector goes here, passes the guard checks and goes into the state-machine.
		/// </summary>
		/// <param name="activity">Activity.</param>
		public void Input(ActivityType activity, long timestamp = -1) {
			incrementCounters(activity);
			Action nextAction;
			var state = stateMachine.CurrentState;

			// Fetch last recorded activity timestamp if there is no timestamp.
			long elapsedSecs;
			if(timestamp == -1) {
				var now = TimeUtil.CurrentEpochTimeSeconds();
				elapsedSecs = now - lastActivityTimestamp;
			}
			else {
				elapsedSecs = Math.Abs(timestamp - lastActivityTimestamp);
				lastActivityTimestamp = timestamp;
			}

			Debug.WriteLine($"RewardEligibilityManager.Input() - elapsed secs: {elapsedSecs}");
			switch(state) {
				case State.CyclingIneligible:
					cyclingIneligibleTimeAlloted -= elapsedSecs;
					Debug.WriteLine($"seconds remaining for cyclingEligible: {cyclingIneligibleTimeAlloted}");
					break;
				case State.UnknownEligible:
					unknownEligibleTimeAlloted -= elapsedSecs;
					Debug.WriteLine($"seconds remaining for ineligible: {unknownEligibleTimeAlloted}");
					break;
				case State.Vehicular:
					vehicularTimeAlloted -= elapsedSecs;
					Debug.WriteLine($"seconds remaining for ineligible: {vehicularTimeAlloted}");
					break;
			}

			if(cyclingIneligibleTimeAlloted < 1 || unknownEligibleTimeAlloted < 1 || vehicularTimeAlloted < 1) {
				stateMachine.MoveNext(Command.Timeout);
				state = stateMachine.CurrentState;
				cyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
				unknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;
				vehicularTimeAlloted = VEHICULAR_TIMEOUT;
			}

			transitionGuards.TryGetValue(state, out nextAction);
			nextAction.Invoke();
		}


		/// <summary>
		/// Exposes the state machine state to the rest of the application.
		/// </summary>
		/// <returns>The current state.</returns>
		public State GetCurrentState() {
			return stateMachine.CurrentState;
		}


		/// <summary>
		/// Increments the number of SUCCESSIVE activities.
		/// When any activity counter reaches THRESHOLD value, change music in background.
		/// </summary>
		/// <param name="activity">Activity.</param>
		void incrementCounters(ActivityType activity) {
			switch(activity) {
				case ActivityType.Stationary:
					stationaryCount++; nonCyclingCount++; nonVehicularCount++;
					cyclingCount = walkingCount = vehicularCount = runningCount = 0;
					if(stationaryCount == THRESHOLD + 1) {
						stationaryCount = 1;
						//DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.StationarySoundSetting);
					}
					return;
				case ActivityType.Walking:
					walkingCount++; nonCyclingCount++; nonVehicularCount++;
					cyclingCount = runningCount = vehicularCount = stationaryCount = 0;
					if(walkingCount == THRESHOLD + 1) {
						walkingCount = 1;
						//DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.WalkingSoundSetting);
					}
					return;
				case ActivityType.Running:
					runningCount++; nonCyclingCount++; nonVehicularCount++;
					cyclingCount = walkingCount = vehicularCount = stationaryCount = 0;
					if(runningCount == THRESHOLD + 1) {
						runningCount = 1;
						//DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.RunningSoundSetting);
					}
					return;
				case ActivityType.Cycling:
					cyclingCount++; nonVehicularCount++;
					nonCyclingCount = walkingCount = stationaryCount = runningCount = vehicularCount = 0;
					if(cyclingCount == THRESHOLD + 1) {
						cyclingCount = 1;
						//DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.CyclingSoundSetting);
					}
					return;
				case ActivityType.Automative:
					vehicularCount++; nonCyclingCount++;
					nonVehicularCount = stationaryCount = walkingCount = runningCount = cyclingCount = 0;
					if(vehicularCount == THRESHOLD + 1) { //&& User.Instance.IsBackgroundAudioEnabled) {
						vehicularCount = 1;
						//DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.VehicularSoundSetting);
					}
					return;
			}
		}


		void resetCounters() {
			stationaryCount = walkingCount = runningCount = cyclingCount = vehicularCount = nonCyclingCount = nonVehicularCount = 0;
			vehicularTimeAlloted = VEHICULAR_TIMEOUT;
			cyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
			// The eligibleUnknownTimeAlloted is different, only reset when going to 'cyclingEligible' state from 'unknownEligible' and 'cyclingIneligible'.
		}


		void ineligibleStateGuards() {
			// If user starts using a bycicle, go to: 'cyclingIneligible'.
			if(cyclingCount > THRESHOLD - 1) {
				resetCounters();
				cyclingEventStart = TimeUtil.CurrentEpochTimeSeconds();
				stateMachine.MoveNext(Command.Cycling);
				// Start timer -> If user continues using a bycicle for a certain time, go to: 'cyclingEligible'.
				cyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
				//if(elapsedTime == USING_REALTIME_MOTION_FLAG) {
				//	timer = new Timer(new TimerCallback(goToCyclingEligibleCallback), null, CYCLING_INELIGIBLE_TIMEOUT);
				//}
			}
		}


		void cyclingIneligibleStateGuards() {
			// If user stops using a bycicle, go back to the start: 'ineligible'.
			if(nonCyclingCount > THRESHOLD * 4 - 1) {
				//timer.Dispose();
				cyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
				resetCounters();
				// Ignore small cycling events.
				cyclingEventStart = 0;
				stateMachine.MoveNext(Command.NotCycling);
			}
		}


		void cyclingEligibleStateGuards() {
			// If user stops using a bycicle, go to: 'unknownEligible'.
			if(nonCyclingCount > THRESHOLD - 1) {
				resetCounters();

				// Record cycling event.
				User.Instance.GetCurrentKPI().AddCyclingEvent(cyclingEventStart, TimeUtil.CurrentEpochTimeSeconds());

				stateMachine.MoveNext(Command.NotCycling);

				// User likely got off bike, give an audible congratulatory sound to let her know she is eligible.
				//if(User.Instance.IsBackgroundAudioEnabled)
				DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.CongratulatorySoundSetting, 0);

				// Start a long timer where the user is still eligible for rewards even when not using a bycicle.
				// If the timer goes off (the user goes too long without using a bycicle), go back to 'ineligible'.
				unknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;
				//if(elapsedTime == USING_REALTIME_MOTION_FLAG) {
				//	timer = new Timer(new TimerCallback(goToIneligibleCallback), null, UNKNOWN_ELIGIBLE_TIMEOUT);
				//}
			}
		}


		void unknownEligibleStateGuards() {
			// When user leaves bike, check for nearby checkpoints/shops for reward notification every CHECK_NEARBY_TIMEOUT period.
			//if(checkNearbyCheckpointsTask == null) {
			//	checkNearbyCheckpointsTask = checkNearbyCheckpoints();
			//	checkNearbyCheckpointsTask.Start();
			//}

			// If users start using a vehicle, go to 'inAVehicle' and start a shorter timer that makes her ineligible after it fires.
			if(vehicularCount > THRESHOLD - 1) {
				resetCounters();
				stateMachine.MoveNext(Command.InAVehicle);
				//if(elapsedTime == USING_REALTIME_MOTION_FLAG) {
				//	vehicularTimer = new Timer(new TimerCallback(goToIneligibleCallback), null, VEHICULAR_TIMEOUT);
				//}
				vehicularTimeAlloted = VEHICULAR_TIMEOUT;
			}
			// If user starts cycling again, go back to 'cyclingEligible'.
			if(cyclingCount > THRESHOLD - 1) {
				resetCounters();
				cyclingEventStart = TimeUtil.CurrentEpochTimeSeconds();
				stateMachine.MoveNext(Command.Cycling);
				unknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;
				//DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.BycicleEligibleSoundSetting);
				//timer.Dispose();
			}
		}


		void vehicularStateGuards() {
			// If the user stops using a vehicle, go back to 'unknownEligible'.
			if(nonVehicularCount > THRESHOLD - 1) {
				resetCounters();
				//vehicularTimer.Dispose();
				vehicularTimeAlloted = VEHICULAR_TIMEOUT;
				stateMachine.MoveNext(Command.NotInAVehicle);
			}
		}


		/// <summary>
		/// When a user goes from 'cyclingEligible' to 'unknownEligible', i.e., stops cycling, 
		/// check for shops nearby with 'distance' condition to see if she is eligible for rewards.
		/// </summary>
		// TODO remove this, this is now done by the geofencing function
		//async Task checkNearbyCheckpoints() {
		//	var now = TimeUtil.CurrentEpochTimeSeconds();
		//	var nChallengesCompleted = 0;
		//	// Compare the distance between the user and the checkpoints.
		//	var userLocation = await GeoUtils.GetCurrentUserLocation();
		//	foreach(var checkpoint in User.Instance.Checkpoints) {
		//		var ckptLocation = new Position {
		//			Longitude = checkpoint.Value.Longitude,
		//			Latitude = checkpoint.Value.Latitude
		//		};
		//		var distance = GeoUtils.DistanceBetweenPoints(userLocation, ckptLocation);

		//		if(distance < CKPT_DISTANCE_THRESHOLD) {
		//			// Check their challenges' conditions.
		//			foreach(var challenge in checkpoint.Value.Challenges) {
		//				var createdAt = challenge.CreatedAt;
		//				var expiresAt = challenge.ExpiresAt;
		//				// For the valid challenges ...
		//				if(TimeUtil.IsWithinPeriod(now, createdAt, expiresAt)) {
		//					// Check if distance cycled meets the criteria.
		//					if(challenge.NeededMetersCycling <= CycledDistanceBetween(createdAt, expiresAt)) {
		//						challenge.IsComplete = true;
		//						challenge.CompletedAt = now;
		//						SQLiteDB.Instance.SaveItem(challenge);
		//						nChallengesCompleted++;
		//						// Record event
		//						User.Instance.GetCurrentKPI().AddCheckInEvent(now, challenge.CheckpointId);
		//					}
		//				}
		//			}
		//		}
		//	}

		//	sendRewardNotificationUser(nChallengesCompleted);
		//	HomePage.UpdateRewardIcon();
		//	// Start a timer that waits a certain period before doing 'checkNearbyCheckpoints' again.
		//	checkNearbyCheckpointsTimer = new Timer(new TimerCallback(
		//											(obj) => { checkNearbyCheckpointsTask = null; }),
		//												null,
		//												CHECK_NEARBY_TIMEOUT);
		//}


		public static long CycledDistanceBetween(long start, long end) {
			long res = 0;
			foreach(Trajectory t in User.Instance.Trajectories) {
				if(TimeUtil.IsWithinPeriod(t.EndTime, start, end))
					res += t.CalculateCyclingDistance();
			}
			return res;
		}


		public static bool IsUserEligible() {
			if(stateMachine == null) return false;
			return stateMachine.CurrentState == State.CyclingEligible || stateMachine.CurrentState == State.UnknownEligible;
		}


		/// <summary>
		/// Called when a user goes from 'cyclingIneligible' to 'cyclingEligible', i.e., 
		/// has been cycling for at least 1.5 mins, 
		/// check the list of challenges for those without 'distance' condition, i.e., the 'cycle to shop' challenges.
		/// </summary>
		//void checkForRewards() {
		//	var now = TimeUtil.CurrentEpochTimeSeconds();
		//	var cycleToShopChallenges = User.Instance.Challenges.FindAll((x) => x.NeededMetersCycling == 0);
		//	var nChallengesCompleted = cycleToShopChallenges.Count;
		//	foreach(var c in cycleToShopChallenges) {
		//		c.IsComplete = true;
		//		c.CompletedAt = now;
		//		SQLiteDB.Instance.SaveItem(c);
		//		// Record event
		//		User.Instance.GetCurrentKPI().AddCheckInEvent(now, c.CheckpointId);
		//	}
		//	HomePage.UpdateRewardIcon();
		//	sendRewardNotificationUser(nChallengesCompleted);
		//}


		//static void sendRewardNotificationUser(int nChallengesCompleted) {
		//	if(nChallengesCompleted > 0) {
		//		DependencyService.Get<INotificationMessage>().Send(
		//			 "checkForRewards",
		//			 Language.RewardsEarned,
		//			 Language.YouHaveEarned + " " + nChallengesCompleted + " " + Language.nRewardsClickToSeeWhat,
		//			 nChallengesCompleted
		//		);
		//	}
		//}


		/// <summary>
		/// Timer callback. User is now eligible for rewards.
		/// </summary>
		void goToCyclingEligibleCallback(object state) {
			resetCounters();
			stateMachine.MoveNext(Command.Timeout);
			//Task.Run(() => checkForRewards()).DoNotAwait();

			if(App.IsInForeground)
				HomePage.AddCyclingIndicator();

			// Start different sound in background if needed.
			//var soundPlayer = DependencyService.Get<ISoundPlayer>();
			//if(soundPlayer.IsPlaying() && User.Instance.IsBackgroundAudioEnabled) {
			//	soundPlayer.StopSound();
			//	soundPlayer.PlaySound(User.Instance.BycicleEligibleSoundSetting);
			//}
		}


		/// <summary>
		/// Timer callback. User is now ineligible for rewards.
		/// </summary>
		void goToIneligibleCallback(object state) {
			resetCounters();
			stateMachine.MoveNext(Command.Timeout);
			vehicularTimeAlloted = VEHICULAR_TIMEOUT;
			unknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;

			if(App.IsInForeground)
				HomePage.RemoveCyclingIndicator();

			//timer.Dispose();
			//vehicularTimer.Dispose();
			//if(User.Instance.IsBackgroundAudioEnabled)
			DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.NoLongerEligibleSoundSetting);
		}
	}
}
