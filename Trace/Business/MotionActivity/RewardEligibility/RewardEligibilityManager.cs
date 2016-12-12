using System;
using System.Collections.Generic;

namespace Trace {

	/// <summary>
	/// This class maintains the state of the several conditions (i.e. guards) that are used to determine
	/// if the state machine should transition or not. 
	/// </summary>
	public class RewardEligibilityManager {
		readonly RewardEligibilityStateMachine stateMachine;
		Dictionary<State, Action> transitionGuards;
		Timer timer;
		Timer vehicularTimer;

		// Successive count threshold for transitioning between states.
		private const int THRESHOLD = 5;
		// Timeout threshold between 'cyclingIneligible' to 'cyclingEligible' in ms. 1,5 min.
		private const int CYCLING_INELIGIBLE_TIMEOUT = 90 * 1000;
		// Timeout threshold between 'unknownEligible' to 'ineligible' in ms. 12 h.
		private const int UNKNOWN_ELIGIBLE_TIMEOUT = 12 * 60 * 60 * 1000;
		// Timeout threshold between 'inAVehicle' to 'ineligible' in ms. 5 min.
		private const int VEHICULAR_TIMEOUT = 5 * 60 * 1000;

		int cyclingCount;
		int nonCyclingCount;
		int vehicularCount;
		int nonVehicularCount;

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
			timer.Dispose();
			vehicularTimer.Dispose();
		}

		/// <summary>
		/// The feedback from the motion detector goes here, passes the guard checks and goes into the state-machine.
		/// </summary>
		/// <param name="activity">Activity.</param>
		public void Input(ActivityType activity) {
			incrementCounters(activity);
			Action nextAction;
			var state = stateMachine.CurrentState;
			transitionGuards.TryGetValue(state, out nextAction);
			nextAction.Invoke();
		}

		/// <summary>
		/// Increments the counters depending on the activity.
		/// Counters count SUCCESSIVE activities.
		/// </summary>
		/// <param name="activity">Activity.</param>
		void incrementCounters(ActivityType activity) {
			if(activity == ActivityType.Cycling) {
				cyclingCount++; nonVehicularCount++; nonCyclingCount = vehicularCount = 0;
			}
			else if(activity == ActivityType.Automative) {
				vehicularCount++; nonCyclingCount++; cyclingCount = 0;
			}
			else {
				nonCyclingCount++; nonVehicularCount++; cyclingCount = vehicularCount = 0;
			}
		}

		void resetCounters() {
			cyclingCount = nonCyclingCount = vehicularCount = nonVehicularCount = 0;
		}

		void ineligibleStateGuards() {

			// If user starts using a bycicle, go to: 'cyclingIneligible'.
			if(cyclingCount > THRESHOLD) {
				resetCounters();
				stateMachine.MoveNext(Command.Cycling);

				// Start timer -> If user continues using a bycicle for a certain time, go to: 'cyclingEligible'.
				timer = new Timer(new TimerCallback(goToCyclingEligibleCallback), null, CYCLING_INELIGIBLE_TIMEOUT);
			}
		}

		void cyclingIneligibleStateGuards() {
			// If user stops using a bycicle, go back to the start: 'ineligible'.
			if(nonCyclingCount > THRESHOLD) {
				timer.Dispose();
				resetCounters();
				stateMachine.MoveNext(Command.NotCycling);
			}
		}

		void cyclingEligibleStateGuards() {
			// If user stops using a bycicle, go to: 'unknownEligible'.
			if(nonCyclingCount > THRESHOLD) {
				resetCounters();
				stateMachine.MoveNext(Command.NotCycling);

				// Start a long timer where the user is still eligible for rewards even when not using a bycicle.
				// If the timer goes off (the user goes too long without using a bycicle), go back to 'ineligible'.
				timer = new Timer(new TimerCallback(goToIneligibleCallback), null, UNKNOWN_ELIGIBLE_TIMEOUT);
			}
		}

		void unknownEligibleStateGuards() {
			// If users start using a vehicle, go to 'inAVehicle' and start a shorter timer that makes her ineligible after it fires.
			if(vehicularCount > THRESHOLD) {
				resetCounters();
				stateMachine.MoveNext(Command.InAVehicle);
				vehicularTimer = new Timer(new TimerCallback(goToIneligibleCallback), null, VEHICULAR_TIMEOUT);
			}
			// If user starts cycling again, go back to 'cyclingEligible'.
			if(cyclingCount > THRESHOLD) {
				resetCounters();
				stateMachine.MoveNext(Command.Cycling);
				timer.Dispose();
			}
		}

		void vehicularStateGuards() {
			// If the user stops using a vehicle, go back to 'unknownEligible'.
			if(nonVehicularCount > THRESHOLD) {
				resetCounters();
				vehicularTimer.Dispose();
				stateMachine.MoveNext(Command.NotInAVehicle);
			}
		}

		/// <summary>
		/// Timer callbacks.
		/// </summary>
		void goToCyclingEligibleCallback(object state) {
			resetCounters();
			// TODO notify user is eligible for rewards.
			stateMachine.MoveNext(Command.Timeout);
		}

		void goToIneligibleCallback(object state) {
			resetCounters();
			// TODO warn user she is no longer eligible for rewards.
			stateMachine.MoveNext(Command.Timeout);
			timer.Dispose();
			vehicularTimer.Dispose();
		}
	}
}
