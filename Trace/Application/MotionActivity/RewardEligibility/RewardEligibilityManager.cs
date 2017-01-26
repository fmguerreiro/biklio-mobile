using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Plugin.Geolocator.Abstractions;
using Trace.Localization;
using Xamarin.Forms;

namespace Trace {

	/// <summary>
	/// This class maintains the state of the several conditions (i.e. guards) that are used to determine
	/// if the state machine should transition or not. 
	/// </summary>
	public class RewardEligibilityManager {
		RewardEligibilityStateMachine stateMachine;
		Dictionary<State, Action<int>> transitionGuards;

		Task checkNearbyCheckpointsTask;

		// Events recorded for KPI.
		long cyclingEventStart;

		// Timeout threshold between 'cyclingIneligible' to 'cyclingEligible' in ms. 1,5 min.
		private const int CYCLING_INELIGIBLE_TIMEOUT = 90 * 1000;
		// Timeout threshold between 'unknownEligible' to 'ineligible' in ms. 12 h.
		private const int UNKNOWN_ELIGIBLE_TIMEOUT = 12 * 60 * 60 * 1000;
		// Timeout threshold between 'inAVehicle' to 'ineligible' in ms. 5 min.
		private const int VEHICULAR_TIMEOUT = 5 * 60 * 1000;

		// When the user allows for constant background execution (either audio or GPS tracking),
		// use timers to determine when states should transition.
		Timer timer;
		Timer vehicularTimer;

		// Otherwise, when working in the background with periodic execution time (GSM updates),
		// these values keep track of the time and are updated whenever the app is allowed to run.
		private int backgroundCyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
		private int backgroundUnknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;
		private int backgroundVehicularTimeAlloted = VEHICULAR_TIMEOUT;

		// This value indicates whether the state machine should start timers or use the 'timeAlloted' counters.
		private const int USING_REALTIME_MOTION_FLAG = -1;

		// Minimum distance between user and checkpoints for reward eligibility.
		private readonly double CKPT_DISTANCE_THRESHOLD = 120;
		// Minimum time between verification attempts.
		private readonly int CHECK_NEARBY_TIMEOUT = 90 * 1000;
		private Timer checkNearbyCheckpointsTimer;

		// Counters for the number of CONSECUTIVE times an activity is obtained.
		// When one reaches THRESHOLD, switch sound setting and transition between states (when applicable).
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
			transitionGuards = new Dictionary<State, Action<int>> {
				{ State.Ineligible, new Action<int>(ineligibleStateGuards) },
				{ State.CyclingIneligible, new Action<int>(cyclingIneligibleStateGuards) },
				{ State.CyclingEligible, new Action<int>(cyclingEligibleStateGuards) },
				{ State.UnknownEligible, new Action<int>(unknownEligibleStateGuards) },
				{ State.Vehicular, new Action<int>(vehicularStateGuards) }
			};
		}


		/// <summary>
		/// Destructor called upon dereferencing (i.e. when user logs out).
		/// </summary>
		~RewardEligibilityManager() {
			timer?.Dispose();
			vehicularTimer?.Dispose();
			checkNearbyCheckpointsTimer?.Dispose();
			stateMachine = null;
		}


		/// <summary>
		/// The feedback from the motion detector goes here, passes the guard checks and goes into the state-machine.
		/// </summary>
		/// <param name="activity">Activity.</param>
		public void Input(ActivityType activity) {
			incrementCounters(activity);
			Action<int> nextAction;
			var state = stateMachine.CurrentState;
			transitionGuards.TryGetValue(state, out nextAction);
			nextAction.Invoke(USING_REALTIME_MOTION_FLAG);
		}


