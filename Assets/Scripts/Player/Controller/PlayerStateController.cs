using System.Collections.Generic;
using System.Linq;
using Soulslike.Core;
using Soulslike.Player.States;

namespace Soulslike.Player.Controller
{
    public class PlayerStateController : StateController
    {
        
        // List of available states, sorted by priority
        private readonly List<PlayerState> sortedStates;
        
        // Class constructor
        public PlayerStateController(PlayerController controller)
        {
                       
            // Construct the states
            Dictionary<StateTypes, PlayerState> states = new Dictionary<StateTypes, PlayerState>()
            {
                { StateTypes.Idle, new PlayerIdle(controller) }, // Idle state
                { StateTypes.Walking, new PlayerWalking(controller) }, // Walking State
                { StateTypes.Sprinting, new PlayerSprinting(controller) }, // Sprinting State
                { StateTypes.Rolling, new PlayerRoll(controller)}, // Rolling state
                { StateTypes.Falling , new PlayerFalling(controller) }, // Falling state
                { StateTypes.Landed,  new PlayerLanded(controller) }, // Landed state
            };
            sortedStates = states.Values.OrderByDescending(state => state.Priority).ToList();
            
            // Subscribe to the event
            StateChanged += (_, newState) => OnStateChanged(newState);
            
            // Assign the starting state
            CurrentState = states[StateTypes.Idle];
        }

        // Called every frame
        public void Update()
        {
            // Make sure there is a current state
            if (CurrentState == null) return;

            // Check if we can change the current state
            if (CurrentState.CanTransition())
                CheckStateChange();

            // Update the current state
            CurrentState.Update();
        }
        
        // Called every frame to check if the current state needs to be changed
        private void CheckStateChange()
        {
            // Loop through each state
            foreach (var state in sortedStates.Where(state => state.CanUse()).TakeWhile(state => state != CurrentState))
            {
                // Attempt to override the current state
                CurrentState = state;
                break;
            }
        }
        
        // Called when the current state is changed
        private void OnStateChanged(EntityState newState)
        {

        }
    }
}