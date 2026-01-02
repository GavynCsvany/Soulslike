using System.Collections.Generic;
using System.Linq;
using Soulslike.Core;
using Soulslike.Player.States;
using Soulslike.Player.States.Actions;
using Soulslike.Player.States.Basic_Locomotion;
using Soulslike.Player.States.Crouch_Locomotion;
using Soulslike.Player.States.Ledge_Climbing;

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
                
                // BASE STATES //
                { StateTypes.Idle, new PlayerIdle(controller,  0) },                    // Idle state
                {StateTypes.CrouchIdle, new PlayerCrouchIdle(controller,  1) },         // Crouch idle state
                { StateTypes.Walking, new PlayerWalking(controller, 2) },               // Walking State
                { StateTypes.CrouchWalking, new PlayerCrouchWalking(controller, 3)},    // Crouch walking state
                { StateTypes.Sprinting, new PlayerSprinting(controller, 4) },           // Sprinting State
                { StateTypes.Falling , new PlayerFalling(controller, 5) },              // Falling state
                { StateTypes.Landed,  new PlayerLanded(controller, 6) },                // Landed state
                //{ StateTypes.Jumping , new PlayerJump(controller, 7)},                       // Jumping state
                { StateTypes.Rolling, new PlayerRoll(controller, 8)},                   // Rolling state
                    
                // LEDGE CLIMBING STATES // 
                { StateTypes.LedgeStart, new PlayerLedgeStart(controller, 30) },        // Ledge climb starts
                { StateTypes.LedgeEnd, new PlayerLedgeLeave(controller, 31) },          // Ledge climb ends
                { StateTypes.LedgeIdle, new PlayerLedgeIdle(controller, 20) },          // Ledge climb idle
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
                // Check if the new state has a higher priority than the current one
                if (state.Priority > CurrentState.Priority)
                {
                    // Attempt to override the current state
                    CurrentState = state;
                    break;
                }
                
                // Check if we can transition to the new state
                if (CurrentState.CanTransition())
                {
                    // Attempt to override the current state
                    CurrentState = state;
                    break;
                } 
            }
        }
        
        // Called when the current state is changed
        private void OnStateChanged(EntityState newState)
        {

        }
    }
}