		/// <summary>
		/// Same as above but used sporadically by iOS devices when significant location updates occur (cell towers change).
		/// </summary>
		/// <param name="activity">Activity.</param>
		public void Input(ActivityType activity, int elapsedMilis) {
			incrementCounters(activity);
			Action<int> nextAction;
			var state = stateMachine.CurrentState;
			Debug.WriteLine($"elapsed milis: {elapsedMilis}");
			switch(state) {
				case State.CyclingIneligible:
					backgroundCyclingIneligibleTimeAlloted -= elapsedMilis;
					Debug.WriteLine($"milis remaining for cyclingEligible: {backgroundCyclingIneligibleTimeAlloted}");
					break;
				case State.UnknownEligible:
					backgroundUnknownEligibleTimeAlloted -= elapsedMilis;
					Debug.WriteLine($"milis remaining for ineligible: {backgroundUnknownEligibleTimeAlloted}");
					break;
				case State.Vehicular:
					backgroundVehicularTimeAlloted -= elapsedMilis;
					Debug.WriteLine($"milis remaining for ineligible: {backgroundVehicularTimeAlloted}");
					break;
			}

			if(backgroundCyclingIneligibleTimeAlloted < 1 || backgroundUnknownEligibleTimeAlloted < 1 || backgroundVehicularTimeAlloted < 1) {
				stateMachine.MoveNext(Command.Timeout);
				state = stateMachine.CurrentState;
				backgroundCyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
				backgroundUnknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;
				backgroundVehicularTimeAlloted = VEHICULAR_TIMEOUT;
			}

			transitionGuards.TryGetValue(state, out nextAction);
			nextAction.Invoke(elapsedMilis);
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
					if(stationaryCount == THRESHOLD && User.Instance.IsBackgroundAudioEnabled) {
						stationaryCount = 0;
						DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.StationarySoundSetting);
					}
					return;
				case ActivityType.Walking:
					walkingCount++; nonCyclingCount++; nonVehicularCount++;
					cyclingCount = runningCount = vehicularCount = stationaryCount = 0;
					if(walkingCount == THRESHOLD && User.Instance.IsBackgroundAudioEnabled) {
						walkingCount = 0;
						DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.WalkingSoundSetting);
					}
					return;
				case ActivityType.Running:
					runningCount++; nonCyclingCount++; nonVehicularCount++;
					cyclingCount = walkingCount = vehicularCount = stationaryCount = 0;
					if(runningCount == THRESHOLD && User.Instance.IsBackgroundAudioEnabled) {
						runningCount = 0;
						DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.RunningSoundSetting);
					}
					return;
				case ActivityType.Cycling:
					cyclingCount++; nonVehicularCount++;
					nonCyclingCount = walkingCount = stationaryCount = runningCount = vehicularCount = 0;
					if(cyclingCount == THRESHOLD && User.Instance.IsBackgroundAudioEnabled) {
						cyclingCount = 0;
						DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.CyclingSoundSetting);
					}
					return;
				case ActivityType.Automative:
					vehicularCount++; nonCyclingCount++;
					nonVehicularCount = stationaryCount = walkingCount = runningCount = cyclingCount = 0;
					if(vehicularCount == THRESHOLD && User.Instance.IsBackgroundAudioEnabled) {
						vehicularCount = 0;
						DependencyService.Get<ISoundPlayer>().PlaySound(User.Instance.VehicularSoundSetting);
					}
					return;
			}
		}


		void resetCounters() {
			stationaryCount = walkingCount = runningCount = cyclingCount = vehicularCount = nonCyclingCount = nonVehicularCount = 0;
			backgroundVehicularTimeAlloted = VEHICULAR_TIMEOUT;
			backgroundCyclingIneligibleTimeAlloted = CYCLING_INELIGIBLE_TIMEOUT;
			// The eligibleUnknownTimeAlloted is different, only reset when going to 'cyclingEligible' state from 'unknownEligible' and 'cyclingIneligible'.
		}


		void ineligibleStateGuards(int elapsedTime) {
			// If user starts using a bycicle, go to: 'cyclingIneligible'.
			if(cyclingCount > THRESHOLD) {
				resetCounters();
				cyclingEventStart = TimeUtil.CurrentEpochTimeSeconds();
				stateMachine.MoveNext(Command.Cycling);
				// Start timer -> If user continues using a bycicle for a certain time, go to: 'cyclingEligible'.
				if(elapsedTime == USING_REALTIME_MOTION_FLAG) {
					timer = new Timer(new TimerCallback(goToCyclingEligibleCallback), null, CYCLING_INELIGIBLE_TIMEOUT);
				}
			}
		}


		void cyclingIneligibleStateGuards(int elapsedTime) {
			// If user stops using a bycicle, go back to the start: 'ineligible'.
			if(nonCyclingCount > THRESHOLD) {
				timer.Dispose();
				resetCounters();
				// Ignore small cycling events.
				cyclingEventStart = 0;
				stateMachine.MoveNext(Command.NotCycling);
			}
		}


		void cyclingEligibleStateGuards(int elapsedTime) {
			// If user stops using a bycicle, go to: 'unknownEligible'.
			if(nonCyclingCount > THRESHOLD) {
				resetCounters();

				// Record cycling event.
				User.Instance.GetCurrentKPI().AddCyclingEvent(cyclingEventStart, TimeUtil.CurrentEpochTimeSeconds());

				stateMachine.MoveNext(Command.NotCycling);

				// User likely got off bike, give an audible congratulatory sound to let her know she is eligible.
				if(User.Instance.IsBackgroundAudioEnabled)
					DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.CongratulatorySoundSetting, 0);

				// Start a long timer where the user is still eligible for rewards even when not using a bycicle.
				// If the timer goes off (the user goes too long without using a bycicle), go back to 'ineligible'.
				if(elapsedTime == USING_REALTIME_MOTION_FLAG) {
					timer = new Timer(new TimerCallback(goToIneligibleCallback), null, UNKNOWN_ELIGIBLE_TIMEOUT);
				}
			}
		}


		void unknownEligibleStateGuards(int elapsedTime) {
			// When user leaves bike, check for nearby checkpoints/shops for reward notification every CHECK_NEARBY_TIMEOUT period.
			if(checkNearbyCheckpointsTask == null) {
				checkNearbyCheckpointsTask = new Task(() => checkNearbyCheckpoints());
				checkNearbyCheckpointsTask.Start();
			}

			// If users start using a vehicle, go to 'inAVehicle' and start a shorter timer that makes her ineligible after it fires.
			if(vehicularCount > THRESHOLD) {
				resetCounters();
				stateMachine.MoveNext(Command.InAVehicle);
				if(elapsedTime == USING_REALTIME_MOTION_FLAG) {
					vehicularTimer = new Timer(new TimerCallback(goToIneligibleCallback), null, VEHICULAR_TIMEOUT);
				}
			}
			// If user starts cycling again, go back to 'cyclingEligible'.
			if(cyclingCount > THRESHOLD) {
				resetCounters();
				cyclingEventStart = TimeUtil.CurrentEpochTimeSeconds();
				stateMachine.MoveNext(Command.Cycling);
				backgroundUnknownEligibleTimeAlloted = UNKNOWN_ELIGIBLE_TIMEOUT;
				//DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.BycicleEligibleSoundSetting);
				timer.Dispose();
			}
		}


		void vehicularStateGuards(int elapsedTime) {
			// If the user stops using a vehicle, go back to 'unknownEligible'.
			if(nonVehicularCount > THRESHOLD) {
				resetCounters();
				vehicularTimer.Dispose();
				stateMachine.MoveNext(Command.NotInAVehicle);
			}
		}


		/// <summary>
		/// When a user goes from 'cyclingEligible' to 'unknownEligible', i.e., stops cycling, 
		/// check for shops nearby with 'distance' condition to see if she is eligible for rewards.
		/// </summary>
		async Task checkNearbyCheckpoints() {
			var now = TimeUtil.CurrentEpochTimeSeconds();
			var nChallengesCompleted = 0;
			// Compare the distance between the user and the checkpoints.
			var userLocation = await GeoUtils.GetCurrentUserLocation();
			foreach(var checkpoint in User.Instance.Checkpoints) {
				var ckptLocation = new Position {
					Longitude = checkpoint.Value.Longitude,
					Latitude = checkpoint.Value.Latitude
				};
				var distance = GeoUtils.DistanceBetweenPoints(userLocation, ckptLocation);

				if(distance < CKPT_DISTANCE_THRESHOLD) {
					// Check their challenges' conditions.
					foreach(var challenge in checkpoint.Value.Challenges) {
						var createdAt = challenge.CreatedAt;
						var expiresAt = challenge.ExpiresAt;
						// For the valid challenges ...
						if(TimeUtil.IsWithinPeriod(now, createdAt, expiresAt)) {
							// Check if distance cycled meets the criteria.
							if(challenge.NeededCyclingDistance <= CycledDistanceBetween(createdAt, expiresAt)) {
								challenge.IsComplete = true;
								challenge.CompletedAt = now;
								SQLiteDB.Instance.SaveItem(challenge);
								nChallengesCompleted++;
								// Record event
								User.Instance.GetCurrentKPI().AddCheckInEvent(now, challenge.CheckpointId);
							}
						}
					}
				}
			}

			sendRewardNotificationUser(nChallengesCompleted);

			// Start a timer that waits a certain period before doing 'checkNearbyCheckpoints' again.
			checkNearbyCheckpointsTimer = new Timer(new TimerCallback(
													(obj) => { checkNearbyCheckpointsTask = null; }),
 													null,
 													CHECK_NEARBY_TIMEOUT);
		}


		public static long CycledDistanceBetween(long start, long end) {
			long res = 0;
			foreach(Trajectory t in User.Instance.Trajectories) {
				if(TimeUtil.IsWithinPeriod(t.EndTime, start, end))
					res += t.CalculateCyclingDistance();
			}
			return res;
		}


		/// <summary>
		/// Called when a user goes from 'cyclingIneligible' to 'cyclingEligible', i.e., 
		/// has been cycling for at least 1.5 mins, 
		/// check the list of challenges for those without 'distance' condition, i.e., the 'cycle to shop' challenges.
		/// </summary>
		void checkForRewards() {
			var now = TimeUtil.CurrentEpochTimeSeconds();
			var cycleToShopChallenges = User.Instance.Challenges.FindAll((x) => x.NeededCyclingDistance == 0);
			var nChallengesCompleted = cycleToShopChallenges.Count;
			foreach(var c in cycleToShopChallenges) {
				c.IsComplete = true;
				c.CompletedAt = now;
				SQLiteDB.Instance.SaveItem(c);
				// Record event
				User.Instance.GetCurrentKPI().AddCheckInEvent(now, c.CheckpointId);
			}

			sendRewardNotificationUser(nChallengesCompleted);
		}


		static void sendRewardNotificationUser(int nChallengesCompleted) {
			if(nChallengesCompleted > 0) {
				DependencyService.Get<INotificationMessage>().Send(
					 "checkForRewards",
					 Language.RewardsEarned,
					 Language.YouHaveEarned + " " + nChallengesCompleted + " " + Language.nRewardsClickToSeeWhat,
					 nChallengesCompleted
				);
			}
		}


		/// <summary>
		/// Timer callback. User is now eligible for rewards.
		/// </summary>
		void goToCyclingEligibleCallback(object state) {
			resetCounters();
			stateMachine.MoveNext(Command.Timeout);
			Task.Run(() => checkForRewards()).DoNotAwait();
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
			timer.Dispose();
			vehicularTimer.Dispose();
			if(User.Instance.IsBackgroundAudioEnabled)
				DependencyService.Get<ISoundPlayer>().PlayShortSound(User.Instance.NoLongerEligibleSoundSetting);
		}
	}
}
