using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace Trace {

	public enum State { Ineligible, CyclingIneligible, CyclingEligible, UnknownEligible, Vehicular }

	public enum Command { Cycling, NotCycling, Timeout, InAVehicle, NotInAVehicle }

	/// <summary>
	/// This class implements a finite-state machine that determines when a user is eligible for checkpoint rewards.
	/// The user is eligible for rewards if she uses a bycicle for a defined period of time.
	/// The input for this FSM is a motion activity tuple: (activityType, confidence).
	/// </summary>
	public class RewardEligibilityStateMachine {

		class StateTransition {
			readonly State CurrentState;
			readonly Command Command;

			public StateTransition(State currentState, Command command) {
				CurrentState = currentState;
				Command = command;
			}

			public override int GetHashCode() {
				return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
			}

			public override bool Equals(object obj) {
				var other = obj as StateTransition;
				return other != null && CurrentState == other.CurrentState && Command == other.Command;
			}
		}

		Dictionary<StateTransition, State> transitions;
		public State CurrentState { get; private set; }

		public RewardEligibilityStateMachine() {

			// Get the initial state status from persistent memory.
			if(Application.Current.Properties.ContainsKey("reward_state")) {
				var _ = Application.Current.Properties["reward_state"];
				CurrentState = (State) _;
			}
			else {
				Application.Current.Properties["reward_state"] = 0; // Ineligible
				CurrentState = (State) Application.Current.Properties["reward_state"];
			}

			// The transition table. Implemented using a dictionary.
			transitions = new Dictionary<StateTransition, State>
			{
				{ new StateTransition(State.Ineligible, Command.Cycling), State.CyclingIneligible },
				{ new StateTransition(State.CyclingIneligible, Command.NotCycling), State.Ineligible },
				{ new StateTransition(State.CyclingIneligible, Command.Timeout), State.CyclingEligible },
				{ new StateTransition(State.CyclingEligible, Command.NotCycling), State.UnknownEligible },
				{ new StateTransition(State.UnknownEligible, Command.Cycling), State.CyclingEligible },
				{ new StateTransition(State.UnknownEligible, Command.Timeout), State.Ineligible },
				{ new StateTransition(State.UnknownEligible, Command.InAVehicle), State.Vehicular },
				{ new StateTransition(State.Vehicular, Command.NotInAVehicle), State.UnknownEligible },
				{ new StateTransition(State.Vehicular, Command.Timeout), State.Ineligible }
			};
		}

		private State getNext(Command command) {
			var transition = new StateTransition(CurrentState, command);
			// If the transition is invalid, go back to 'ineligible', likely a conflict between 'vehicular' timer
			// and 'unknownEligible' timer. Either way, the user goes back to beginning.
			State nextState = State.Ineligible;
			transitions.TryGetValue(transition, out nextState);
			Debug.WriteLine("StateMachine: nextState() -> " + nextState);
#if DEBUG
			App.DEBUG_ActivityLog += "--------" + nextState + "--------\n";
#endif
			return nextState;
		}

		public State MoveNext(Command command) {
			CurrentState = getNext(command);
			Application.Current.Properties["reward_state"] = (int) CurrentState;
			return CurrentState;
		}
	}
}